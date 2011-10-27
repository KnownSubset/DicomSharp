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

using System;
using System.IO;
using System.Net.Sockets;
using Dicom.Data;
using Dicom.Utility;

namespace Dicom.Net {
    /// <summary>
    /// Association related object factory
    /// </summary>
    public class AssociationFactory {
        private static readonly AssociationFactory s_instance = new AssociationFactory();

        private AssociationFactory() {}

        public static AssociationFactory Instance {
            get { return s_instance; }
        }

        public virtual AAssociateRQ NewAAssociateRQ() {
            return new AAssociateRQ();
        }

        public virtual AAssociateAC NewAAssociateAC() {
            return new AAssociateAC();
        }

        public virtual AAssociateRJ NewAAssociateRJ(int result, int source, int reason) {
            return new AAssociateRJ(result, source, reason);
        }

        public virtual PDataTF NewPDataTF(int maxLength) {
            return new PDataTF(maxLength);
        }

        public virtual AReleaseRQ NewAReleaseRQ() {
            return AReleaseRQ.Instance;
        }

        public virtual AReleaseRP NewAReleaseRP() {
            return AReleaseRP.Instance;
        }

        public virtual AAbort NewAAbort(int source, int reason) {
            return new AAbort(source, reason);
        }

        public virtual PresContext NewPresContext(int pcid, String asuid, String[] tsuids) {
            return new PresContext(0x020, pcid, 0, StringUtils.CheckUID(asuid), StringUtils.CheckUIDs(tsuids));
        }

        public virtual PresContext NewPresContext(int pcid, int result, String tsuid) {
            return new PresContext(0x021, pcid, result, null, new[] {StringUtils.CheckUID(tsuid)});
        }

        public virtual AsyncOpsWindow NewAsyncOpsWindow(int maxOpsInvoked, int maxOpsPerfomed) {
            return new AsyncOpsWindow(maxOpsInvoked, maxOpsPerfomed);
        }

        public virtual RoleSelection NewRoleSelection(String uid, bool scu, bool scp) {
            return new RoleSelection(uid, scu, scp);
        }

        public virtual ExtNegotiation NewExtNegotiation(String uid, byte[] info) {
            return new ExtNegotiation(uid, info);
        }

        public virtual PduI readFrom(Stream ins, byte[] buf) {
            var raw = new UnparsedPdu(ins, buf);
            switch (raw.GetType()) {
                case 1:
                    return AAssociateRQ.Parse(raw);

                case 2:
                    return AAssociateAC.Parse(raw);

                case 3:
                    return AAssociateRJ.Parse(raw);

                case 4:
                    return PDataTF.Parse(raw);

                case 5:
                    return AReleaseRQ.Parse(raw);

                case 6:
                    return AReleaseRP.Parse(raw);

                case 7:
                    return AAbort.Parse(raw);

                default:
                    throw new PduException("Unrecognized " + raw,
                                           new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
            }
        }

        public virtual Association NewRequestor(TcpClient s) {
            return new Association(s, true);
        }

        public virtual Association NewAcceptor(TcpClient s) {
            return new Association(s, false);
        }

        public virtual ActiveAssociation NewActiveAssociation(Association assoc, DcmServiceRegistry services) {
            return new ActiveAssociation(assoc, services);
        }

        public virtual Dimse NewDimse(int pcid, Command cmd) {
            return new Dimse(pcid, cmd, null, null);
        }

        public virtual Dimse NewDimse(int pcid, Command cmd, Dataset ds) {
            return new Dimse(pcid, cmd, ds, null);
        }

        public virtual Dimse NewDimse(int pcid, Command cmd, DataSourceI src) {
            return new Dimse(pcid, cmd, null, src);
        }

        public virtual AcceptorPolicy NewAcceptorPolicy() {
            return new AcceptorPolicy();
        }

        public virtual DcmServiceRegistry NewDcmServiceRegistry() {
            return new DcmServiceRegistry();
        }
    }
}