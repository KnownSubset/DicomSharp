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
// 7/22/08: Solved bug by Maarten JB van Ettinger. A deadlock has been removed (changed lines 152-153, 178-187).

#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using DicomSharp.Data;
using DicomSharp.Utility;
using log4net;

namespace DicomSharp.Net {
    /// <summary>
    /// 
    /// </summary>
    public class ActiveAssociation : LF_ThreadPool.ThreadHandlerI {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Association assoc;
        private readonly Hashtable cancelDispatcher = new Hashtable();
        private readonly LF_ThreadPool m_threadPool;
        private readonly Hashtable rspDispatcher = new Hashtable();
        private readonly DcmServiceRegistry services;
        private bool m_released;
        private int timeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assoc"></param>
        /// <param name="services"></param>
        public ActiveAssociation(Association assoc, DcmServiceRegistry services) {
            if (assoc.State != Association.ASSOCIATION_ESTABLISHED) {
                throw new SystemException("Association not esrablished - " + assoc.State);
            }

            m_threadPool = new LF_ThreadPool(this);

            this.assoc = assoc;
            this.services = services;
            this.assoc.ActiveAssociation = this;
            this.assoc.SetThreadPool(m_threadPool);
        }

        public virtual int Timeout {
            get { return timeout; }
            set { timeout = value; }
        }

        public virtual Association Association {
            get { return assoc; }
        }

        #region ThreadHandlerI Members

        /// <summary>
        /// Run this active association
        /// </summary>
        public void Run(LF_ThreadPool pool) {
            try {
                lock (this) {
                    Dimse dimse = assoc.Read(timeout);

                    // if Association was released
                    if (dimse == null) {
                        lock (rspDispatcher) {
                            if (rspDispatcher.Count != 0) {
                                rspDispatcher.Clear();
                                m_released = true;
                            }

                            Monitor.Pulse(rspDispatcher);
                        }

                        pool.Shutdown();
                        return;
                    }

                    DicomCommand cmd = dimse.DicomCommand;
                    switch (cmd.CommandField) {
                        case DicomCommand.C_STORE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).c_store(this, dimse);
                            break;

                        case DicomCommand.C_GET_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).c_get(this, dimse);
                            break;

                        case DicomCommand.C_FIND_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).c_find(this, dimse);
                            break;

                        case DicomCommand.C_MOVE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).c_move(this, dimse);
                            break;

                        case DicomCommand.C_ECHO_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).c_echo(this, dimse);
                            break;

                        case DicomCommand.N_EVENT_REPORT_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).n_event_report(this, dimse);
                            break;

                        case DicomCommand.N_GET_RQ:
                            services.Lookup(cmd.RequestedSOPClassUID).n_get(this, dimse);
                            break;

                        case DicomCommand.N_SET_RQ:
                            services.Lookup(cmd.RequestedSOPClassUID).n_set(this, dimse);
                            break;

                        case DicomCommand.N_ACTION_RQ:
                            services.Lookup(cmd.RequestedSOPClassUID).n_action(this, dimse);
                            break;

                        case DicomCommand.N_CREATE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUID).n_action(this, dimse);
                            break;

                        case DicomCommand.N_DELETE_RQ:
                            services.Lookup(cmd.RequestedSOPClassUID).n_delete(this, dimse);
                            break;

                        case DicomCommand.C_STORE_RSP:
                        case DicomCommand.C_GET_RSP:
                        case DicomCommand.C_FIND_RSP:
                        case DicomCommand.C_MOVE_RSP:
                        case DicomCommand.C_ECHO_RSP:
                        case DicomCommand.N_EVENT_REPORT_RSP:
                        case DicomCommand.N_GET_RSP:
                        case DicomCommand.N_SET_RSP:
                        case DicomCommand.N_ACTION_RSP:
                        case DicomCommand.N_CREATE_RSP:
                        case DicomCommand.N_DELETE_RSP:
                            HandleResponse(dimse);
                            break;

                        case DicomCommand.C_CANCEL_RQ:
                            HandleCancel(dimse);
                            break;

                        default:
                            throw new SystemException("Illegal Command: " + cmd);
                    }
                }
            }
            catch (Exception ioe) {
                log.Error(ioe);
                pool.Shutdown();
            }
        }

        #endregion

        /// <summary>
        /// Add DIMSE message listener
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="l"></param>
        public void AddCancelListener(int msgID, DimseListenerI l) {
            cancelDispatcher.Add(msgID, l);
        }

        /// <summary>
        /// Start a new pooled thread for handling this active association
        /// </summary>
        public void Start() {
            ThreadPool.QueueUserWorkItem(Run);
        }

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="rq"></param>
        /// <param name="l"></param>
        public void Invoke(Dimse rq, DimseListenerI l) {
            int msgID = rq.DicomCommand.MessageID;
            int maxOps = assoc.MaxOpsInvoked;
            if (maxOps == 0) {
                rspDispatcher.Add(msgID, l);
            }
            else {
                lock (rspDispatcher) {
                    while (rspDispatcher.Count >= maxOps) {
                        Monitor.Wait(rspDispatcher);
                    }
                    rspDispatcher.Add(msgID, l);
                }
            }
            assoc.Write(rq);
        }

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public FutureRSP Invoke(Dimse rq) {
            var retval = new FutureRSP();
            assoc.AddAssociationListener(retval);
            Invoke(rq, retval);
            return retval;
        }

        /// <summary>
        /// Wait on all responses)
        /// </summary>
        public void WaitOnRSP() {
            lock (rspDispatcher) {
                while (rspDispatcher.Count != 0) {
                    Monitor.Wait(rspDispatcher);
                }
            }
        }

        /// <summary>
        /// Send association release request and release this association
        /// </summary>
        /// <param name="waitOnRSP"></param>
        public void Release(bool waitOnRSP) {
            if (waitOnRSP) {
                WaitOnRSP();
            }
            if (!m_released) {
                (assoc).WriteReleaseRQ();
            }
        }

        /// <summary>
        /// ThreadPool wrapper method for Join
        /// </summary>
        /// <param name="state"></param>
        public void Run(Object state) {
            m_threadPool.Join();
        }

        /// <summary>
        /// Handle DIMSE response
        /// </summary>
        /// <param name="dimse"></param>
        private void HandleResponse(Dimse dimse) {
            DicomCommand cmd = dimse.DicomCommand;
            Dataset ds = dimse.Dataset; // read out dataset, if any
            int msgID = cmd.MessageIDToBeingRespondedTo;
            DimseListenerI l = null;
            if (cmd.IsPending()) {
                l = (DimseListenerI) rspDispatcher[msgID];
            }
            else {
                lock (rspDispatcher) {
                    l = (DimseListenerI) rspDispatcher[msgID];
                    rspDispatcher.Remove(msgID);
                    Monitor.Pulse(rspDispatcher);
                }
            }

            if (l != null) {
                l.DimseReceived(assoc, dimse);
            }
        }

        /// <summary>
        /// Handler DIMSE cancel request
        /// </summary>
        /// <param name="dimse"></param>
        private void HandleCancel(Dimse dimse) {
            DicomCommand cmd = dimse.DicomCommand;
            int msgID = cmd.MessageIDToBeingRespondedTo;

            var l = (DimseListenerI) cancelDispatcher[msgID];
            cancelDispatcher.Remove(msgID);

            if (l != null) {
                l.DimseReceived(assoc, dimse);
            }
        }
    }
}