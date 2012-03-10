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

using System;
using System.Collections;
using System.IO;
using System.Threading;

namespace DicomSharp.Net {
    /// <summary> 
    /// 
    /// </summary>
    public class FutureDimseResponse : IDimseListener, IAssociationListener {
        private readonly ArrayList pending = new ArrayList();
        private bool closed;
        private IOException exception;
        private bool ready;
        private Dimse rsp;
        private long setAfterCloseTO = 500;

        public virtual IOException Exception {
            get {
                lock (this) {
                    return exception;
                }
            }

            set {
                lock (this) {
                    exception = value;
                    ready = true;
                    Monitor.PulseAll(this);
                }
            }
        }

        #region IAssociationListener Members

        public virtual void Write(Association src, IPdu Pdu) {}

        public virtual void Received(Association src, IDimse dimse) {}

        public virtual void Error(Association src, IOException ioe) {
            Exception = ioe;
        }

        public virtual void Close(Association src) {
            lock (this) {
                closed = true;
                Monitor.PulseAll(this);
            }
        }

        public virtual void Write(Association src, IDimse dimse) {}

        public virtual void Received(Association src, IPdu Pdu) {}

        #endregion

        #region IDimseListener Members

        public virtual void DimseReceived(Association assoc, Dimse dimse) {
            if (dimse.DicomCommand.IsPending()) {
                pending.Add(dimse);
            }
            else {
                Set(dimse);
            }
        }

        #endregion

        public virtual void Set(Dimse rsp) {
            lock (this) {
                this.rsp = rsp;
                ready = true;
                Monitor.PulseAll(this);
            }
        }

        public virtual Dimse Get() {
            lock (this) {
                while (!ready && !closed) {
                    Monitor.Wait(this);
                }

                if (!ready) {
                    Monitor.Wait(this, TimeSpan.FromMilliseconds(setAfterCloseTO));
                }

                return DoGet();
            }
        }

        public virtual ArrayList ListPending() {
            lock (this) {
                return pending;
            }
        }

        public virtual bool IsReady() {
            lock (this) {
                return ready;
            }
        }

        public virtual Dimse Peek() {
            lock (this) {
                return rsp;
            }
        }

        private Dimse DoGet() {
            if (exception != null) {
                throw exception;
            }
            else {
                return rsp;
            }
        }
    }
}