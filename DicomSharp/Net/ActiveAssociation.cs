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
    public class ActiveAssociation : LF_ThreadPool.ThreadHandlerI, IActiveAssociation {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAssociation _association;
        private readonly Hashtable cancelDispatcher = new Hashtable();
        private readonly LF_ThreadPool m_threadPool;
        private readonly Hashtable rspDispatcher = new Hashtable();
        private readonly DcmServiceRegistry services;
        private bool m_released;
        private int timeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="association"> </param>
        /// <param name="services"></param>
        public ActiveAssociation(IAssociation association, DcmServiceRegistry services) {
            if (association.State != AssociationState.ASSOCIATION_ESTABLISHED) {
                throw new SystemException("Association not esrablished - " + association.State);
            }

            m_threadPool = new LF_ThreadPool(this);

            this._association = association;
            this.services = services;
            this._association.ActiveAssociation = this;
            this._association.SetThreadPool(m_threadPool);
        }

        public virtual int Timeout {
            get { return timeout; }
            set { timeout = value; }
        }

        public virtual IAssociation Association {
            get { return _association; }
        }

        #region ThreadHandlerI Members

        /// <summary>
        /// Run this active association
        /// </summary>
        public void Run(LF_ThreadPool pool) {
            try {
                lock (this) {
                    Dimse dimse = _association.Read(timeout);

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

                    IDicomCommand cmd = dimse.DicomCommand;
                    switch ((DicomCommandMessage)cmd.CommandField)
                    {
                        case DicomCommandMessage.C_STORE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).c_store(this, dimse);
                            break;

                        case DicomCommandMessage.C_GET_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).c_get(this, dimse);
                            break;

                        case DicomCommandMessage.C_FIND_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).c_find(this, dimse);
                            break;

                        case DicomCommandMessage.C_MOVE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).c_move(this, dimse);
                            break;

                        case DicomCommandMessage.C_ECHO_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).c_echo(this, dimse);
                            break;

                        case DicomCommandMessage.N_EVENT_REPORT_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).n_event_report(this, dimse);
                            break;

                        case DicomCommandMessage.N_GET_RQ:
                            services.Lookup(cmd.RequestedSOPClassUniqueId).n_get(this, dimse);
                            break;

                        case DicomCommandMessage.N_SET_RQ:
                            services.Lookup(cmd.RequestedSOPClassUniqueId).n_set(this, dimse);
                            break;

                        case DicomCommandMessage.N_ACTION_RQ:
                            services.Lookup(cmd.RequestedSOPClassUniqueId).n_action(this, dimse);
                            break;

                        case DicomCommandMessage.N_CREATE_RQ:
                            services.Lookup(cmd.AffectedSOPClassUniqueId).n_action(this, dimse);
                            break;

                        case DicomCommandMessage.N_DELETE_RQ:
                            services.Lookup(cmd.RequestedSOPClassUniqueId).n_delete(this, dimse);
                            break;

                        case DicomCommandMessage.C_STORE_RSP:
                        case DicomCommandMessage.C_GET_RSP:
                        case DicomCommandMessage.C_FIND_RSP:
                        case DicomCommandMessage.C_MOVE_RSP:
                        case DicomCommandMessage.C_ECHO_RSP:
                        case DicomCommandMessage.N_EVENT_REPORT_RSP:
                        case DicomCommandMessage.N_GET_RSP:
                        case DicomCommandMessage.N_SET_RSP:
                        case DicomCommandMessage.N_ACTION_RSP:
                        case DicomCommandMessage.N_CREATE_RSP:
                        case DicomCommandMessage.N_DELETE_RSP:
                            HandleResponse(dimse);
                            break;

                        case DicomCommandMessage.C_CANCEL_RQ:
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
        public void AddCancelListener(int msgID, IDimseListener l) {
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
        /// <param name="dimseListener"></param>
        public void Invoke(IDimse rq, IDimseListener dimseListener) {
            int msgID = rq.DicomCommand.MessageID;
            int maxOps = _association.MaxOpsInvoked;
            if (maxOps == 0) {
                rspDispatcher.Add(msgID, dimseListener);
            }
            else {
                lock (rspDispatcher) {
                    while (rspDispatcher.Count >= maxOps) {
                        Monitor.Wait(rspDispatcher);
                    }
                    rspDispatcher.Add(msgID, dimseListener);
                }
            }
            _association.Write(rq);
        }

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public FutureDimseResponse Invoke(IDimse rq) {
            var retval = new FutureDimseResponse();
            _association.AddAssociationListener(retval);
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
                (_association).WriteReleaseRQ();
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
            IDicomCommand cmd = dimse.DicomCommand;
            dimse.ReadDataset(); // read out dataset, if any
            int msgID = cmd.MessageIDToBeingRespondedTo;
            IDimseListener dimseListener = null;
            if (cmd.IsPending()) {
                dimseListener = (IDimseListener) rspDispatcher[msgID];
            }
            else {
                lock (rspDispatcher) {
                    dimseListener = (IDimseListener) rspDispatcher[msgID];
                    rspDispatcher.Remove(msgID);
                    Monitor.Pulse(rspDispatcher);
                }
            }

            if (dimseListener != null) {
                dimseListener.DimseReceived(_association, dimse);
            }
        }

        /// <summary>
        /// Handler DIMSE cancel request
        /// </summary>
        /// <param name="dimse"></param>
        private void HandleCancel(Dimse dimse) {
            IDicomCommand cmd = dimse.DicomCommand;
            int msgID = cmd.MessageIDToBeingRespondedTo;

            var l = (IDimseListener) cancelDispatcher[msgID];
            cancelDispatcher.Remove(msgID);

            if (l != null) {
                l.DimseReceived(_association, dimse);
            }
        }
    }
}