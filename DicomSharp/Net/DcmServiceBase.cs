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
using DicomSharp.Data;
using DicomSharp.Dictionary;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {
    public class DcmServiceBase : IDcmService {
        public const int Success = 0x0000;
        public const int Pending = 0xFF00;
        public const int NoSuchSOPClass = 0x0118;
        public const int UnrecognizeOperation = 0x0211;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DcmServiceBase));

        public static readonly IDcmService VerificationScp = new VerificationServiceClassProvider();

        public static readonly IDcmService NoSuchSOPClassScp = new DcmServiceBase(new DcmServiceException(NoSuchSOPClass));

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
            defEx = new DcmServiceException(UnrecognizeOperation);
        }

        #region IDcmService Members

        public virtual void c_store(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitCStoreRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, requestCmd.AffectedSOPInstanceUniqueId, Success);
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
            rspCmd.InitCGetRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, Success);
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
            rspCmd.InitCFindRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, Pending);
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
            rspCmd.InitCMoveRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, Pending);
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
            rspCmd.InitCEchoRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, Success);
            try {
                DoCEcho(assoc, request, rspCmd);
            }
            catch (DcmServiceException e) {
                Logger.Error(e);
                e.WriteTo(rspCmd);
            }
            IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd);
            assoc.Association.Write(rsp);
            DoAfterRsp(assoc, rsp);
        }

        public virtual void n_event_report(ActiveAssociation assoc, IDimse request) {
            IDicomCommand requestCmd = request.DicomCommand;
            DicomCommand rspCmd = objFact.NewCommand();
            rspCmd.InitNEventReportRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, requestCmd.AffectedSOPInstanceUniqueId, Success);
            DataSet rspData = null;
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
            rspCmd.InitNGetRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, Success);
            DataSet rspData = null;
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
            rspCmd.InitNSetRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, Success);
            DataSet rspData = null;
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
            rspCmd.InitNActionRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, Success);
            DataSet rspData = null;
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
            rspCmd.InitNCreateRSP(requestCmd.MessageID, requestCmd.AffectedSOPClassUniqueId, CreateUniqueId(requestCmd.AffectedSOPInstanceUniqueId),
                                  Success);
            DataSet rspData = null;
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
            rspCmd.InitNDeleteRSP(requestCmd.MessageID, requestCmd.RequestedSOPClassUniqueId, requestCmd.RequestedSOPInstanceUniqueId, Success);
            DataSet rspData = null;
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
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual IMultiDimseRsp DoCGet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual IMultiDimseRsp DoCFind(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual IMultiDimseRsp DoCMove(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual void DoCEcho(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            //      request.getDataset(); // read out DataSet
            throw defEx;
        }

        protected virtual DataSet DoNEventReport(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual DataSet DoNGet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual DataSet DoNSet(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual DataSet DoNAction(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual DataSet DoNCreate(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        protected virtual DataSet DoNDelete(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            request.ReadDataset(); // read out DataSet, if any
            throw defEx;
        }

        // Private -------------------------------------------------------
        private void DoMultiRsp(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd, IMultiDimseRsp mdr) {
            try {
                assoc.AddCancelListener(rspCmd.MessageIDToBeingRespondedTo, mdr.CancelListener);
                do {
                    DataSet rspData = mdr.Next(assoc, request, rspCmd);
                    IDimse rsp = assocFact.NewDimse(request.pcid(), rspCmd, rspData);
                    assoc.Association.Write(rsp);
                    DoAfterRsp(assoc, rsp);
                } while (rspCmd.IsPending());
            }
            finally {
                mdr.Release();
            }
        }

        private static String CreateUniqueId(String uid) {
            return uid ?? _uniqueIdGen.CreateUniqueId();
        }

        // Inner classes -------------------------------------------------

        #region Nested type: IMultiDimseRsp

        public interface IMultiDimseRsp {
            IDimseListener CancelListener { get; }
            DataSet Next(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd);
            void Release();
        }

        #endregion
    }


    internal class VerificationServiceClassProvider : DcmServiceBase {
        protected override void DoCEcho(ActiveAssociation assoc, IDimse request, DicomCommand rspCmd) {
            rspCmd.PutUS(Tags.Status, Success);
        }
    }
}