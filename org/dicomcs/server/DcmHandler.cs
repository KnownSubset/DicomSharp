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

namespace org.dicomcs.server
{
	using System;
	using System.Collections;
	using System.Threading;
	using org.dicomcs.net;
	
	/// <summary> <description>
	/// 
	/// </summary>
	public class DcmHandler : DcmHandlerI
	{
		private readonly static AssociationFactory assocFact = AssociationFactory.Instance;
		
		private AcceptorPolicy policy;
		private DcmServiceRegistry services;
		private ArrayList listeners = new ArrayList();
		
		private int requestTO = 5000;
		

		public DcmHandler(AcceptorPolicy policy, DcmServiceRegistry services)
		{
			if (policy == null)
				throw new System.NullReferenceException();
			
			if (services == null)
				throw new System.NullReferenceException();
			
			this.policy = policy;
			this.services = services;
		}
		
		public virtual void  Handle( Object socket )
		{
			Association assoc = assocFact.NewAcceptor( (System.Net.Sockets.TcpClient)socket );
			for( IEnumerator enu = listeners.GetEnumerator(); enu.MoveNext(); )
			{
				assoc.AddAssociationListener((AssociationListenerI) enu.Current);
			}
			
			if (assoc.Accept(policy, requestTO) is AAssociateAC)
			{
				ActiveAssociation active = assocFact.NewActiveAssociation(assoc, services);
				active.Start();
			}
		}
		
		public virtual void  AddAssociationListener(AssociationListenerI l)
		{
			lock(listeners)
			{
				listeners.Add(l);
			}
		}
		
		public virtual void  RemoveAssociationListener(AssociationListenerI l)
		{
			lock(listeners)
			{
				listeners.Remove(l);
			}
		}
		
		public virtual bool IsSockedClosedByHandler()
		{
			return true;
		}		
	}
}