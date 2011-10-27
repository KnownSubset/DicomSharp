#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002,2010 by TIANI MEDGRAPH AG. All rights reserved.
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
// 7/22/08: Changed by Maarten JB van Ettinger. Added function that will read in all elements (added lines 409-413).
#endregion

namespace org.dicomcs.data
{
	using System;
	using System.Text;
	using System.IO;
	using System.Reflection;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	public class DcmParser
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType);

		private const uint UNDEFINED_TAG = 0x00000000;
		private const uint TS_ID_TAG = 0x00020010;
		private const uint ITEM_TAG = 0xFFFEE000;
		private const uint ITEM_DELIMITATION_ITEM_TAG = 0xFFFEE00D;
		private const uint SEQ_DELIMITATION_ITEM_TAG = 0xFFFEE0DD;
		
		private byte[] b0 = new byte[0];
		private byte[] b12 = new byte[12];
		private ByteBuffer bb12 = null;
		private DcmDecodeParam decodeParam = DcmDecodeParam.IVR_LE;
		private int maxAlloc = 0x20000000; // 512MB
		
		private BinaryReader m_ins = null;
		private DcmHandlerI handler = null;
		
		private uint rTag = UNDEFINED_TAG;
		private int rVR = -1;
		private int rLen = -1;
		private long rPos = 0L;
		private bool eof = false;
		private String tsUID = null;
		
		private MemoryStream unBuf = null;

		public virtual Stream InputStream
		{
			get { return m_ins.BaseStream; }			
		}
		public virtual uint ReadTag
		{
			get { return rTag; }			
		}
		public virtual int ReadVR
		{
			get { return rVR; }			
		}
		public virtual int ReadLength
		{
			get { return rLen; }			
		}
		public virtual long StreamPosition
		{
			get { return rPos; }			
		}
		public virtual DcmHandlerI DcmHandler
		{
			set { this.handler = value; }			
		}		
		public virtual DcmDecodeParam DcmDecodeParam
		{
			get { return decodeParam; }			
			set 
			{ 
				if (log.IsDebugEnabled)
				{
					log.Debug(value.ToString());
				}
				if (value.deflated != decodeParam.deflated)
				{
					if (!value.deflated)
						throw new NotSupportedException("Cannot remove Inflater");
					else
					{
						throw new NotSupportedException("Cannot remove Deflater");
					}
				}
				bb12.SetOrder(value.byteOrder);
				decodeParam = value;
			}			
		}				
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ins"></param>
		public DcmParser(Stream ins)
		{
			bb12 = ByteBuffer.Wrap(b12, ByteOrder.LITTLE_ENDIAN);
			this.m_ins = new BinaryReader(ins);
		}
		
		public void  Seek(long pos)
		{
			throw new NotSupportedException();
		}
		
		public bool HasSeenEOF()
		{
			return eof;
		}
				
		private String LogMsg()
		{
			return "rPos:" + rPos + " " + Tags.ToHexString(rTag) + " " + VRs.ToString(rVR) + " #" + rLen;
		}
		
		public FileFormat DetectFileFormat()
		{
			FileFormat retval = null;
			retval = DetectFileFormat( m_ins.BaseStream );
			if (log.IsDebugEnabled)
				log.Debug("detect " + retval);
			return retval;
		}
		
		/// <summary>
		/// Test if this is Explicit VR format
		/// </summary>
		/// <param name="decodeParam"></param>
		/// <returns></returns>
		private int TestEVRFormat(DcmDecodeParam decodeParam)
		{
			DcmDecodeParam = decodeParam;
			ParseHeader();
			if (rVR == VRs.GetVR(rTag))
			{
				if ((rTag>>16) == 2)
					return 0;
				if (rTag >= 0x00080000)
					return 1;
			}
			return -1;
		}
		
		/// <summary>
		/// Test if this is Implicit VR format
		/// </summary>
		/// <param name="decodeParam"></param>
		/// <returns></returns>
		private int TestIVRFormat(DcmDecodeParam decodeParam)
		{
			DcmDecodeParam = decodeParam;
			ParseHeader();
			if (rVR != VRs.UN && rLen <= 64)
			{
				if (rTag >= 0x00050000)		// Should be 0x00080000
					return 1;
				if ((rTag>>16) == 2)
					return 0;
				
			}
			return -1;
		}
		
		private FileFormat DetectFileFormat( Stream ins )
		{
			long mark = ins.Position; // valid only for 144 bytes
			
			try
			{
				switch (TestIVRFormat(DcmDecodeParam.IVR_LE))
				{
					case 0: 
						return FileFormat.IVR_LE_FILE_WO_PREAMBLE;					
					case 1: 
						return FileFormat.ACRNEMA_STREAM;					
				}

				ins.Position = mark;
				switch (TestEVRFormat(DcmDecodeParam.EVR_LE))
				{
					case 0: 
						return FileFormat.DICOM_FILE_WO_PREAMBLE;					
					case 1: 
						return FileFormat.EVR_LE_STREAM;					
				}

				ins.Position = mark;
				switch (TestEVRFormat(DcmDecodeParam.EVR_BE))
				{
					case 0: 
						return FileFormat.EVR_BE_FILE_WO_PREAMBLE;					
					case 1: 
						return FileFormat.EVR_BE_STREAM;					
				}

				ins.Position = mark;
				switch (TestIVRFormat(DcmDecodeParam.IVR_BE))
				{
					case 0: 
						return FileFormat.IVR_BE_FILE_WO_PREAMBLE;					
					case 1: 
						return FileFormat.IVR_BE_STREAM;					
				}

				Stream fs = m_ins.BaseStream;
				Int64 pos = mark;

				ins.Position = mark;
				pos = fs.Seek(128, SeekOrigin.Current) - pos;   //skip 128 bytes private header
				if (pos != 128 || m_ins.ReadByte() != 'D' || m_ins.ReadByte() != 'I' || m_ins.ReadByte() != 'C' || m_ins.ReadByte() != 'M')
					return null;  //bad code, what happens first?
				if (TestEVRFormat(DcmDecodeParam.EVR_LE) == 0)
					return FileFormat.DICOM_FILE;

				ins.Position = mark;
				pos = fs.Seek(132, SeekOrigin.Current) - mark;
				if (pos != 132)
					return null;
				if (TestIVRFormat(DcmDecodeParam.IVR_LE) == 0)
					return FileFormat.IVR_LE_FILE;

				ins.Position = mark;
				pos = fs.Seek(132, SeekOrigin.Current) - mark;
				if (pos != 132)
					return null;
				if (TestEVRFormat(DcmDecodeParam.EVR_BE) == 0)
					return FileFormat.EVR_BE_FILE;
				
				ins.Position = mark;
				pos = fs.Seek(132, SeekOrigin.Current) - mark;
				if (pos != 132)
					return null;
				if (TestEVRFormat(DcmDecodeParam.IVR_BE) == 0)
					return FileFormat.IVR_BE_FILE;
			}
			finally
			{
				ins.Position = mark;
			}
			throw new DcmParseException("Unknown Format");
		}
		
		public int ParseHeader()
		{
			eof = false;
			try
			{
				b12[0] = (byte) m_ins.ReadByte();
			}
			catch (EndOfStreamException ex)
			{
				eof = true;
				log.Debug("Detect EOF");
				return -1;
			}

			m_ins.BaseStream.Read(b12, 1, 7);
			rPos += 8;
			rTag = (uint) ((bb12.ReadInt16(0) << 16) | (bb12.ReadInt16(2) & 0xffff));
			int retval = 8;
			switch (rTag)
			{
				case ITEM_TAG: 
				case ITEM_DELIMITATION_ITEM_TAG: 
				case SEQ_DELIMITATION_ITEM_TAG: 
					rVR = VRs.NONE;
					rLen = bb12.ReadInt32(4);
					break;
				
				default: 
					if (!decodeParam.explicitVR)
					{
						rVR = VRs.GetVR(rTag);
						rLen = bb12.ReadInt32(4);
					}
					else
					{
						rVR = (bb12.ReadByte(4) << 8) | (bb12.ReadByte(5) & 0xff);
						if (VRs.IsLengthField16Bit(rVR))
						{
							rLen = bb12.ReadInt16(6) & 0xffff;
						}
						else
						{
							m_ins.BaseStream.Read( b12, 8, 4);
							rPos += 4;
							rLen = bb12.ReadInt32(8);
							retval = 12;
						}
					}
					break;
				
			}
			if (unBuf != null)
				unBuf.Write(b12, 0, retval);
			if (log.IsDebugEnabled)
				log.Debug(LogMsg());
			return retval;
		}
		
		private byte[] ParsePreamble()
		{
			log.Debug("rPos:" + rPos);
			
			byte[] b128 = new byte[128];
			m_ins.BaseStream.Read(b128, 0, 128);
			rPos += 128;
			m_ins.BaseStream.Read(b12, 0, 4);
			rPos += 4;
			if (b12[0] != (byte) 'D' || b12[1] != (byte) 'I' || b12[2] != (byte) 'C' || b12[3] != (byte) 'M')
				throw new DcmParseException("Missing DICM Prefix");
			
			return b128;
		}
		
		public long ParseFileMetaInfo(bool preamble, DcmDecodeParam param)
		{
			rPos = 0L;
			byte[] data = preamble?ParsePreamble():null;
			if (handler != null)
				handler.StartFileMetaInfo(data);
			
			DcmDecodeParam = param;
			ParseGroup(2);
			if (handler != null)
				handler.EndFileMetaInfo();
			return rPos;
		}
		
		public long ParseFileMetaInfo()
		{
			return ParseFileMetaInfo(true, DcmDecodeParam.EVR_LE);
		}
		
		public long ParseCommand()
		{
			if (handler != null)
				handler.StartCommand();
			
			DcmDecodeParam = DcmDecodeParam.IVR_LE;
			long read = ParseGroup(0);
			if (handler != null)
				handler.EndCommand();
			return read;
		}
		
		private long ParseGroup(uint groupTag)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("Parse group " + groupTag);
			}
			if (handler != null)
				handler.DcmDecodeParam = decodeParam;
			
			long rPos0 = rPos;
			int hlen = ParseHeader();

			// Group length is optional so we should not throw exception here
			if (hlen != 8 || (rTag>>16) != groupTag || rVR != VRs.UL || rLen != 4)
				throw new DcmParseException("hlen=" + hlen + ", " + LogMsg());
			
			m_ins.BaseStream.Read(b12, 0, 4);
			rPos += 4;
			if (handler != null)
			{
				handler.StartElement(rTag, rVR, rPos0);
				byte[] b4 = new byte[4];
				Array.Copy(b12, 0, b4, 0, 4);
				handler.Value(b4, 0, 4);
				handler.EndElement();
			}
			return DoParse(UNDEFINED_TAG, bb12.ReadInt32(0)) + 12;
		}

		public long ParseDataset(DcmDecodeParam param, uint stopTag)
		{
			DcmDecodeParam = param;
			if (handler != null)
			{
				handler.StartDataset();
				handler.DcmDecodeParam = decodeParam;
			}
			long read = DoParse(stopTag, -1);
			if (handler != null)
				handler.EndDataset();
			return read;
		}
		
		public long ParseDcmFile(FileFormat format)
		{
			return ParseDcmFile(format, 0xffffffff);
		}
		
		public long ParseDcmFile(FileFormat format, uint stopTag)
		{
			if (format == null)
				format = DetectFileFormat();
			if (handler != null)
				handler.StartDcmFile();
			DcmDecodeParam param = format.decodeParam;
			rPos = 0L;
			if (format.hasFileMetaInfo)
			{
				tsUID = null;
				ParseFileMetaInfo(format.hasPreamble, format.decodeParam);
				if (tsUID == null)
					log.Warn("Missing Transfer Syntax UID in FMI");
				else
					param = DcmDecodeParam.ValueOf(tsUID);
			}
			ParseDataset(param, stopTag);
			if (handler != null)
				handler.EndDcmFile();
			return rPos;
		}
		
		public long ParseItemDataset()
		{
			m_ins.BaseStream.Read(b12, 0, 8);
			rPos += 8;
			uint itemtag = (uint)( (bb12.ReadInt16(0) << 16) | (bb12.ReadInt16(2) & 0xffff));
			int itemlen = bb12.ReadInt32(4);
			if (itemtag == SEQ_DELIMITATION_ITEM_TAG)
			{
				if (itemlen != 0)
				{
					throw new DcmParseException("(fffe,e0dd), Length:" + itemlen);
				}
				return -1L;
			}
			if (itemtag != ITEM_TAG)
			{
				throw new DcmParseException(Tags.ToHexString(itemtag));
			}
			if (log.IsDebugEnabled)
			{
				log.Debug("rpos:" + (rPos - 8) + ",(fffe,e0dd)");
			}
			if (handler != null)
			{
				handler.StartDataset();
			}
			long lread;
			if (itemlen == -1)
			{
				lread = DoParse(ITEM_DELIMITATION_ITEM_TAG, itemlen);
				if (rTag != ITEM_DELIMITATION_ITEM_TAG || rLen != 0)
					throw new DcmParseException(LogMsg());
			}
			else
			{
				lread = DoParse(UNDEFINED_TAG, itemlen);
			}
			if (handler != null)
				handler.EndDataset();
			return 8 + lread;
		}
		
		/// <summary>
		/// Real parsing
		/// </summary>
		/// <param name="stopTag"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private long DoParse(uint stopTag, int length)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("rpos:" + rPos + ",stopTag:" + Tags.ToHexString(stopTag) + ",length:" + length);
			}
			long lread = 0;
			if (length != 0)
			{
				long llen = length & 0xffffffffL; 
				if ((llen & 1) != 0)
					llen++;

				do 
				{
					long rPos0 = rPos;
					int hlen = ParseHeader();
					if (hlen == -1)
					{
						if (length != -1)
							throw new EndOfStreamException();					
						goto loop_brk;
					}
					lread += hlen;
					if (stopTag == rTag)
					{
						// solves a bug with some files, but might be completly wrong
						if ((rLen == -1)
						&&	(lread > hlen))
							rLen = 0;

						goto loop_brk;
					}
					if (handler != null && unBuf == null && rTag != ITEM_DELIMITATION_ITEM_TAG)
						handler.StartElement(rTag, rVR, rPos0);
					
					if (rLen == -1 || rVR == VRs.SQ)
					{
						switch (rVR)
						{
							case VRs.SQ: 
							case VRs.OB: 
							case VRs.OW: 
							case VRs.UN: 
								break;
							
							default: 
								throw new DcmParseException(LogMsg());
						}
						lread += ParseSequence(rVR, rLen);
					}
					else
					{
						readValue();
						lread += rLen;
					}
					if (handler != null && unBuf == null)
						handler.EndElement();
				}
				while (length == -1 || lread < llen);
loop_brk: ;
				
				if (length != -1 && lread > llen)
					throw new DcmParseException(LogMsg() + ", Read: " + lread + ", Length: " + llen);
			}
			return lread;
		}
				
		/// <summary>
		/// Parse Sequence
		/// </summary>
		/// <param name="vr"></param>
		/// <param name="sqLen"></param>
		/// <returns></returns>
		private long ParseSequence(int vr, int sqLen)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("rPos:" + rPos + "," + VRs.ToString(vr) + " #" + sqLen);
			}
			if (handler != null && unBuf == null)
				handler.StartSequence(sqLen);
			long lread = 0;
			if (sqLen != 0)
			{
				long llen = sqLen & 0xffffffffL;
				int id = 0; 
				do 
				{
					m_ins.BaseStream.Read(b12, 0, 8);
					rPos += 8;
					if (unBuf != null)
						unBuf.Write(b12, 0, 8);
					lread += 8;
					uint itemtag = (uint)( (bb12.ReadInt16(0) << 16) | (bb12.ReadInt16(2) & 0xffff));
					int itemlen = bb12.ReadInt32(4);
					switch (itemtag)
					{
						case SEQ_DELIMITATION_ITEM_TAG: 
							if (sqLen != -1)
							{
								log.Warn("Unexpected Sequence Delimination Item" + " (fffe,e0dd) for Sequence with explicit length: " + sqLen);
							}
							// solves a bug with some files, but might be completly wrong
							if ((itemlen != 0)
							&&  (lread <= 0))
								throw new DcmParseException("(fffe,e0dd), Length:" + itemlen);
							goto loop1_brk;
						
						case ITEM_TAG: 
							lread += ParseItem(++id, vr, itemlen);
							break;
						
						default: 
							throw new DcmParseException(Tags.ToHexString(itemtag));
						
					}
				}
				while (sqLen == -1 || lread < llen);
loop1_brk: ;
				
				if (sqLen != -1 && lread > llen)
					throw new DcmParseException(LogMsg() + ", Read: " + lread + ", Length: " + llen);
			}
			//        rLen = sqLen; // restore rLen value
			if (handler != null && unBuf == null)
				handler.EndSequence(sqLen);
			return lread;
		}
		
		private long ParseItem(int id, int vr, int itemlen)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("rPos:" + rPos + "," + VRs.ToString(vr) + " #" + itemlen);
			}
			switch (vr)
			{
				case VRs.SQ: 
					return ParseSQItem(id, itemlen);
				
				case VRs.UN: 
					if (itemlen == -1)
						return ParseUNItem(id, itemlen);
					goto case VRs.OB;
				
				case VRs.OB: 
				case VRs.OW: 
					return readFragment(id, itemlen);
				
				default: 
					throw new System.SystemException(LogMsg());
				
			}
		}
		
		private long ParseUNItem(int id, int itemlen)
		{
			long retval;
			if (unBuf != null)
				retval = ParseSQItem(id, itemlen);
			else
			{
				long rPos0 = rPos;
				unBuf = new MemoryStream();
				DcmDecodeParam tmpDecodeParam = decodeParam;
				try
				{
					DcmDecodeParam = DcmDecodeParam.IVR_LE;
					bb12.SetOrder(ByteOrder.LITTLE_ENDIAN);
					retval = ParseSQItem(id, itemlen);
					if (handler != null)
					{
						handler.Fragment(id, rPos0 - 8, unBuf.ToArray(), 0, (int)(unBuf.Length - 8));
					}
				}
				finally
				{
					DcmDecodeParam = tmpDecodeParam;
					unBuf = null;
				}
			}
			return retval;
		}
		
		private long ParseSQItem(int id, int itemlen)
		{
			if (handler != null && unBuf == null)
				handler.StartItem(id, rPos - 8, itemlen);
			
			long lread;
			if (itemlen == -1)
			{
				lread = DoParse(ITEM_DELIMITATION_ITEM_TAG, itemlen);
				if (rTag != ITEM_DELIMITATION_ITEM_TAG || rLen != 0)
					throw new DcmParseException(LogMsg());
			}
			else
				lread = DoParse(UNDEFINED_TAG, itemlen);
			
			if (handler != null && unBuf == null)
				handler.EndItem(itemlen);
			
			return lread;
		}
		
		private int readValue()
		{
			byte[] data = readBytes(rLen);
			if (handler != null && unBuf == null)
				handler.Value(data, 0, rLen);
			if (rTag == TS_ID_TAG)
				tsUID = DecodeUID(data, rLen - 1);
			return rLen;
		}
		
		private String DecodeUID(byte[] data, int rlen1)
		{
			if (rlen1 < 0)
			{
				log.Warn("Empty Transfer Syntax UID in FMI");
				return "";
			}
			try
			{
				return Encoding.ASCII.GetString( data, 0, data[rlen1] == 0?rlen1:rlen1 + 1 );
			}
			catch (IOException ex)
			{
				log.Warn("Decoding Transfer Syntax UID in FMI failed!", ex);
				return null;
			}
		}
		
		private int readFragment(int id, int itemlen)
		{
			long rPos0 = rPos;
			byte[] data = readBytes(itemlen);
			if (handler != null && unBuf == null)
				handler.Fragment(id, rPos0 - 8, data, 0, itemlen);
			return itemlen;
		}
		
		private byte[] readBytes(int len)
		{
			if (len == 0)
				return b0;
			if (len < 0 || len > maxAlloc)
				throw new DcmParseException(LogMsg() + ", MaxAlloc:" + maxAlloc);
			byte[] retval = new byte[len];

			m_ins.BaseStream.Read(retval, 0, len);
			rPos += len;
			if (unBuf != null)
				unBuf.Write(retval, 0, len);
			return retval;
		}
	}
}