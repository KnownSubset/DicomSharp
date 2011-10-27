#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002 Fang Yang. All rights reserved.
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
#endregion

namespace org.dicomcs.net
{
	using System;
	using System.IO;
	using System.Threading;
	using System.Net.Sockets;
	using System.Collections;
	using System.Reflection;
	using org.dicomcs.util;
	using log4net;

	/// <summary>
	/// Finite State Mechine of DICOM communication
	/// </summary>
	public class Fsm
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly State STA1 = null;
		private readonly State STA2 = null;
		private readonly State STA3 = null;
		private readonly State STA4 = null;
		private readonly State STA5 = null;
		private readonly State STA6 = null;
		private readonly State STA7 = null;
		private readonly State STA8 = null;
		private readonly State STA9 = null;
		private readonly State STA10 = null;
		private readonly State STA11 = null;
		private readonly State STA12 = null;
		private readonly State STA13 = null;
		private State state = null;

		private Association assoc;
		private bool requestor;
		private TcpClient s;
		private Stream stream;		
		private int tcpCloseTimeout = 500;
		private AAssociateRQ rq = null;
		private AAssociateAC ac = null;
		private AAssociateRJ rj = null;
		private AAbort aa = null;
		private AssociationListenerI assocListener = null;
		private LF_ThreadPool m_threadPool = null;

		public virtual LF_ThreadPool ReaderThreadPool
		{
			get { return m_threadPool; }
			set { m_threadPool = value; }
		}

		/// <summary>
		/// Creates a new instance of DcmULService 
		/// </summary>
		public Fsm(Association assoc, System.Net.Sockets.TcpClient s, bool requestor)
		{
			STA1 = new State1(this, Association.IDLE);
			STA2 = new State2(this, Association.AWAITING_READ_ASS_RQ);
			STA3 = new State3(this, Association.AWAITING_WRITE_ASS_RP);
			STA4 = new State4(this, Association.AWAITING_WRITE_ASS_RQ);
			STA5 = new State5(this, Association.AWAITING_READ_ASS_RP);
			STA6 = new State6(this, Association.ASSOCIATION_ESTABLISHED);
			STA7 = new State7(this, Association.AWAITING_READ_REL_RP);
			STA8 = new State8(this, Association.AWAITING_WRITE_REL_RP);
			STA9 = new State9(this, Association.RCRS_AWAITING_WRITE_REL_RP);
			STA10 = new State10(this, Association.RCAS_AWAITING_READ_REL_RP);
			STA11 = new State11(this, Association.RCRS_AWAITING_READ_REL_RP);
			STA12 = new State12(this, Association.RCAS_AWAITING_WRITE_REL_RP);
			STA13 = new State13(this, Association.ASSOCIATION_TERMINATING);
			state = STA1;

			this.assoc = assoc;
			this.requestor = requestor;
			this.s = s;
			this.stream = (Stream) s.GetStream();
			log.Info(s.ToString());
			ChangeState(requestor?STA4:STA2);
		}
		
		public void  AddAssociationListener(AssociationListenerI l)
		{
			lock(this)
			{
				assocListener = Multicaster.add(assocListener, l);
			}
		}
		
		public void  RemoveAssociationListener(AssociationListenerI l)
		{
			lock(this)
			{
				assocListener = Multicaster.Remove(assocListener, l);
			}
		}				

		internal System.Net.Sockets.TcpClient Socket()
		{
			return s;
		}
		
		internal bool IsRequestor()
		{
			return requestor;
		}
			
		internal String GetAcceptedTransferSyntaxUID(int pcid)
		{
			if (ac == null)
			{
				throw new SystemException(state.ToString());
			}
			PresContext pc = ac.GetPresContext(pcid);
			if (pc == null || pc.result() != PresContext.ACCEPTANCE)
			{
				return null;
			}
			return pc.TransferSyntaxUID;
		}
		
