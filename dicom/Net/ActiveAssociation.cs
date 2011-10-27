#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2008 Fang Yang. All rights reserved.
// 
// This file is part of dicomcs, see http://www.sourceforge.net/projects/dicom-cs
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.                                 
// 
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
// Fang Yang (yangfang@email.com)
//
// 7/22/08: Solved bug by Maarten JB van Ettinger. A deadlock has been removed (changed lines 152-153, 178-187).
#endregion

namespace org.dicomcs.net
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Threading;
	using org.dicomcs.net;
	using org.dicomcs.data;
	using org.dicomcs.util;
	using log4net;

	/// <summary>
	/// 
	/// </summary>
	public class ActiveAssociation : LF_ThreadPool.ThreadHandlerI
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public virtual int Timeout
		{
			get { return timeout; }			
			set { this.timeout = value; }			
		}
		public virtual Association Association
		{
			get { return assoc; }			
		}

		private readonly Association assoc;
		private readonly DcmServiceRegistry services;
		private readonly Hashtable rspDispatcher = new Hashtable();
		private readonly Hashtable cancelDispatcher = new Hashtable();
		private int timeout = 0;
		private bool m_released = false;
		private LF_ThreadPool m_threadPool = null;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="assoc"></param>
		/// <param name="services"></param>
		public ActiveAssociation(Association assoc, DcmServiceRegistry services)
		{
			if (assoc.State != Association.ASSOCIATION_ESTABLISHED)
				throw new SystemException("Association not esrablished - " + assoc.State);
			
			m_threadPool = new LF_ThreadPool( this );
			
			this.assoc = assoc;
			this.services = services;
			this.assoc.ActiveAssociation = this;
			this.assoc.SetThreadPool( m_threadPool );
		}
		
		/// <summary>
		/// Add DIMSE message listener
		/// </summary>
		/// <param name="msgID"></param>
		/// <param name="l"></param>
		public void  AddCancelListener(int msgID, DimseListenerI l)
		{
			cancelDispatcher.Add(msgID, l);
		}
		
		/// <summary>
		/// Start a new pooled thread for handling this active association
		/// </summary>
		public void  Start()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback( Run ) );
		}
		
		/// <summary>
		/// Send a DIMSE message
		/// </summary>
		/// <param name="rq"></param>
		/// <param name="l"></param>
		public void  Invoke(Dimse rq, DimseListenerI l)
		{
			int msgID = rq.Command.MessageID;
			int maxOps = assoc.MaxOpsInvoked;
			if (maxOps == 0)
			{
				rspDispatcher.Add(msgID, l);
			}
			else
				lock(rspDispatcher)
				{
					while (rspDispatcher.Count >= maxOps)
					{
						System.Threading.Monitor.Wait(rspDispatcher);
					}
					rspDispatcher.Add(msgID, l);
				}
			assoc.Write(rq);
		}
		
		/// <summary>
		/// Send a DIMSE message
		/// </summary>
		/// <param name="rq"></param>
		/// <returns></returns>
		public FutureRSP Invoke(Dimse rq)
		{
			FutureRSP retval = new FutureRSP();
			assoc.AddAssociationListener(retval);
			Invoke(rq, retval);
			return retval;
		}

		/// <summary>
		/// Wait on all responses)
		/// </summary>
		public void WaitOnRSP()
		{
			lock(rspDispatcher)
			{
				while (rspDispatcher.Count != 0)
				{
					System.Threading.Monitor.Wait(rspDispatcher);
				}
			}
		}
		
		/// <summary>
		/// Send association release request and release this association
		/// </summary>
		/// <param name="waitOnRSP"></param>
		public void  Release(bool waitOnRSP)
		{
			if (waitOnRSP)
			{
				WaitOnRSP();
			}
			if (!m_released)
				((Association) assoc).WriteReleaseRQ();
		}

		/// <summary>
		/// ThreadPool wrapper method for Join
		/// </summary>
		/// <param name="state"></param>
		public void Run( Object state )
		{
			m_threadPool.Join();
		}
		
		/// <summary>
		/// Run this active association
		/// </summary>
		public void  Run( LF_ThreadPool pool )
		{
			try
			{
				lock (this)
				{
					Dimse dimse = assoc.Read(timeout);

					// if Association was released
					if (dimse == null)
					{
						lock (rspDispatcher)
						{
							if (rspDispatcher.Count != 0)
							{
								rspDispatcher.Clear();
								m_released = true;
							}

							System.Threading.Monitor.Pulse(rspDispatcher);
						}

						pool.Shutdown();
						return ;
					}
			
					Command cmd = dimse.Command;
					switch (cmd.CommandField)
					{
						case Command.C_STORE_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).c_store(this, dimse);
							break;
				
						case Command.C_GET_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).c_get(this, dimse);
							break;
				
						case Command.C_FIND_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).c_find(this, dimse);
							break;
				
						case Command.C_MOVE_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).c_move(this, dimse);
							break;
				
						case Command.C_ECHO_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).c_echo(this, dimse);
							break;
				
						case Command.N_EVENT_REPORT_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).n_event_report(this, dimse);
							break;
				
						case Command.N_GET_RQ: 
							services.Lookup(cmd.RequestedSOPClassUID).n_get(this, dimse);
							break;
				
						case Command.N_SET_RQ: 
							services.Lookup(cmd.RequestedSOPClassUID).n_set(this, dimse);
							break;
				
						case Command.N_ACTION_RQ: 
							services.Lookup(cmd.RequestedSOPClassUID).n_action(this, dimse);
							break;
				
						case Command.N_CREATE_RQ: 
							services.Lookup(cmd.AffectedSOPClassUID).n_action(this, dimse);
							break;
				
						case Command.N_DELETE_RQ: 
							services.Lookup(cmd.RequestedSOPClassUID).n_delete(this, dimse);
							break;
				
						case Command.C_STORE_RSP: case Command.C_GET_RSP: case Command.C_FIND_RSP: case Command.C_MOVE_RSP: case Command.C_ECHO_RSP: case Command.N_EVENT_REPORT_RSP: case Command.N_GET_RSP: case Command.N_SET_RSP: case Command.N_ACTION_RSP: case Command.N_CREATE_RSP: case Command.N_DELETE_RSP: 
							HandleResponse(dimse);
							break;
				
						case Command.C_CANCEL_RQ: 
							HandleCancel(dimse);
							break;
				
						default: 
							throw new System.SystemException("Illegal Command: " + cmd);
				
					}
				}
			}
			catch (Exception ioe)
			{
				log.Error(ioe);
				pool.Shutdown();
			}
		}

		/// <summary>
		/// Handle DIMSE response
		/// </summary>
		/// <param name="dimse"></param>
		private void  HandleResponse(Dimse dimse)
		{
			Command cmd = dimse.Command;
			Dataset ds = dimse.Dataset; // read out dataset, if any
			int msgID = cmd.MessageIDToBeingRespondedTo;
			DimseListenerI l = null;
			if (cmd.IsPending())
			{
				l = (DimseListenerI) rspDispatcher[msgID];
			}
			else
				lock(rspDispatcher)
				{
					l = (DimseListenerI) rspDispatcher[msgID];
					rspDispatcher.Remove(msgID);
					System.Threading.Monitor.Pulse(rspDispatcher);
				}
			
			if (l != null)
				l.DimseReceived(assoc, dimse);
		}
		
		/// <summary>
		/// Handler DIMSE cancel request
		/// </summary>
		/// <param name="dimse"></param>
		private void  HandleCancel(Dimse dimse)
		{
			Command cmd = dimse.Command;
			int msgID = cmd.MessageIDToBeingRespondedTo;

			DimseListenerI l = (DimseListenerI)cancelDispatcher[msgID]; 
			cancelDispatcher.Remove(msgID);
			
			if (l != null)
				l.DimseReceived(assoc, dimse);
		}		
	}
}