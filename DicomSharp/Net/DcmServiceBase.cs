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
using DicomSharp.Data;
using DicomSharp.Dictionary;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {
    public class DcmServiceBase : IDcmService {
        public const int SUCCESS = 0x0000;
        public const int PENDING = 0xFF00;
        public const int NO_SUCH_SOP_CLASS = 0x0118;
        public const int UNRECOGNIZE_OPERATION = 0x0211;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly IDcmService VERIFICATION_SCP = new VerificationSCP();

        public static readonly IDcmService NO_SUCH_SOP_CLASS_SCP =
            new DcmServiceBase(new DcmServiceException(NO_SUCH_SOP_CLASS));

        protected static DcmObjectFactory objFact = DcmObjectFactory.Instance;
        protected static AssociationFactory assocFact = AssociationFactory.Instance;
        protected static UniqueIdGenerator _uniqueIdGen = UniqueIdGenerator.Instance;
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

        #region IDcmService Members

        public virtual void c_store(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCStoreRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, requestCmd.AffectedSOPInstanceUniqueId, SUCCESS);
            try {
                DoCStore(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void c_get(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCGetRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, SUCCESS);
            try {
                DoMultiRsp(assoc, request, rspCmd, DoCGet(assoc, request, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_find(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCFindRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, PENDING);
            try {
                DoMultiRsp(assoc, request, rspCmd, DoCFind(assoc, request, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_move(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCMoveRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, PENDING);
            try {
                DoMultiRsp(assoc, request, rspCmd, DoCMove(assoc, request, rspCmd));
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
                IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
                assoc.Association.Write(rsp);
                DoAfterRsp(assoc, rsp);
            }
        }

        public virtual void c_echo(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCEchoRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, SUCCESS);
            try {
                DoCEcho(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                log.Error(e);
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_event_report(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNEventReportRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, requestCmd.AffectedSOPInstanceUniqueId, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNEventReport(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_get(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNGetRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNGet(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_set(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNSetRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNSet(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_action(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            var rspCmd = new DicomCommand();
            rspCmd.InitNActionRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNAction(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_create(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNCreateRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, CreateUID(requestCmd.AffectedSOPInstanceUniqueId),
                                  SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNCreate(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_delete(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNDeleteRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, SUCCESS);
            Dataset rspData = null;
            try {
                rspData = DoNDelete(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        #endregion

        protected virtual void DoAfterRsp(ActiveAssociation assoc, IDimse rsp) {}

        protected virtual void DoCStore(ActiveAssociation activeAssociation, IDimse request, DicomCommand responseCommand) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCGet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCFind(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual MultiDimseRsp DoCMove(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual void DoCEcho(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            //      request.getDataset(); // read out dataset
            throw defEx;
        }

        protected virtual Dataset DoNEventReport(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual Dataset DoNGet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual Dataset DoNSet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual Dataset DoNAction(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual Dataset DoNCreate(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        protected virtual Dataset DoNDelete(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out dataset, if any
            throw defEx;
        }

        // Private -------------------------------------------------------
        private void DoMultiRsp(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd, MultiDimseRsp mdr) {
            try {
                assoc.AddCancelListener(rspCmd.MessageIDToBeingRespondedTo, mdr.CancelListener);
                do {
                    Dataset rspData = mdr.next(assoc, request, rspCmd);
                    IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
                    assoc.Association.Write(rsp);
                    DoAfterRsp(assoc, rsp);
                } while (rspCmd.IsPending());
            }
            finally {
                mdr.release();
            }
        }

        private static String CreateUID(String uid) {
            return uid != null ? uid : _uniqueIdGen.CreateUniqueId();
        }

        // Inner classes -------------------------------------------------

        #region Nested type: MultiDimseRsp

        public interface MultiDimseRsp {
            IDimseListener CancelListener { get; }
            Dataset next(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd);
            void release();
        }

        #endregion
    }


    internal class VerificationSCP : DcmServiceBase {
        protected override void DoCEcho(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            rspCmd.PutUS(Tags.Status, SUCCESS);
        }
    }
}