		internal PresContext GetAcceptedPresContext(String asuid, String tsuid)
		{
			if (ac == null)
			{
				throw new SystemException(state.ToString());
			}
			for( IEnumerator enu = rq.ListPresContext().GetEnumerator(); enu.MoveNext(); )
			{
				PresContext rqpc = (PresContext) enu.Current;
				if (asuid.Equals(rqpc.AbstractSyntaxUID))
				{
					PresContext acpc = ac.GetPresContext(rqpc.pcid());
					if (acpc != null && acpc.result() ==PresContext.ACCEPTANCE && tsuid.Equals(acpc.TransferSyntaxUID))
					{
						return acpc;
					}
				}
			}
			return null;
		}
		
		public ArrayList ListAcceptedPresContext(String asuid)
		{
			if (ac == null)
			{
				throw new SystemException(state.ToString());
			}
			ArrayList list = new ArrayList();
			for ( IEnumerator enu = rq.ListPresContext().GetEnumerator(); enu.MoveNext(); )
			{
				PresContext rqpc = (PresContext) enu.Current;
				if (asuid.Equals(rqpc.AbstractSyntaxUID))
				{
					PresContext acpc = ac.GetPresContext(rqpc.pcid());
					if (acpc != null && acpc.result() == PresContext.ACCEPTANCE)
					{
						list.Add(acpc);
					}
				}
			}
			return list;
		}
		
		public int CountAcceptedPresContext()
		{
			if (ac == null)
			{
				throw new SystemException(state.ToString());
			}
			return ac.countAcceptedPresContext();
		}
		
		private void  ChangeState(State state)
		{
			if (this.state != state)
			{
				State prev = this.state;
				this.state = state;
				state.Entry();
				if (log.IsInfoEnabled)
				{
					log.Info(state.ToString());
				}
			}
		}
		
