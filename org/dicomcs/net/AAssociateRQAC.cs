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
	using System.Text;
	using System.Collections;
	using org.dicomcs.dict;
	using org.dicomcs.util;
	
	/// <summary>
	/// </summary>
	public abstract class AAssociateRQAC : PduI
	{	
		private String appCtxUID = UIDs.DICOMApplicationContextName;
		private int version = 1;
		private String verName = Implementation.VersionName;
		private int maxLength = PDataTF.DEF_MAX_PDU_LENGTH;
		private String callingAET = "ANONYMOUS";
		private String calledAET = "ANONYMOUS";
		private String classUID = Implementation.ClassUID;
		private AsyncOpsWindow asyncOpsWindow = null;
		protected Hashtable presCtxs = new Hashtable();
		protected Hashtable roleSels = new Hashtable();
		protected Hashtable extNegs = new Hashtable();


		public virtual int ProtocolVersion
		{
			get { return version; }			
			set { this.version = value; }			
		}
		public virtual String CalledAET
		{
			get { return calledAET; }			
			set { this.calledAET = StringUtils.CheckAET(value); }			
		}
		public virtual String CallingAET
		{
			get { return callingAET; }			
			set { this.callingAET = StringUtils.CheckAET(value); }			
		}
		public virtual String ApplicationContext
		{
			get { return appCtxUID; }			
			set { value = StringUtils.CheckUID(value); }			
		}
		public virtual String ClassUID
		{
			get { return classUID; }			
			set { this.classUID = StringUtils.CheckUID(value); }			
		}
		public virtual String VersionName
		{
			get { return verName; }			
			set { this.verName = value != null?StringUtils.CheckAET(value):null;}			
		}
		public virtual int MaxPduLength
		{
			get { return maxLength; }			
			set 
			{ 
				if (value < 0)
				{
					throw new System.ArgumentException("maxLength:" + value);
				}
				this.maxLength = value;
			}			
		}
		public virtual AsyncOpsWindow AsyncOpsWindow
		{
			get { return asyncOpsWindow; }			
			set { this.asyncOpsWindow = value; }			
		}
		private int UserInfoLength
		{
			get
			{
				int retval = 12 + ClassUID.Length;
				if (asyncOpsWindow != null)
				{
					retval += 8;
				}
				 for (IEnumerator enu = roleSels.Values.GetEnumerator(); enu.MoveNext(); )
				{
					RoleSelection rs = (RoleSelection) enu.Current;
					retval += 4 + rs.length();
				}
				if (VersionName != null)
				{
					retval += 4 + VersionName.Length;
				}
				 for (IEnumerator enu = extNegs.Values.GetEnumerator(); enu.MoveNext(); )
				{
					ExtNegotiation en = (ExtNegotiation) enu.Current;
					retval += 4 + en.length();
				}
				return retval;
			}			
		}
				
		protected virtual AAssociateRQAC Init(UnparsedPdu raw)
		{
			if (raw.buffer() == null)
			{
				throw new PduException("Pdu length exceeds supported maximum " + raw, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.REASON_NOT_SPECIFIED));
			}
			ByteBuffer bb = ByteBuffer.Wrap( raw.buffer(), 6, raw.length(), ByteOrder.BIG_ENDIAN );
			try
			{
				version = bb.ReadInt16();		// Protocol version
				bb.Skip(2);						// Skip 2 bytes
				calledAET = bb.ReadString(16);	// Called AET
				callingAET = bb.ReadString(16);	// Calling AET
				if( bb.Skip(32) != 32)
				{
					throw new EndOfStreamException();
				}

				while( bb.Position < bb.Length )
				{
					int itemType = bb.ReadByte();	// Item type
					bb.Skip();						// Skip one byte
					int itemLen = bb.ReadInt16();	// Item length

					switch (itemType)
					{
						case 0x10: 
							appCtxUID = bb.ReadString(itemLen);
							break;
						
						case 0x20: 
						case 0x21: 
							if (itemType != pctype())
							{
								throw new PduException("unexpected item type " + System.Convert.ToString(itemType, 16) + 'H', new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU_PARAMETER));
							}
							AddPresContext(new PresContext(itemType, bb, itemLen));
							break;
						
						case 0x50: 
							ReadUserInfo(bb, itemLen);
							break;
						
						default: 
							throw new PduException("unrecognized item type " + System.Convert.ToString(itemType, 16) + 'H', new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU_PARAMETER));						
					}
				}
			}
			catch (PduException e)
			{
				throw e;
			}
			catch (System.Exception e)
			{
				throw new PduException("Failed to parse " + raw, e, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.REASON_NOT_SPECIFIED));
			}
			return this;
		}			
		
		public int NextPCID()
		{
			int c = presCtxs.Count;
			if (c == 128)
			{
				return - 1;
			}
			int retval = ((c << 1) | 1);
			while (presCtxs.ContainsKey(retval))
			{
				retval = (retval + 2) % 256;
			}
			return retval;
		}
		
		public void AddPresContext(PresContext presCtx)
		{
			if (((PresContext) presCtx).type() != pctype())
			{
				throw new System.ArgumentException("wrong type of " + presCtx);
			}
			presCtxs.Add(presCtx.pcid(), presCtx);
		}
		
		public void RemovePresContext(int pcid)
		{
			presCtxs.Remove(pcid);
		}
		
		public PresContext GetPresContext(int pcid)
		{
			return (PresContext) presCtxs[pcid];
		}
		
		public ICollection ListPresContext()
		{
			return presCtxs.Values;
		}
		
		public void  ClearPresContext()
		{
			presCtxs.Clear();
		}
				
		public void RemoveRoleSelection(String uid)
		{
			roleSels.Remove(uid);
		}
		
		public RoleSelection GetRoleSelection(String uid)
		{
			return (RoleSelection) roleSels[uid];
		}
		
		public virtual ICollection ListRoleSelections()
		{
			return roleSels.Values;
		}
		
		public virtual void  ClearRoleSelections()
		{
			roleSels.Clear();
		}
		
		public void RemoveExtNegotiation(String uid)
		{
			extNegs.Remove(uid);
		}
		
		public ExtNegotiation GetExtNegotiation(String uid)
		{
			return (ExtNegotiation) extNegs[uid];
		}
		
		public virtual ICollection ListExtNegotiations()
		{
			return extNegs.Values;
		}
		
		public virtual void  ClearExtNegotiations()
		{
			extNegs.Clear();
		}
		
		internal static String ReadASCII(BinaryReader ins, int len)
		{
			byte[] b = new byte[len];
			ins.Read( b, 0, b.Length );
			while (len > 0 && b[len - 1] == 0)
			{
				--len;
			}
			return Encoding.ASCII.GetString( b, 0, len ).Trim();
		}
		
		public virtual void AddRoleSelection(RoleSelection roleSel)
		{
			roleSels.Add(roleSel.SOPClassUID, roleSel);
		}
		
		public virtual void AddExtNegotiation(ExtNegotiation extNeg)
		{
			extNegs.Add(extNeg.SOPClassUID, extNeg);
		}
		
		private void  ReadUserInfo(ByteBuffer bb, int len)
		{
			int diff = len - (int)bb.Remaining;
			if (diff != 0)
			{
				throw new PduException("User info item length=" + len + " mismatch Pdu length (diff=" + diff + ")", new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
			}

			while ( bb.Remaining > 0  )
			{
				int subItemType = bb.ReadByte();
				bb.Skip();
				int itemLen = bb.ReadInt16();
				switch (subItemType)
				{
					case 0x51: 
						if (itemLen != 4)
						{
							throw new PduException("Illegal length of Maximum length sub-item: " + itemLen, new AAbort(AAbort.SERVICE_PROVIDER, AAbort.INVALID_PDU_PARAMETER_VALUE));
						}
						maxLength = bb.ReadInt32();
						break;
					
					case 0x52: 
						ClassUID = bb.ReadString(itemLen);
						break;
					
					case 0x53: 
						asyncOpsWindow = new AsyncOpsWindow(bb, itemLen);
						break;
					
					case 0x54: 
						AddRoleSelection(new RoleSelection(bb, itemLen));
						break;
					
					case 0x55: 
						VersionName = bb.ReadString(itemLen);
						break;
					
					case 0x56: 
						AddExtNegotiation(new ExtNegotiation(bb, itemLen));
						break;
					
					default: 
						throw new PduException("unrecognized user sub-item type " + System.Convert.ToString(subItemType, 16) + 'H', new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU_PARAMETER));
				}
			}
		}
		
		protected abstract int type();
		
		protected abstract int pctype();
		
		private static byte[] ZERO32 = new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
		
		private void  WriteAE(ByteBuffer bb, String aet)
		{
			bb.Write(aet);
			for (int n = aet.Length; n < 16; ++n)
			{
				bb.Write((System.Byte) ' ');
			}
		}
		
		private sealed class MyByteBuffer : ByteBuffer
		{
			AAssociateRQAC assoc = null;
			internal MyByteBuffer( AAssociateRQAC assoc ):base(4096, ByteOrder.BIG_ENDIAN)
			{
				Write((System.Byte) 0);
				Write((System.Byte) 0);
				Write((System.Byte) 0);
				Write((System.Byte) 0);
				Write((System.Byte) 0);
				Write((System.Byte) 0);
				this.assoc = assoc;
			}
			
			internal void  WriteTo(int type, Stream os )
			{
				int len = (int) Length - 6;
				this.Position = 0;
				Write((byte) type);
				Write((byte) 0);
				Write((int)len );
				os.Write( ToArray(), 0, (int) Length);

				StringUtils.dumpBytes( assoc.TypeAsString(), ToArray(), 0, (int)Length );
			}
		}
		
		public void  WriteTo(Stream outs)
		{
			MyByteBuffer bb = new MyByteBuffer( this );

			bb.Write((System.Int16) version);
			bb.Write((System.Byte) 0);
			bb.Write((System.Byte) 0);
			WriteAE(bb, calledAET);
			WriteAE(bb, callingAET);
			bb.Write(ZERO32);
			bb.Write((System.Byte) 0x10);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) appCtxUID.Length);
			bb.Write(appCtxUID);
			
			for (IEnumerator enu = presCtxs.Values.GetEnumerator(); enu.MoveNext(); )
			{
				((PresContext) enu.Current).WriteTo(bb);
			}
			WriteUserInfo(bb);
			bb.WriteTo(type(), outs);
		}
		
		private void  WriteUserInfo(ByteBuffer bb)
		{
			bb.Write((System.Byte) 0x50);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) UserInfoLength);
			bb.Write((System.Byte) 0x51);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) 4);
			bb.Write((System.Int32) maxLength);
			bb.Write((System.Byte) 0x52);
			bb.Write((System.Byte) 0);
			bb.Write((System.Int16) ClassUID.Length);
			bb.Write(ClassUID);
			if (asyncOpsWindow != null)
			{
				((AsyncOpsWindow) asyncOpsWindow).WriteTo(bb);
			}
			
			for (IEnumerator enu = roleSels.Values.GetEnumerator(); enu.MoveNext(); )
			{
				((RoleSelection) enu.Current).WriteTo(bb);
			}
			if (VersionName != null)
			{
				bb.Write((System.Byte) 0x55);
				bb.Write((System.Byte) 0);
				bb.Write((System.Int16) VersionName.Length);
				bb.Write(VersionName);
			}
			for (IEnumerator enu = extNegs.Values.GetEnumerator(); enu.MoveNext(); )
			{
				((ExtNegotiation) enu.Current).WriteTo(bb);
			}
		}
		
		protected abstract String TypeAsString();
		
		public override String ToString()
		{
			return ToString(false);
		}
		
		public virtual String ToString(bool verbose)
		{
			return ToStringBuffer(new System.Text.StringBuilder(), verbose).ToString();
		}
		
		internal System.Text.StringBuilder ToStringBuffer(System.Text.StringBuilder sb, bool verbose)
		{
			sb.Append(TypeAsString()).Append("\n\tappCtxName:\t").Append(UIDs.GetName(appCtxUID)).Append("\n\tClass:\t").Append(ClassUID).Append("\n\tVersion:\t").Append(VersionName).Append("\n\tcalledAET:\t").Append(calledAET).Append("\n\tcallingAET:\t").Append(callingAET).Append("\n\tmaxPduLen:\t").Append(maxLength).Append("\n\tasyncOpsWindow:\t");
			if (asyncOpsWindow != null)
			{
				sb.Append("maxOpsInvoked=").Append(asyncOpsWindow.MaxOpsInvoked).Append(", maxOpsPerformed=").Append(asyncOpsWindow.MaxOpsPerformed);
			}
			if (verbose)
			{
				for (IEnumerator enu = presCtxs.Values.GetEnumerator(); enu.MoveNext(); )
				{
					Append((PresContext) enu.Current, sb);
				}
				 for (IEnumerator enu = roleSels.Values.GetEnumerator(); enu.MoveNext(); )
				{
					sb.Append("\n\t").Append(enu.Current);
				}
				 for (IEnumerator enu = extNegs.Values.GetEnumerator(); enu.MoveNext(); )
				{
					sb.Append("\n\t").Append(enu.Current);
				}
			}
			else
			{
				AppendPresCtxSummary(sb);
				sb.Append("\n\troleSel:\t#").Append(roleSels.Count).Append("\n\textNego:\t#").Append(extNegs.Count);
			}
			return sb;
		}
		
		protected abstract void  Append(PresContext pc, System.Text.StringBuilder sb);
		
		protected abstract void  AppendPresCtxSummary(System.Text.StringBuilder sb);
	}
}