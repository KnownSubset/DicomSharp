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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IAssociation _association;
        private readonly Hashtable _cancelDispatcher = new Hashtable();
        private readonly LF_ThreadPool _threadPool;
        private readonly Hashtable _responseDispatcher = new Hashtable();
        private readonly DcmServiceRegistry _dcmServiceRegistry;
        private bool _released;
        private int _timeout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="association"> </param>
        /// <param name="dcmServiceRegistry"></param>
        public ActiveAssociation(IAssociation association, DcmServiceRegistry dcmServiceRegistry) {
            if (association.State != AssociationState.ASSOCIATION_ESTABLISHED) {
                throw new SystemException("Association not esrablished - " + association.State);
            }

            _threadPool = new LF_ThreadPool(this);

            this._association = association;
            this._dcmServiceRegistry = dcmServiceRegistry;
            this._association.ActiveAssociation = this;
            this._association.SetThreadPool(_threadPool);
        }

        public virtual int Timeout {
            get { return _timeout; }
            set { _timeout = value; }
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
                    Dimse dimse = _association.Read(_timeout);

                    // if Association was released
                    if (dimse == null) {
                        lock (_responseDispatcher) {
                            if (_responseDispatcher.Count != 0) {
                                _responseDispatcher.Clear();
                                _released = true;
                            }

                            Monitor.Pulse(_responseDispatcher);
                        }

                        pool.Shutdown();
                        return;
                    }

                    IDicomCommand cmd = dimse.DicomCommand;
                    switch ((DicomCommandMessage)cmd.CommandField)
                    {
                        case DicomCommandMessage.C_STORE_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).CStore(this, dimse);
                            break;

                        case DicomCommandMessage.C_GET_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).CGet(this, dimse);
                            break;

                        case DicomCommandMessage.C_FIND_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).CFind(this, dimse);
                            break;

                        case DicomCommandMessage.C_MOVE_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).CMove(this, dimse);
                            break;

                        case DicomCommandMessage.C_ECHO_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).CEcho(this, dimse);
                            break;

                        case DicomCommandMessage.N_EVENT_REPORT_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).NEventReport(this, dimse);
                            break;

                        case DicomCommandMessage.N_GET_RQ:
                            _dcmServiceRegistry.Lookup(cmd.RequestedSOPClassUniqueId).NGet(this, dimse);
                            break;

                        case DicomCommandMessage.N_SET_RQ:
                            _dcmServiceRegistry.Lookup(cmd.RequestedSOPClassUniqueId).NSet(this, dimse);
                            break;

                        case DicomCommandMessage.N_ACTION_RQ:
                            _dcmServiceRegistry.Lookup(cmd.RequestedSOPClassUniqueId).NAction(this, dimse);
                            break;

                        case DicomCommandMessage.N_CREATE_RQ:
                            _dcmServiceRegistry.Lookup(cmd.AffectedSOPClassUniqueId).NAction(this, dimse);
                            break;

                        case DicomCommandMessage.N_DELETE_RQ:
                            _dcmServiceRegistry.Lookup(cmd.RequestedSOPClassUniqueId).NDelete(this, dimse);
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
                Logger.Error(ioe);
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
            _cancelDispatcher.Add(msgID, l);
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
                _responseDispatcher.Add(msgID, dimseListener);
            }
            else {
                lock (_responseDispatcher) {
                    while (_responseDispatcher.Count >= maxOps) {
                        Monitor.Wait(_responseDispatcher);
                    }
                    _responseDispatcher.Add(msgID, dimseListener);
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
            lock (_responseDispatcher) {
                while (_responseDispatcher.Count != 0) {
                    Monitor.Wait(_responseDispatcher);
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
            if (!_released) {
                (_association).WriteReleaseRQ();
            }
        }

        /// <summary>
        /// ThreadPool wrapper method for Join
        /// </summary>
        /// <param name="state"></param>
        public void Run(Object state) {
            _threadPool.Join();
        }

        /// <summary>
        /// Handle DIMSE response
        /// </summary>
        /// <param name="dimse"></param>
        private void HandleResponse(Dimse dimse) {
            IDicomCommand cmd = dimse.DicomCommand;
            dimse.ReadDataset(); // read out DataSet, if any
            int msgID = cmd.MessageIDToBeingRespondedTo;
            IDimseListener dimseListener = null;
            if (cmd.IsPending()) {
                dimseListener = (IDimseListener) _responseDispatcher[msgID];
            }
            else {
                lock (_responseDispatcher) {
                    dimseListener = (IDimseListener) _responseDispatcher[msgID];
                    _responseDispatcher.Remove(msgID);
                    Monitor.Pulse(_responseDispatcher);
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

            var l = (IDimseListener) _cancelDispatcher[msgID];
            _cancelDispatcher.Remove(msgID);

            if (l != null) {
                l.DimseReceived(_association, dimse);
            }
        }
    }
}