		/// <summary>
		/// Read from network socket
		/// </summary>
		/// <param name="timeout"></param>
		/// <param name="buf"></param>
		/// <returns></returns>
		public PduI Read(int timeout, byte[] buf)
		{
			try
			{
				UnparsedPdu raw = null;

				s.ReceiveTimeout = timeout;
				try
				{
					raw = new UnparsedPdu(stream, buf);
				}
				catch (IOException e)
				{
					ChangeState(STA1);
					throw e;
				}
				return raw != null ? state.Parse(raw) : null;
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		public void  Write(AAssociateRQ rq)
		{
			FireWrite(rq);
			try
			{
				lock( stream )
				{
					state.Write(rq);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
			this.rq = rq;
		}
		
		public void  Write(AAssociateAC ac)
		{
			FireWrite(ac);
			try
			{
				lock( stream )
				{
					state.Write(ac);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
			this.ac = ac;
		}
		
		public void  Write(AAssociateRJ rj)
		{
			FireWrite(rj);
			try
			{
				lock( stream )
				{
					state.Write(rj);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		public void  Write(PDataTF data)
		{
			FireWrite(data);
			try
			{
				lock( stream )
				{
					state.Write(data);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		public void  Write(AReleaseRQ rq)
		{
			FireWrite(rq);
			try
			{
				lock( stream )
				{
					state.Write(rq);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		public void  Write(AReleaseRP rp)
		{
			FireWrite(rp);
			try
			{
				lock( stream )
				{
					state.Write(rp);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		public void  Write(AAbort abort)
		{
			FireWrite(abort);
			try
			{
				lock( stream )
				{
					state.Write(abort);
				}
			}
			catch (IOException ioe)
			{
				if (assocListener != null)
					assocListener.Error(assoc, ioe);
				throw ioe;
			}
		}
		
		internal void  FireReceived(Dimse dimse)
		{
			if (log.IsInfoEnabled)
			{
				log.Info("received " + dimse);
			}
			if (assocListener != null)
				assocListener.Received(assoc, dimse);
		}
		
		internal void  FireWrite(Dimse dimse)
		{
			if (log.IsInfoEnabled)
			{
				log.Info("sending " + dimse);
			}
			if (assocListener != null)
				assocListener.Write(assoc, dimse);
		}
		
		private void  FireWrite(PduI pdu)
		{
			if (pdu is PDataTF)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("sending " + pdu);
				}
			}
			else
			{
				if (log.IsInfoEnabled)
				{
					log.Info("sending " + pdu.ToString(log.IsDebugEnabled));
				}
			}
			if (assocListener != null)
				assocListener.Write(assoc, pdu);
		}
		
		private PduI FireReceived(PduI pdu)
		{
			if (pdu is PDataTF)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("received " + pdu);
				}
			}
			else
			{
				if (log.IsInfoEnabled)
				{
					log.Info("received " + pdu.ToString(log.IsDebugEnabled));
				}
			}
			if (assocListener != null)
				assocListener.Received(assoc, pdu);
			return pdu;
		}
		

		/// <summary>
		/// State
		/// </summary>
		internal abstract class State
		{
			protected Fsm m_fsm;
			private int type;	
		
			public virtual int Type
			{
				get { return type;}				
			}			
			internal State( Fsm fsm, int type)
			{
				m_fsm = fsm;
				this.type = type;
			}						
			public virtual bool IsOpen()
			{
				return false;
			}			
			public virtual bool CanWritePDataTF()
			{
				return false;
			}			
			public virtual bool CanReadPDataTF()
			{
				return false;
			}			
			internal virtual void  Entry()
			{
			}			
			internal virtual PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
						case 2: 
						case 3: 
						case 4: 
						case 5: 
						case 6: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
			
			internal virtual void  Write(AAssociateRQ rq)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(AAssociateAC ac)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(AAssociateRJ rj)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(PDataTF data)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(AReleaseRQ rq)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(AReleaseRP rp)
			{
				throw new SystemException();
			}
			
			internal virtual void  Write(AAbort abort)
			{
				try
				{
					abort.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA13);
			}
		}				

		/// <summary>
		/// Sta 1 - Idle
		/// </summary>
		internal class State1 : State
		{
			internal State1(Fsm fsm, int type):base(fsm, type)
			{
			}
			public override String ToString()
			{
				return "Sta 1 - Idle";
			}
			
			internal override void Entry()
			{
				if( m_fsm.ReaderThreadPool != null ) m_fsm.ReaderThreadPool.Shutdown(); // stop reading

				if (m_fsm.assocListener != null)
					m_fsm.assocListener.Close(m_fsm.assoc);

				if (Fsm.log.IsInfoEnabled)
				{
					Fsm.log.Info("closing connection - " + m_fsm.s);
				}
				try
				{
					m_fsm.stream.Close();
					m_fsm.s.Close();
				}
				catch (IOException ignore)
				{
				}
			}
			
			internal override void  Write(AAbort abort)
			{
			}
		}
		
		/// <summary>
		/// Sta 2 - Transport connection open (Awaiting A-ASSOCIATE-RQ PDU)
		/// </summary>
		internal class State2 : State
		{
			internal State2(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 2 - Transport connection open (Awaiting A-ASSOCIATE-RQ PDU)";
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
							m_fsm.FireReceived(m_fsm.rq = AAssociateRQ.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA3);
							return m_fsm.rq;
						
						case 2: 
						case 3: 
						case 4: 
						case 5: 
						case 6: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
		}
		
		/// <summary>
		/// Sta 3 - Awaiting local A-ASSOCIATE response primitive
		/// </summary>
		internal class State3 : State
		{
			internal State3(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 3 - Awaiting local A-ASSOCIATE response primitive";
			}
			
			internal override void  Write(AAssociateAC ac)
			{
				try
				{
					ac.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA6);
			}
			
			internal override void  Write(AAssociateRJ rj)
			{
				try
				{
					rj.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA13);
			}
		}
		
		/// <summary>
		/// Sta 4 - Awaiting transport connection opening to complete
		/// </summary>
		internal class State4 : State
		{
			internal State4(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 4 - Awaiting transport connection opening to complete";
			}
			
			internal override void  Write(AAssociateRQ rq)
			{
				try
				{
					rq.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA5);
			}
			
			internal override void  Write(AAbort abort)
			{
				m_fsm.ChangeState(m_fsm.STA1);
			}
		}
		
		/// <summary>
		/// Sta 5 - Awaiting A-ASSOCIATE-AC or A-ASSOCIATE-RJ PDU
		/// </summary>
		internal class State5 : State
		{
			internal State5(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 5 - Awaiting A-ASSOCIATE-AC or A-ASSOCIATE-RJ PDU";
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 2: 
							m_fsm.FireReceived(m_fsm.ac = AAssociateAC.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA6);
							return m_fsm.ac;
						
						case 3: 
							m_fsm.FireReceived(m_fsm.rj = AAssociateRJ.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA13);
							return m_fsm.rj;
						
						case 4: case 5: case 6: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
		}

		/// <summary>
		/// Sta 6 - Association established and Ready for data transfer
		/// </summary>
		internal class State6 : State
		{
			internal State6(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 6 - Association established and Ready for data transfer";
			}
			
			public override bool IsOpen()
			{
				return true;
			}
			
			public override bool CanWritePDataTF()
			{
				return true;
			}
			
			public override bool CanReadPDataTF()
			{
				return true;
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
						case 2: 
						case 3: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 4: 
							return m_fsm.FireReceived(PDataTF.Parse(raw));
						
						case 5: 
							PduI pdu = m_fsm.FireReceived(AReleaseRQ.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA8);
							return pdu;
						
						case 6: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
			
			internal override void  Write(PDataTF tf)
			{
				try
				{
					tf.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
			}
			
			internal override void  Write(AReleaseRQ rq)
			{
				try
				{
					m_fsm.ChangeState(m_fsm.STA7);
					rq.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
			}
		}

		/// <summary>
		/// Sta 7 - Awaiting A-RELEASE-RP PDU
		/// </summary>
		internal class State7 : State
		{
			internal State7(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 7 - Awaiting A-RELEASE-RP PDU";
			}
			
			public override bool CanReadPDataTF()
			{
				return true;
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
						case 2:
						case 3: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 4: 
							return m_fsm.FireReceived(PDataTF.Parse(raw));
						
						case 5: 
							PduI pdu = m_fsm.FireReceived(AReleaseRQ.Parse(raw));
							m_fsm.ChangeState(m_fsm.requestor?m_fsm.STA9:m_fsm.STA10);
							return pdu;
						
						case 6: 
							m_fsm.FireReceived(pdu = AReleaseRP.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return pdu;
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
		}

		/// <summary>
		/// Sta 8 - Awaiting local A-RELEASE response primitive
		/// </summary>
		internal class State8 : State
		{
			internal State8(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 8 - Awaiting local A-RELEASE response primitive";
			}
			
			public override bool CanWritePDataTF()
			{
				return true;
			}
			
			internal override void  Write(PDataTF tf)
			{
				try
				{
					tf.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
			}
			
			internal override void  Write(AReleaseRP rp)
			{
				try
				{
					rp.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA13);
			}
		}


		/// <summary>
		/// Sta 9 - Release collision requestor side; awaiting A-RELEASE response
		/// </summary>
		internal class State9 : State
		{
			internal State9(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 9 - Release collision requestor side; awaiting A-RELEASE response";
			}
			
			internal override void  Write(AReleaseRP rp)
			{
				try
				{
					rp.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA11);
			}
		}

		/// <summary>
		/// Sta 10 - Release collision acceptor side; awaiting A-RELEASE response
		/// </summary>
		internal class State10 : State
		{
			internal State10(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 10 - Release collision acceptor side; awaiting A-RELEASE response";
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
						case 2: 
						case 3: 
						case 4: 
						case 5: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 6: 
							PduI pdu = m_fsm.FireReceived(AReleaseRP.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA12);
							return pdu;
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
		}

		/// <summary>
		/// Sta 11 - Release collision requestor side; awaiting A-RELEASE-RP PDU
		/// </summary>
		internal class State11 : State
		{
			internal State11(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 11 - Release collision requestor side; awaiting A-RELEASE-RP PDU";
			}
			
			internal override PduI Parse(UnparsedPdu raw)
			{
				try
				{
					switch (raw.GetType())
					{
						case 1: 
						case 2: 
						case 3: 
						case 4: 
						case 5: 
							throw new PduException("Unexpected " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));
						
						case 6: 
							PduI pdu = m_fsm.FireReceived(AReleaseRP.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return pdu;
						
						case 7: 
							m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
							m_fsm.ChangeState(m_fsm.STA1);
							return m_fsm.aa;
						
						default: 
							throw new PduException("Unrecognized " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));						
					}
				}
				catch (PduException ule)
				{
					try
					{
						Write(ule.AAbort);
					}
					catch (System.Exception ignore)
					{
					}
					throw ule;
				}
			}
		}

		/// <summary>
		/// Sta 12 - Release collision acceptor side; awaiting A-RELEASE-RP PDU
		/// </summary>
		internal class State12 : State
		{
			internal State12(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 12 - Release collision acceptor side; awaiting A-RELEASE-RP PDU";
			}
			
			internal override void  Write(AReleaseRP rp)
			{
				try
				{
					rp.WriteTo(m_fsm.stream);
				}
				catch (IOException e)
				{
					m_fsm.ChangeState(m_fsm.STA1);
					throw e;
				}
				m_fsm.ChangeState(m_fsm.STA13);
			}
		}

		/// <summary>
		/// Sta 13 - Awaiting Transport Connection Close Indication
		/// </summary>
		internal class State13 : State
		{
			internal void TimeoutIt( Object state )
			{
				NDC.Push(m_fsm.assoc.Name);
				m_fsm.ChangeState(m_fsm.STA1);
				NDC.Pop();
			}

			internal State13(Fsm fsm, int state):base(fsm, state)
			{
			}
			public override String ToString()
			{
				return "Sta 13 - Awaiting Transport Connection Close Indication";
			}
			
			internal override void  Entry()
			{
				if( m_fsm.ReaderThreadPool != null )
					m_fsm.ReaderThreadPool.Shutdown();

				Timer timer = new Timer(new TimerCallback(TimeoutIt), null, m_fsm.tcpCloseTimeout, Timeout.Infinite);
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// Properties
		///////////////////////////////////////////////////////////////////////
		
		public virtual int TCPCloseTimeout
		{
			get { return tcpCloseTimeout; }			
			set
			{
				if (value < 0)
				{
					throw new System.ArgumentException("tcpCloseTimeout:" + value);
				}
				this.tcpCloseTimeout = value;
			}			
		}
		public virtual int GetState()
		{
			return state.Type;
		}
		public virtual String StateAsString
		{
			get { return state.ToString(); }
			
		}
		public virtual AAssociateRQ AAssociateRQ
		{
			get { return rq; }			
		}
		public virtual String CallingAET
		{
			get
			{
				if (rq == null)
				{
					throw new SystemException(state.ToString());
				}
				return rq.CallingAET;
			}			
		}
		public virtual String CalledAET
		{
			get
			{
				if (rq == null)
				{
					throw new SystemException(state.ToString());
				}
				return rq.CalledAET;
			}			
		}
		public virtual AAssociateAC AAssociateAC
		{
			get { return ac; }			
		}
		public virtual AAssociateRJ AAssociateRJ
		{
			get { return rj; }			
		}
		public virtual AAbort AAbort
		{
			get { return aa; }
		}
		public virtual int WriteMaxLength
		{
			get
			{
				if (ac == null || rq == null)
				{
					throw new SystemException(state.ToString());
				}
				return requestor?ac.MaxPduLength:rq.MaxPduLength;
			}			
		}
		public virtual int ReadMaxLength
		{
			get
			{
				if (ac == null || rq == null)
				{
					throw new SystemException(state.ToString());
				}
				return requestor?rq.MaxPduLength:ac.MaxPduLength;
			}			
		}
		public virtual int MaxOpsInvoked
		{
			get
			{
				if (ac == null)
				{
					throw new SystemException(state.ToString());
				}
				AsyncOpsWindow aow = ac.AsyncOpsWindow;
				if (aow == null)
					return 1;
				return requestor?aow.MaxOpsInvoked:aow.MaxOpsPerformed;
			}			
		}
		public virtual int MaxOpsPerformed
		{
			get
			{
				if (ac == null)
				{
					throw new SystemException(state.ToString());
				}
				AsyncOpsWindow aow = ac.AsyncOpsWindow;
				if (aow == null)
					return 1;
				return requestor?aow.MaxOpsPerformed:aow.MaxOpsInvoked;
			}			
		}
	}
}