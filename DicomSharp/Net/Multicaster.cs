#region Copyright

// 
// This library is based on Dicom# see http://sourceforge.net/projects/dicom-cs/
// Copyright (C) 2002 Fang Yang. All rights reserved.
// That library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2012 Nathan Dauber. All rights reserved.
// 
// This file is part of dicomSharp, see https://github.com/KnownSubset/DicomSharp
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
// Nathan Dauber (nathan.dauber@gmail.com)
//

#endregion

using System.IO;

namespace DicomSharp.Net {
    /// <summary>
    /// </summary>
    public class Multicaster : IAssociationListener {
        private readonly IAssociationListener a;
        private readonly IAssociationListener b;

        public Multicaster(IAssociationListener a, IAssociationListener b) {
            this.a = a;
            this.b = b;
        }

        #region IAssociationListener Members

        public virtual void Write(Association src, IPdu pdu) {
            a.Write(src, pdu);
            b.Write(src, pdu);
        }

        public virtual void Write(Association src, Dimse dimse) {
            a.Write(src, dimse);
            b.Write(src, dimse);
        }

        public virtual void Received(Association src, IPdu pdu) {
            a.Received(src, pdu);
            b.Received(src, pdu);
        }

        public virtual void Received(Association src, Dimse dimse) {
            a.Received(src, dimse);
            b.Received(src, dimse);
        }

        public virtual void Error(Association src, IOException ioe) {
            a.Error(src, ioe);
            b.Error(src, ioe);
        }

        public virtual void Close(Association src) {
            a.Close(src);
            b.Close(src);
        }

        #endregion

        public static IAssociationListener add(IAssociationListener a, IAssociationListener b) {
            if (a == null) {
                return b;
            }
            if (b == null) {
                return a;
            }
            return new Multicaster(a, b);
        }

        public static IAssociationListener Remove(IAssociationListener l, IAssociationListener oldl) {
            if (l == oldl || l == null) {
                return null;
            }
            if (l is Multicaster) {
                return ((Multicaster) l).Remove(oldl);
            }
            return null;
        }

        private IAssociationListener Remove(IAssociationListener oldl) {
            if (oldl == a) {
                return b;
            }
            if (oldl == b) {
                return a;
            }
            IAssociationListener a2 = Remove(a, oldl);
            IAssociationListener b2 = Remove(b, oldl);
            if (a2 == a && b2 == b) {
                return this;
            }
            return add(a2, b2);
        }
    }
}