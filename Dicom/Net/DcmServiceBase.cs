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
using System.Reflection;
using Dicom.Data;
using Dicom.Dictionary;
using Dicom.Utility;
using log4net;

namespace Dicom.Net {
    public class DcmServiceBase : DcmServiceI {
        public const int SUCCESS = 0x0000;
        public const int PENDING = 0xFF00;
        public const int NO_SUCH_SOP_CLASS = 0x0118;
        public const int UNRECOGNIZE_OPERATION = 0x0211;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly DcmServiceI VERIFICATION_SCP = new VerificationSCP();

        public static readonly DcmServiceI NO_SUCH_SOP_CLASS_SCP =
            new DcmServiceBase(new DcmServiceException(NO_SUCH_SOP_CLASS));

        protected static DcmObjectFactory objFact = DcmObjectFactory.Instance;
        protected static AssociationFactory assocFact = AssociationFactory.Instance;
        protected static UIDGenerator uidGen = UIDGenerator.Instance;
        protected DcmServiceException defEx;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defEx"></param>
        public DcmServiceBase(DcmServiceException defEx) {
            this.defEx = defEx;
        }

        public DcmServiceBase() {
            defEx = new DcmServiceException(UNRECOGNIZE_OPERATION);
        }

        #region DcmServiceI Members

        public virtual void c_store(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitCStoreRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, rqCmd.AffectedSOPInstanceUID, SUCCESS);
            try {
                DoCStore(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void c_get(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitCGetRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, SUCCESS);
            try {
                DoMultiRsp(assoc, rq, rspCmd, DoCGet(assoc, rq, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_find(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitCFindRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, PENDING);
            try {
                DoMultiRsp(assoc, rq, rspCmd, DoCFind(assoc, rq, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_move(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitCMoveRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, PENDING);
            try {
                DoMultiRsp(assoc, rq, rspCmd, DoCMove(assoc, rq, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_echo(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitCEchoRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, SUCCESS);
            try {
                DoCEcho(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                log.Error(e);
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_event_report(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitNEventReportRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, rqCmd.AffectedSOPInstanceUID, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNEventReport(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_get(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitNGetRSP(rqCmd.MessageID, rqCmd.RequestedSOPClassUID, rqCmd.RequestedSOPInstanceUID, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNGet(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_set(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitNSetRSP(rqCmd.MessageID, rqCmd.RequestedSOPClassUID, rqCmd.RequestedSOPInstanceUID, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNSet(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_action(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            var rspCmd = new Command();
            rspCmd.InitNActionRSP(rqCmd.MessageID, rqCmd.RequestedSOPClassUID, rqCmd.RequestedSOPInstanceUID, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNAction(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_create(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitNCreateRSP(rqCmd.MessageID, rqCmd.AffectedSOPClassUID, CreateUID(rqCmd.AffectedSOPInstanceUID),
                                  SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNCreate(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_delete(ActiveAssociation assoc, Dimse rq) {
            Command rqCmd = rq.Command;
            Command rspCmd = objFact.NewCommand();
            rspCmd.InitNDeleteRSP(rqCmd.MessageID, rqCmd.RequestedSOPClassUID, rqCmd.RequestedSOPInstanceUID, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNDelete(assoc, rq, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        #endregion

        protected virtual void DoAfterRsp(ActiveAssociation assoc, Dimse rsp) {}

        protected virtual void DoCStore(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCGet(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCFind(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCMove(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual void DoCEcho(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            //      rq.getDataset(); // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNEventReport(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNGet(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNSet(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNAction(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNCreate(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNDelete(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            Dataset generatedAux = rq.Dataset; // read out dataset
            throw defEx;
        }

        // Private -------------------------------------------------------
        private void DoMultiRsp(ActiveAssociation assoc, Dimse rq, Command rspCmd, MultiDimseRsp mdr) {
            try {
                assoc.AddCancelListener(rspCmd.MessageIDToBeingRespondedTo, mdr.CancelListener);
                do {
                    Dataset rspData = mdr.next(assoc, rq, rspCmd);
                    Dimse rsp = assocFact.NewDimse(rq.pcid(), rspCmd, rspData);
                    assoc.Association.Write(rsp);
                    DoAfterRsp(assoc, rsp);
                } while (rspCmd.IsPending());
            }
            finally {
                mdr.release();
            }
        }

        private static String CreateUID(String uid) {
            return uid != null ? uid : uidGen.createUID();
        }

        // Inner classes -------------------------------------------------

        #region Nested type: MultiDimseRsp

        public interface MultiDimseRsp {
            DimseListenerI CancelListener { get; }
            Dataset next(ActiveAssociation assoc, Dimse rq, Command rspCmd);
            void release();
        }

        #endregion
    }


    internal class VerificationSCP : DcmServiceBase {
        protected override void DoCEcho(ActiveAssociation assoc, Dimse rq, Command rspCmd) {
            rspCmd.PutUS(Tags.Status, SUCCESS);
        }
    }
}