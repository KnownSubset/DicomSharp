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
// 7/22/08: Solved bug by Maarten JB van Ettinger. A exception has been removed (changed line 294).
#endregion

namespace org.dicomcs.net
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Threading;
	using org.dicomcs.data;
	using org.dicomcs.net;
	using org.dicomcs.util;
	
	/// <summary>
	/// DIMSE message reader
	/// </summary>
	public class DimseReader
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private Fsm fsm;
		private int timeout = 0;
		
		private DcmObjectFactory dcmObjFact = DcmObjectFactory.Instance;
		private PDataTF pDataTF = null;
		private PDataTF.PDV pdv = null;
		private Command cmd = null;
		private byte[] buf = null;
		private ActiveAssociation activeAssociation = null;
		private LF_ThreadPool m_threadPool = null;

		/// <summary>
		/// Creates a new instance of DimseReader 
		/// </summary>
		public DimseReader(Fsm fsm)
		{
			this.fsm = fsm;
		}
		
		public virtual ActiveAssociation ActiveAssociation
		{
			get { return activeAssociation; }
			set { activeAssociation = value; }			
		}
		
		public virtual LF_ThreadPool ReaderThreadPool
		{
			get { return m_threadPool; }
			set { m_threadPool = value; }
		}

		/// <summary>
		/// Read DIMSE message from the current association
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public Dimse Read(int timeout)
		{
			lock(this)
			{
				this.timeout = timeout;
				if (!NextPDV())
				{
					return null;
				}
				if (!pdv.cmd())
				{
					Abort("Command PDV expected, but received " + pdv);
				}
				int pcid = pdv.pcid();
				String tsUID = fsm.GetAcceptedTransferSyntaxUID(pcid);
				if (tsUID == null)
				{
					Abort("No Presentation Context negotiated with pcid:" + pcid);
				}
				Stream ins = new PDataTFInputStream(this, pdv.InputStream);
				cmd = dcmObjFact.NewCommand();
				bool ds = false;
				try
				{
					cmd.Read(ins);
					ds = cmd.HasDataset();
				}
				catch (ArgumentException e)
				{
					Abort(e.Message);
				}
				catch (DcmValueException e)
				{
					Abort(e.Message);
				}
				finally
				{
					ins.Close();
					ins= null;
				}

				if (ds)
				{
					if (!NextPDV())
					{
						throw new EndOfStreamException("Association released during receive of DIMSE");
					}
					if (pdv.cmd())
					{
						Abort("Data PDV expected, but received " + pdv);
					}
					if (pcid != pdv.pcid())
					{
						Abort("Mismatch between Command PDV pcid: " + pcid + " and " + pdv);
					}
					ins = new PDataTFInputStream(this, pdv.InputStream);
				}
				else
				{
					// no Dataset
					// if no Data Fragment
					ForkNextReadNext();
				}
				Dimse retval = new Dimse(pcid, tsUID, cmd, ins);
				fsm.FireReceived(retval);
				return retval;
			}
		}
		
		/// <summary>
		/// Fire up a new reading thread for this association
		/// </summary>
		private void  ForkNextReadNext()
		{
			if (cmd.IsRequest())
			{
				switch (cmd.CommandField)
				{
					case Command.C_GET_RQ: 
					case Command.C_FIND_RQ: 
					case Command.C_MOVE_RQ: 
					case Command.C_CANCEL_RQ: 
						break;
					
					default: 
						// no need for extra thread in syncronous mode
						if (fsm.MaxOpsPerformed == 1)
							return ;

						break;
				}
			}
			m_threadPool.PromoteNewLeader();
			//log.Debug( "~~~~~ Spawning new thread for reading socket for this association ......" );
			//ThreadPool.QueueUserWorkItem(new WaitCallback(activeAssociation.Run));
		}
		
		/// <summary>
		/// Read next available data stream from this association
		/// </summary>
		/// <returns></returns>
		private Stream NextStream()
		{
			if (pdv != null && pdv.last())
			{
				// if last Data Fragment
				if (!pdv.cmd())
				{
					ForkNextReadNext();
				}
				return null;
			}
			if (!NextPDV())
			{
				throw new EndOfStreamException("Association released during receive of DIMSE");
			}
			return pdv.InputStream;
		}
		
		/// <summary>
		/// Read next DICOM PDV
		/// </summary>
		/// <returns></returns>
		private bool NextPDV()
		{
			bool hasPrev = pdv != null && !pdv.last();
			bool prevCmd = hasPrev && pdv.cmd();
			int prevPcid = hasPrev?pdv.pcid():0;
			while (pDataTF == null || (pdv = pDataTF.ReadPDV()) == null)
			{
				if (!NextPDataTF())
				{
					return false;
				}
			}
			if (hasPrev && (prevCmd != pdv.cmd() || prevPcid != pdv.pcid()))
			{
				Abort("Mismatch of following PDVs: " + pdv);
			}
			return true;
		}
		
		private void  Abort(String msg)
		{
			AAbort aa = new AAbort(AAbort.SERVICE_USER, 0);
			fsm.Write(aa);
			throw new PduException(msg, aa);
		}
		
		/// <summary>
		/// Read next DICOM PDU
		/// </summary>
		/// <returns></returns>
		private bool NextPDataTF()
		{
			if (buf == null)
			{
				buf = new byte[fsm.ReadMaxLength + 6];
			}
			PduI pdu = fsm.Read(timeout, buf);
			if (pdu is PDataTF)
			{
				pDataTF = (PDataTF) pdu;
				return true;
			}
			if (pdu is AReleaseRP)
			{
				return false;
			}
			if (pdu is AReleaseRQ)
			{
				fsm.Write(AReleaseRP.Instance);
				return false;
			}
			if( pdu == null )
				return false;

			throw new PduException("Received " + pdu, (AAbort) pdu);
		}
		
		/// <summary>
		/// PDU InputStream
		/// </summary>
		private class PDataTFInputStream : MemoryStream
		{
			private DimseReader reader;
			private Stream ins;
			
			internal PDataTFInputStream(DimseReader reader, Stream ins)
			{
				this.reader = reader;
				this.ins = ins;
			}

			public int Available()
			{
				if( ins == null )
				{
					return 0; 
				}
				return (int)(ins.Length - ins.Position);
			}

			public override int ReadByte()
			{
				if( ins == null )
				{
					return -1;
				}
				
				int c = ins.ReadByte();

				if( c == -1 )
				{
					ins = reader.NextStream();
					return ins == null ? -1 : ins.ReadByte();
				}
				return c;
			}
						
			public override int Read(byte[] b, int off, int len)
			{
				if (ins == null)
				{
					return -1;
				}
				else if (b == null)
				{
					throw new System.NullReferenceException();
				}
				else if ((off < 0) || (off > b.Length) || (len < 0) || ((off + len) > b.Length) || ((off + len) < 0))
				{
					throw new System.IndexOutOfRangeException();
				}
				else if (len == 0)
				{
					return 0;
				}
				
				int n = ins.Read( b, off, len);

				if (n < 0)
					n = 0;
				
				off += n;

				while (n < len)
				{
					ins = reader.NextStream();

					if (ins == null)
						return n == 0 ? -1 : n;

					int n2 = ins.Read( b, off, len - n);

					if (n2 < 0)
						break;
					
					n += n2;
					off += n2;
				}

				return n;
			}
			
			public override void  Close()
			{
				while (ins != null)
				{
					ins= reader.NextStream();
				}
			}
		}
	}
}