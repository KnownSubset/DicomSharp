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
	using System.Collections;
	using org.dicomcs;
	using org.dicomcs.dict;
	using org.dicomcs.net;
	using org.dicomcs.util;
	
	/// <summary> 
	/// Defines association acceptance/rejection behavior.
	/// </summary>
	public class AcceptorPolicy
	{
		private int maxLength = PDataTF.DEF_MAX_PDU_LENGTH;
		private AsyncOpsWindow aow = null;
		private String classUID = Implementation.ClassUID;
		private String Vers = Implementation.VersionName;
		private Hashtable appCtxMap = new Hashtable();
		private ArrayList calledAETs =  null;
		private ArrayList callingAETs = null;
		private Hashtable policyForCalledAET = new Hashtable();
		private Hashtable policyForCallingAET = new Hashtable();
		private Hashtable presCtxMap = new Hashtable();
		private Hashtable roleSelectionMap = new Hashtable();
		private Hashtable extNegotiaionMap = new Hashtable();

		public virtual int MaxPduLength
		{
			get { return maxLength; }			
			set
			{
				if (value < 0)
					throw new System.ArgumentException("maxLength:" + value);
				
				this.maxLength = value;
			}			
		}
		public virtual AsyncOpsWindow AsyncOpsWindow
		{
			get { return aow; }			
		}
		public virtual String ClassUID
		{
			get { return classUID; }			
			set { this.classUID = StringUtils.CheckUID(value); }
			
		}
		public virtual String VersionName
		{
			get { return Vers; }			
			set { this.Vers = value != null?StringUtils.CheckAET(value):null; }			
		}
		public virtual String[] CalledAETs
		{
			get
			{
				return calledAETs != null?(String[]) calledAETs.ToArray():null;
			}			
			set
			{
				calledAETs = value != null?new ArrayList( StringUtils.CheckAETs(value)):null;
			}			
		}
		public virtual String[] CallingAETs
		{
			get
			{
				return callingAETs != null?(String[]) callingAETs.ToArray():null;
			}			
			set
			{
				callingAETs = value != null?new ArrayList(StringUtils.CheckAETs(value)):null;
			}			
		}
		
		/// <summary>
		/// Constructor
		/// </summary>		
		public AcceptorPolicy()
		{
			PutPresContext(UIDs.Verification, new String[]{UIDs.ImplicitVRLittleEndian});
		}
		
		public virtual void  setAsyncOpsWindow(int maxOpsInvoked, int maxOpsPerformed)
		{
			if (maxOpsInvoked == 1 && maxOpsPerformed == 1)
			{
				aow = null;
			}
			else if (aow == null || aow.MaxOpsInvoked != maxOpsInvoked || aow.MaxOpsPerformed != maxOpsPerformed)
			{
				aow = new AsyncOpsWindow(maxOpsInvoked, maxOpsPerformed);
			}
		}
		
		public virtual void PutApplicationContextName(String proposed, String returned)
		{
			appCtxMap.Add(StringUtils.CheckUID(proposed), StringUtils.CheckUID(returned));
		}
		
		public virtual bool AddCalledAET(String aet)
		{
			StringUtils.CheckAET(aet);
			
			if (calledAETs == null)
				calledAETs = new ArrayList();
			
			return (calledAETs.Add(aet) >= 0);
		}
		
		public virtual void RemoveCalledAET(String aet)
		{
			if( calledAETs != null )
				calledAETs.Remove(aet);
		}
		
		
		
		public virtual bool AddCallingAET(String aet)
		{
			StringUtils.CheckAET(aet);
			
			if (callingAETs == null)
				callingAETs = new ArrayList();
			
			return (callingAETs.Add(aet)>=0);
		}
		
		public virtual void RemoveCallingAET(String aet)
		{
			if( callingAETs != null )
				callingAETs.Remove(aet);
		}
		
		public virtual AcceptorPolicy GetPolicyForCallingAET(String aet)
		{
			return (AcceptorPolicy) policyForCallingAET[aet];
		}
		
		public virtual void PutPolicyForCallingAET(String aet, AcceptorPolicy policy)
		{
			PutPolicyForXXXAET(aet, policy, policyForCallingAET);
		}
		
		public virtual AcceptorPolicy GetPolicyForCalledAET(String aet)
		{
			return (AcceptorPolicy) policyForCalledAET[aet];
		}
		
		public virtual void PutPolicyForCalledAET(String aet, AcceptorPolicy policy)
		{
			PutPolicyForXXXAET(aet, policy, policyForCalledAET);
		}
		
		private void PutPolicyForXXXAET(String aet, AcceptorPolicy policy, Hashtable map)
		{
			if (policy != null)
			{
				map.Add(StringUtils.CheckAET(aet), policy);
			}
			else
			{
				map.Remove(aet);
			}
		}
		
		public void PutPresContext(String asuid, String[] tsuids)
		{
			if (tsuids != null)
			{
				presCtxMap.Add(asuid, new PresContext(0x020, 1, 0, StringUtils.CheckUID(asuid), StringUtils.CheckUIDs(tsuids)));
			}
			else
			{
				presCtxMap.Remove(asuid);
			}
		}
		
		public virtual PresContext GetPresContext(String syntax )
		{
			return (PresContext) presCtxMap[syntax];
		}
		
		public virtual void PutRoleSelection(String uid, bool scu, bool scp)
		{
			roleSelectionMap.Add(StringUtils.CheckUID(uid), new RoleSelection(uid, scu, scp));
		}
		
		public virtual RoleSelection GetRoleSelection(String uid)
		{
			return (RoleSelection) roleSelectionMap[uid];
		}
		
		public virtual void RemoveRoleSelection(String uid)
		{
			roleSelectionMap.Remove(uid);
		}
		
		public virtual void PutExtNegPolicy(String uid, ExtNegotiatorI en)
		{
			if (en != null)
			{
				extNegotiaionMap.Add(uid, en);
			}
			else
			{
				extNegotiaionMap.Remove(uid);
			}
		}
		
		public virtual ExtNegotiatorI GetExtNegPolicy(String uid)
		{
			return (ExtNegotiatorI) extNegotiaionMap[uid];
		}
		
		public virtual PduI Negotiate(AAssociateRQ rq)
		{
			if ((rq.ProtocolVersion & 0x0001) == 0)
			{
				return new AAssociateRJ(AAssociateRJ.REJECTED_PERMANENT, AAssociateRJ.SERVICE_PROVIDER_ACSE, AAssociateRJ.PROTOCOL_VERSION_NOT_SUPPORTED);
			}
			String calledAET = rq.CalledAET;
			if (calledAETs != null && !calledAETs.Contains(calledAET))
			{
				return new AAssociateRJ(AAssociateRJ.REJECTED_PERMANENT, AAssociateRJ.SERVICE_USER, AAssociateRJ.CALLED_AE_TITLE_NOT_RECOGNIZED);
			}
			AcceptorPolicy policy1 = (AcceptorPolicy) GetPolicyForCalledAET(calledAET);
			if (policy1 == null)
				policy1 = this;
			
			String callingAET = rq.CalledAET;
			if (policy1.callingAETs != null && !policy1.callingAETs.Contains(callingAET))
			{
				return new AAssociateRJ(AAssociateRJ.REJECTED_PERMANENT, AAssociateRJ.SERVICE_USER, AAssociateRJ.CALLING_AE_TITLE_NOT_RECOGNIZED);
			}
			AcceptorPolicy policy2 = (AcceptorPolicy) policy1.GetPolicyForCallingAET(callingAET);
			if (policy2 == null)
				policy2 = policy1;
			
			return policy2.doNegotiate(rq);
		}
		
		private PduI doNegotiate(AAssociateRQ rq)
		{
			String appCtx = NegotiateAppCtx(rq.ApplicationContext);
			if (appCtx == null)
			{
				return new AAssociateRJ(AAssociateRJ.REJECTED_PERMANENT, AAssociateRJ.SERVICE_USER, AAssociateRJ.APPLICATION_CONTEXT_NAME_NOT_SUPPORTED);
			}
			AAssociateAC ac = new AAssociateAC();
			ac.ApplicationContext = appCtx;
			ac.CalledAET = rq.CalledAET;
			ac.CallingAET = rq.CallingAET;
			ac.MaxPduLength = this.maxLength;
			ac.ClassUID = this.ClassUID;
			ac.VersionName = this.Vers;
			ac.AsyncOpsWindow = NegotiateAOW(rq.AsyncOpsWindow);
			NegotiatePresCtx(rq, ac);
			NegotiateRoleSelection(rq, ac);
			NegotiateExt(rq, ac);
			return ac;
		}
		
		private String NegotiateAppCtx(String proposed)
		{
			String retval = (String) appCtxMap[proposed];
			if (retval != null)
				return retval;
			
			if (UIDs.DICOMApplicationContextName.Equals(proposed))
				return proposed;
			
			return null;
		}
		
		private void  NegotiatePresCtx(AAssociateRQ rq, AAssociateAC ac)
		{
			 for ( IEnumerator enu= rq.ListPresContext().GetEnumerator(); enu.MoveNext(); )
				ac.AddPresContext(NegotiatePresCtx((PresContext) enu.Current));
		}
		
		private PresContext NegotiatePresCtx(PresContext offered)
		{
			int result = PresContext.ABSTRACT_SYNTAX_NOT_SUPPORTED;
			String tsuid = UIDs.ImplicitVRLittleEndian;
			
			PresContext accept = GetPresContext(offered.AbstractSyntaxUID);
			if (accept != null)
			{
				result = PresContext.TRANSFER_SYNTAXES_NOT_SUPPORTED;
				for ( IEnumerator enu= accept.TransferSyntaxUIDs.GetEnumerator(); enu.MoveNext(); )
				{
					tsuid = (String) enu.Current;
					if (offered.TransferSyntaxUIDs.IndexOf(tsuid) != - 1)
					{
						result = PresContext.ACCEPTANCE;
						break;
					}
				}
			}
			return new PresContext(0x021, offered.pcid(), result, null, new String[]{tsuid});
		}
		
		private void  NegotiateRoleSelection(AAssociateRQ rq, AAssociateAC ac)
		{
			for ( IEnumerator enu= rq.ListRoleSelections().GetEnumerator(); enu.MoveNext(); )
				ac.AddRoleSelection(NegotiateRoleSelection((RoleSelection) enu.Current));
		}
		
		private RoleSelection NegotiateRoleSelection(RoleSelection offered)
		{
			bool scu = offered.scu();
			bool scp = false;
			
			RoleSelection accept = GetRoleSelection(offered.SOPClassUID);
			if (accept != null)
			{
				scu = offered.scu() && accept.scu();
				scp = offered.scp() && accept.scp();
			}
			return new RoleSelection(offered.SOPClassUID, scu, scp);
		}
		
		private void  NegotiateExt(AAssociateRQ rq, AAssociateAC ac)
		{
			for ( IEnumerator enu= rq.ListExtNegotiations().GetEnumerator(); enu.MoveNext(); )
			{
				ExtNegotiation offered = (ExtNegotiation) enu.Current;
				String uid = offered.SOPClassUID;
				ExtNegotiatorI enp = GetExtNegPolicy(uid);
				if (enp != null)
					ac.AddExtNegotiation(new ExtNegotiation(uid, enp.Negotiate(offered.info())));
			}
		}
		
		private AsyncOpsWindow NegotiateAOW(AsyncOpsWindow offered)
		{
			if (offered == null)
				return null;
			
			if (aow == null)
				return AsyncOpsWindow.DEFAULT;
			
			return new AsyncOpsWindow(minAOW(offered.MaxOpsInvoked, aow.MaxOpsInvoked), minAOW(offered.MaxOpsPerformed, aow.MaxOpsPerformed));
		}
		
		internal static int minAOW(int a, int b)
		{
			return a == 0?b:b == 0?a:System.Math.Min(a, b);
		}
		
		public virtual ArrayList ListPresContext()
		{
			return new ArrayList(presCtxMap.Values);
		}
		
		// Inner classes -------------------------------------------------
	}
}