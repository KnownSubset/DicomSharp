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
	
	/// <summary>
	/// </summary>
	public class Multicaster : AssociationListenerI
	{
		private AssociationListenerI a;
		private AssociationListenerI b;
		
		public static AssociationListenerI add(AssociationListenerI a, AssociationListenerI b)
		{
			if (a == null)
				return b;
			if (b == null)
				return a;
			return new Multicaster(a, b);
		}
		
		public static AssociationListenerI Remove(AssociationListenerI l, AssociationListenerI oldl)
		{
			if (l == oldl || l == null)
			{
				return null;
			}
			if (l is Multicaster)
			{
				return ((Multicaster) l).Remove(oldl);
			}
			return null;
		}
		
		public Multicaster(AssociationListenerI a, AssociationListenerI b)
		{
			this.a = a;
			this.b = b;
		}
		
		public virtual void  Write(Association src, PduI pdu)
		{
			a.Write(src, pdu);
			b.Write(src, pdu);
		}
		
		public virtual void  Write(Association src, Dimse dimse)
		{
			a.Write(src, dimse);
			b.Write(src, dimse);
		}
		
		public virtual void  Received(Association src, PduI pdu)
		{
			a.Received(src, pdu);
			b.Received(src, pdu);
		}
		
		public virtual void  Received(Association src, Dimse dimse)
		{
			a.Received(src, dimse);
			b.Received(src, dimse);
		}
		
		public virtual void  Error(Association src, System.IO.IOException ioe)
		{
			a.Error(src, ioe);
			b.Error(src, ioe);
		}
		
		public virtual void  Close(Association src)
		{
			a.Close(src);
			b.Close(src);
		}
		
		private AssociationListenerI Remove(AssociationListenerI oldl)
		{
			if (oldl == a)
				return b;
			if (oldl == b)
				return a;
			AssociationListenerI a2 = Remove(a, oldl);
			AssociationListenerI b2 = Remove(b, oldl);
			if (a2 == a && b2 == b)
			{
				return this;
			}
			return add(a2, b2);
		}
	}
}