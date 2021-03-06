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
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {
    /// <summary>
    /// Finite State Mechine of DICOM communication
    /// </summary>
    public class Fsm {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Fsm));

        private readonly State STA1;
        private readonly State STA10;
        private readonly State STA11;
        private readonly State STA12;
        private readonly State STA13;
        private readonly State STA2;
        private readonly State STA3;
        private readonly State STA4;
        private readonly State STA5;
        private readonly State STA6;
        private readonly State STA7;
        private readonly State STA8;
        private readonly State STA9;

        private readonly Association assoc;
        private readonly bool requestor;
        private readonly TcpClient s;
        private readonly Stream stream;
        private AAbort aa;
        private AAssociateAC ac;
        private IAssociationListener assocListener;
        private AAssociateRJ rj;
        private AAssociateRQ rq;
        private State state;
        private int tcpCloseTimeout = 500;

        /// <summary>
        /// Creates a new instance of DcmULService 
        /// </summary>
        public Fsm(Association assoc, TcpClient s, bool requestor) {
            STA1 = new State1(this, AssociationState.IDLE);
            STA2 = new State2(this, AssociationState.AWAITING_READ_ASS_RQ);
            STA3 = new State3(this, AssociationState.AWAITING_WRITE_ASS_RP);
            STA4 = new State4(this, AssociationState.AWAITING_WRITE_ASS_RQ);
            STA5 = new State5(this, AssociationState.AWAITING_READ_ASS_RP);
            STA6 = new State6(this, AssociationState.ASSOCIATION_ESTABLISHED);
            STA7 = new State7(this, AssociationState.AWAITING_READ_REL_RP);
            STA8 = new State8(this, AssociationState.AWAITING_WRITE_REL_RP);
            STA9 = new State9(this, AssociationState.RCRS_AWAITING_WRITE_REL_RP);
            STA10 = new State10(this, AssociationState.RCAS_AWAITING_READ_REL_RP);
            STA11 = new State11(this, AssociationState.RCRS_AWAITING_READ_REL_RP);
            STA12 = new State12(this, AssociationState.RCAS_AWAITING_WRITE_REL_RP);
            STA13 = new State13(this, AssociationState.ASSOCIATION_TERMINATING);
            state = STA1;

            this.assoc = assoc;
            this.requestor = requestor;
            this.s = s;
            stream = s.GetStream();
            Logger.Info(s.ToString());
            ChangeState(requestor ? STA4 : STA2);
        }

        public virtual LeadFollowerThreadPool ReaderThreadPool { get; set; }

        /// Properties
        ///////////////////////////////////////////////////////////////////////
        public virtual int TCPCloseTimeout {
            get { return tcpCloseTimeout; }
            set {
                if (value < 0) {
                    throw new ArgumentException("tcpCloseTimeout:" + value);
                }
                tcpCloseTimeout = value;
            }
        }

        public virtual String StateAsString {
            get { return state.ToString(); }
        }

        public virtual AAssociateRQ AAssociateRQ {
            get { return rq; }
        }

        public virtual String CallingAET {
            get {
                if (rq == null) {
                    throw new SystemException(state.ToString());
                }
                return rq.Name;
            }
        }

        public virtual String CalledAET {
            get {
                if (rq == null) {
                    throw new SystemException(state.ToString());
                }
                return rq.ApplicationEntityTitle;
            }
        }

        public virtual AAssociateAC AAssociateAC {
            get { return ac; }
        }

        public virtual AAssociateRJ AAssociateRJ {
            get { return rj; }
        }

        public virtual AAbort AAbort {
            get { return aa; }
        }

        public virtual int WriteMaxLength {
            get {
                if (ac == null || rq == null) {
                    throw new SystemException(state.ToString());
                }
                return requestor ? ac.MaxPduLength : rq.MaxPduLength;
            }
        }

        public virtual int ReadMaxLength {
            get {
                if (ac == null || rq == null) {
                    throw new SystemException(state.ToString());
                }
                return requestor ? rq.MaxPduLength : ac.MaxPduLength;
            }
        }

        public virtual int MaxOpsInvoked {
            get {
                if (ac == null) {
                    throw new SystemException(state.ToString());
                }
                AsyncOpsWindow aow = ac.AsyncOpsWindow;
                if (aow == null) {
                    return 1;
                }
                return requestor ? aow.MaxOpsInvoked : aow.MaxOpsPerformed;
            }
        }

        public virtual int MaxOpsPerformed {
            get {
                if (ac == null) {
                    throw new SystemException(state.ToString());
                }
                AsyncOpsWindow aow = ac.AsyncOpsWindow;
                if (aow == null) {
                    return 1;
                }
                return requestor ? aow.MaxOpsPerformed : aow.MaxOpsInvoked;
            }
        }

        public void AddAssociationListener(IAssociationListener l) {
            lock (this) {
                assocListener = Multicaster.add(assocListener, l);
            }
        }

        public void RemoveAssociationListener(IAssociationListener l) {
            lock (this) {
                assocListener = Multicaster.Remove(assocListener, l);
            }
        }

        internal TcpClient Socket() {
            return s;
        }

        internal bool IsRequestor() {
            return requestor;
        }

        internal String GetAcceptedTransferSyntaxUID(int pcid) {
            if (ac == null) {
                throw new SystemException(state.ToString());
            }
            PresentationContext pc = ac.GetPresentationContext(pcid);
            if (pc == null || pc.result() != PresentationContext.ACCEPTANCE) {
                return null;
            }
            return pc.TransferSyntaxUID;
        }

        internal PresentationContext GetAcceptedPresContext(String asuid, String tsuid) {
            if (ac == null) {
                throw new SystemException(state.ToString());
            }
            for (IEnumerator enu = rq.ListPresContext().GetEnumerator(); enu.MoveNext();) {
                var rqpc = (PresentationContext) enu.Current;
                if (asuid.Equals(rqpc.AbstractSyntaxUID)) {
                    PresentationContext acpc = ac.GetPresentationContext(rqpc.pcid());
                    if (acpc != null && acpc.result() == PresentationContext.ACCEPTANCE && tsuid.Equals(acpc.TransferSyntaxUID)) {
                        return acpc;
                    }
                }
            }
            return null;
        }

        public ArrayList ListAcceptedPresContext(String asuid) {
            if (ac == null) {
                throw new SystemException(state.ToString());
            }
            var list = new ArrayList();
            for (IEnumerator enu = rq.ListPresContext().GetEnumerator(); enu.MoveNext();) {
                var rqpc = (PresentationContext) enu.Current;
                if (asuid.Equals(rqpc.AbstractSyntaxUID)) {
                    PresentationContext acpc = ac.GetPresentationContext(rqpc.pcid());
                    if (acpc != null && acpc.result() == PresentationContext.ACCEPTANCE) {
                        list.Add(acpc);
                    }
                }
            }
            return list;
        }

        public int CountAcceptedPresContext() {
            if (ac == null) {
                throw new SystemException(state.ToString());
            }
            return ac.countAcceptedPresContext();
        }

        private void ChangeState(State state) {
            if (this.state != state) {
                this.state = state;
                state.Entry();
                if (Logger.IsInfoEnabled) {
                    Logger.Info(state.ToString());
                }
            }
        }

        /// <summary>
        /// Read from network socket
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public IPdu Read(int timeout, byte[] buf) {
            try {
                UnparsedPdu raw = null;

                s.ReceiveTimeout = timeout;
                try {
                    raw = new UnparsedPdu(stream, buf);
                }
                catch (IOException e) {
                    ChangeState(STA1);
                    throw e;
                }
                return raw != null ? state.Parse(raw) : null;
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        public void Write(AAssociateRQ rq) {
            FireWrite(rq);
            try {
                lock (stream) {
                    state.Write(rq);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
            this.rq = rq;
        }

        public void Write(AAssociateAC ac) {
            FireWrite(ac);
            try {
                lock (stream) {
                    state.Write(ac);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
            this.ac = ac;
        }

        public void Write(AAssociateRJ rj) {
            FireWrite(rj);
            try {
                lock (stream) {
                    state.Write(rj);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        public void Write(PDataTF data) {
            FireWrite(data);
            try {
                lock (stream) {
                    state.Write(data);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        public void Write(AReleaseRQ rq) {
            FireWrite(rq);
            try {
                lock (stream) {
                    state.Write(rq);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        public void Write(AReleaseRP rp) {
            FireWrite(rp);
            try {
                lock (stream) {
                    state.Write(rp);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        public void Write(AAbort abort) {
            FireWrite(abort);
            try {
                lock (stream) {
                    state.Write(abort);
                }
            }
            catch (IOException ioe) {
                if (assocListener != null) {
                    assocListener.Error(assoc, ioe);
                }
                throw ioe;
            }
        }

        internal void FireReceived(Dimse dimse) {
            if (Logger.IsInfoEnabled) {
                Logger.Info("received " + dimse);
            }
            if (assocListener != null) {
                assocListener.Received(assoc, dimse);
            }
        }

        internal void FireWrite(IDimse dimse) {
            if (Logger.IsInfoEnabled) {
                Logger.Info("sending " + dimse);
            }
            if (assocListener != null) {
                assocListener.Write(assoc, dimse);
            }
        }

        private void FireWrite(IPdu pdu) {
            if (pdu is PDataTF) {
                if (Logger.IsDebugEnabled) {
                    Logger.Debug("sending " + pdu);
                }
            }
            else {
                if (Logger.IsInfoEnabled) {
                    Logger.Info("sending " + pdu.ToString(Logger.IsDebugEnabled));
                }
            }
            if (assocListener != null) {
                assocListener.Write(assoc, pdu);
            }
        }

        private IPdu FireReceived(IPdu pdu) {
            if (pdu is PDataTF) {
                if (Logger.IsDebugEnabled) {
                    Logger.Debug("received " + pdu);
                }
            }
            else {
                if (Logger.IsInfoEnabled) {
                    Logger.Info("received " + pdu.ToString(Logger.IsDebugEnabled));
                }
            }
            if (assocListener != null) {
                assocListener.Received(assoc, pdu);
            }
            return pdu;
        }

        public virtual int GetState() {
            return state.Type;
        }

        #region Nested type: State

        /// <summary>
        /// State
        /// </summary>
        internal abstract class State {
            private readonly int type;
            protected Fsm m_fsm;

            internal State(Fsm fsm, AssociationState type){
                m_fsm = fsm;
                this.type = (int) type;
            }

            internal State(Fsm fsm, int type) {
                m_fsm = fsm;
                this.type = type;
            }

            public virtual int Type {
                get { return type; }
            }

            public virtual bool IsOpen() {
                return false;
            }

            public virtual bool CanWritePDataTF() {
                return false;
            }

            public virtual bool CanReadPDataTF() {
                return false;
            }

            internal virtual void Entry() {}

            internal virtual IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }

            internal virtual void Write(AAssociateRQ rq) {
                throw new SystemException();
            }

            internal virtual void Write(AAssociateAC ac) {
                throw new SystemException();
            }

            internal virtual void Write(AAssociateRJ rj) {
                throw new SystemException();
            }

            internal virtual void Write(PDataTF data) {
                throw new SystemException();
            }

            internal virtual void Write(AReleaseRQ rq) {
                throw new SystemException();
            }

            internal virtual void Write(AReleaseRP rp) {
                throw new SystemException();
            }

            internal virtual void Write(AAbort abort) {
                try {
                    abort.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA13);
            }
        }

        #endregion

        #region Nested type: State1

        /// <summary>
        /// Sta 1 - Idle
        /// </summary>
        internal class State1 : State {
            internal State1(Fsm fsm, AssociationState type) : base(fsm, (int) type) {}

            public override String ToString() {
                return "Sta 1 - Idle";
            }

            internal override void Entry() {
                if (m_fsm.ReaderThreadPool != null) {
                    m_fsm.ReaderThreadPool.Shutdown(); // stop reading
                }

                if (m_fsm.assocListener != null) {
                    m_fsm.assocListener.Close(m_fsm.assoc);
                }

                if (Logger.IsInfoEnabled) {
                    Logger.Info("closing connection - " + m_fsm.s);
                }
                try {
                    m_fsm.stream.Close();
                    m_fsm.s.Close();
                }
                catch (IOException ignore)
                {
                    Logger.Error(ignore);
                }
            }

            internal override void Write(AAbort abort) {}
        }

        #endregion

        #region Nested type: State10

        /// <summary>
        /// Sta 10 - Release collision acceptor side; awaiting A-RELEASE response
        /// </summary>
        internal class State10 : State {
            internal State10(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 10 - Release collision acceptor side; awaiting A-RELEASE response";
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 6:
                            IPdu pdu = m_fsm.FireReceived(AReleaseRP.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA12);
                            return pdu;

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }
        }

        #endregion

        #region Nested type: State11

        /// <summary>
        /// Sta 11 - Release collision requestor side; awaiting A-RELEASE-RP PDU
        /// </summary>
        internal class State11 : State {
            internal State11(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 11 - Release collision requestor side; awaiting A-RELEASE-RP PDU";
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 6:
                            IPdu pdu = m_fsm.FireReceived(AReleaseRP.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return pdu;

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }
        }

        #endregion

        #region Nested type: State12

        /// <summary>
        /// Sta 12 - Release collision acceptor side; awaiting A-RELEASE-RP PDU
        /// </summary>
        internal class State12 : State {
            internal State12(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 12 - Release collision acceptor side; awaiting A-RELEASE-RP PDU";
            }

            internal override void Write(AReleaseRP rp) {
                try {
                    rp.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA13);
            }
        }

        #endregion

        #region Nested type: State13

        /// <summary>
        /// Sta 13 - Awaiting Transport Connection Close Indication
        /// </summary>
        internal class State13 : State {
            internal State13(Fsm fsm, AssociationState state) : base(fsm, state) { }

            internal void TimeoutIt(Object state) {
                NDC.Push(m_fsm.assoc.Name);
                m_fsm.ChangeState(m_fsm.STA1);
                NDC.Pop();
            }

            public override String ToString() {
                return "Sta 13 - Awaiting Transport Connection Close Indication";
            }

            internal override void Entry() {
                if (m_fsm.ReaderThreadPool != null) {
                    m_fsm.ReaderThreadPool.Shutdown();
                }
            }
        }

        #endregion

        #region Nested type: State2

        /// <summary>
        /// Sta 2 - Transport connection open (Awaiting A-ASSOCIATE-RQ PDU)
        /// </summary>
        internal class State2 : State {
            internal State2(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 2 - Transport connection open (Awaiting A-ASSOCIATE-RQ PDU)";
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                            m_fsm.FireReceived(m_fsm.rq = AAssociateRQ.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA3);
                            return m_fsm.rq;

                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }
        }

        #endregion

        #region Nested type: State3

        /// <summary>
        /// Sta 3 - Awaiting local A-ASSOCIATE response primitive
        /// </summary>
        internal class State3 : State {
            internal State3(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 3 - Awaiting local A-ASSOCIATE response primitive";
            }

            internal override void Write(AAssociateAC ac) {
                try {
                    ac.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA6);
            }

            internal override void Write(AAssociateRJ rj) {
                try {
                    rj.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA13);
            }
        }

        #endregion

        #region Nested type: State4

        /// <summary>
        /// Sta 4 - Awaiting transport connection opening to complete
        /// </summary>
        internal class State4 : State {
            internal State4(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 4 - Awaiting transport connection opening to complete";
            }

            internal override void Write(AAssociateRQ rq) {
                try {
                    rq.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA5);
            }

            internal override void Write(AAbort abort) {
                m_fsm.ChangeState(m_fsm.STA1);
            }
        }

        #endregion

        #region Nested type: State5

        /// <summary>
        /// Sta 5 - Awaiting A-ASSOCIATE-AC or A-ASSOCIATE-RJ PDU
        /// </summary>
        internal class State5 : State {
            internal State5(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 5 - Awaiting A-ASSOCIATE-AC or A-ASSOCIATE-RJ PDU";
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 2:
                            m_fsm.FireReceived(m_fsm.ac = AAssociateAC.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA6);
                            return m_fsm.ac;

                        case 3:
                            m_fsm.FireReceived(m_fsm.rj = AAssociateRJ.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA13);
                            return m_fsm.rj;

                        case 4:
                        case 5:
                        case 6:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }
        }

        #endregion

        #region Nested type: State6

        /// <summary>
        /// Sta 6 - Association established and Ready for data transfer
        /// </summary>
        internal class State6 : State {
            internal State6(Fsm fsm, AssociationState state) : base(fsm, state) {}

            public override String ToString() {
                return "Sta 6 - Association established and Ready for data transfer";
            }

            public override bool IsOpen() {
                return true;
            }

            public override bool CanWritePDataTF() {
                return true;
            }

            public override bool CanReadPDataTF() {
                return true;
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                        case 2:
                        case 3:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 4:
                            return m_fsm.FireReceived(PDataTF.Parse(raw));

                        case 5:
                            IPdu pdu = m_fsm.FireReceived(AReleaseRQ.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA8);
                            return pdu;

                        case 6:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }

            internal override void Write(PDataTF tf) {
                try {
                    tf.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
            }

            internal override void Write(AReleaseRQ rq) {
                try {
                    m_fsm.ChangeState(m_fsm.STA7);
                    rq.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
            }
        }

        #endregion

        #region Nested type: State7

        /// <summary>
        /// Sta 7 - Awaiting A-RELEASE-RP PDU
        /// </summary>
        internal class State7 : State {
            internal State7(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 7 - Awaiting A-RELEASE-RP PDU";
            }

            public override bool CanReadPDataTF() {
                return true;
            }

            internal override IPdu Parse(UnparsedPdu raw) {
                try {
                    switch (raw.GetType()) {
                        case 1:
                        case 2:
                        case 3:
                            throw new PduException("Unexpected " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNEXPECTED_PDU));

                        case 4:
                            return m_fsm.FireReceived(PDataTF.Parse(raw));

                        case 5:
                            IPdu pdu = m_fsm.FireReceived(AReleaseRQ.Parse(raw));
                            m_fsm.ChangeState(m_fsm.requestor ? m_fsm.STA9 : m_fsm.STA10);
                            return pdu;

                        case 6:
                            m_fsm.FireReceived(pdu = AReleaseRP.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return pdu;

                        case 7:
                            m_fsm.FireReceived(m_fsm.aa = AAbort.Parse(raw));
                            m_fsm.ChangeState(m_fsm.STA1);
                            return m_fsm.aa;

                        default:
                            throw new PduException("Unrecognized " + raw,
                                                   new AAbort(AAbort.SERVICE_PROVIDER, AAbort.UNRECOGNIZED_PDU));
                    }
                }
                catch (PduException ule) {
                    try {
                        Write(ule.AAbort);
                    }
                    catch (Exception) {}
                    throw ule;
                }
            }
        }

        #endregion

        #region Nested type: State8

        /// <summary>
        /// Sta 8 - Awaiting local A-RELEASE response primitive
        /// </summary>
        internal class State8 : State {
            internal State8(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 8 - Awaiting local A-RELEASE response primitive";
            }

            public override bool CanWritePDataTF() {
                return true;
            }

            internal override void Write(PDataTF tf) {
                try {
                    tf.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
            }

            internal override void Write(AReleaseRP rp) {
                try {
                    rp.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA13);
            }
        }

        #endregion

        #region Nested type: State9

        /// <summary>
        /// Sta 9 - Release collision requestor side; awaiting A-RELEASE response
        /// </summary>
        internal class State9 : State {
            internal State9(Fsm fsm, AssociationState state) : base(fsm, state) { }

            public override String ToString() {
                return "Sta 9 - Release collision requestor side; awaiting A-RELEASE response";
            }

            internal override void Write(AReleaseRP rp) {
                try {
                    rp.WriteTo(m_fsm.stream);
                }
                catch (IOException e) {
                    m_fsm.ChangeState(m_fsm.STA1);
                    throw e;
                }
                m_fsm.ChangeState(m_fsm.STA11);
            }
        }

        #endregion
    }
}