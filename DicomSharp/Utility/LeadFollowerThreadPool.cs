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
using System.Threading;
using log4net;

namespace DicomSharp.Utility {
    /// <summary>
    ///  Leader/Follower Thread Pool
    /// </summary>
    public class LeadFollowerThreadPool {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static int s_instCount;

        private readonly IThreadHandler m_handler;
        private readonly int m_instNo = ++s_instCount;
        private readonly Object m_mutex = new Object();
        private bool m_isShutdown;
        private Thread m_leader;
        private int m_maxRunning = Int32.MaxValue;
        private int m_running;

        //
        // This value determine how long should an idle thread wait before it's terminated.
        // For a pooled thread, the period should be shorter enough.
        //
        private int m_timeout = 10000; // 10 seconds
        private int m_waiting;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="m_handler"></param>
        public LeadFollowerThreadPool(IThreadHandler m_handler) {
            if (m_handler == null) {
                throw new NullReferenceException();
            }

            this.m_handler = m_handler;
        }

        public int Waiting {
            get { return m_waiting; }
        }

        public virtual int Running {
            get { return m_running; }
        }

        public bool IsShutdown {
            get { return m_isShutdown; }
        }

        public int MaxRunning {
            get { return m_maxRunning; }
            set {
                if (value <= 0) {
                    throw new ArgumentException("m_maxRunning: " + value);
                }

                m_maxRunning = value;
            }
        }

        public int Timeout {
            get { return m_timeout; }
            set { m_timeout = value; }
        }

        public override String ToString() {
            return "LeadFollowerThreadPool-" + m_instNo + " [m_leader: " +
                   (m_leader == null ? "null" : "#" + m_leader.GetHashCode().ToString()) + ", waiting: " + m_waiting +
                   ", running: " + m_running + "]";
        }

        /// <summary>
        /// ThreadPool Wrapper method for Join
        /// </summary>
        /// <param name="state"></param>
        public void Run(Object state) {
            Join();
        }

        /// <summary>
        /// Make the current m_leader running. For the first time, the caller thread becomes the m_leader
        /// </summary>
        public void Join() {
            while (!m_isShutdown && (m_waiting + m_running) < m_maxRunning) {
                lock (m_mutex) {
                    while (m_leader != null) {
                        Logger.Debug(this + " - #" + Thread.CurrentThread.GetHashCode().ToString() + " Enter Wait()");
                        ++m_waiting;
                        try {
                            if (!Monitor.Wait(m_mutex, m_timeout)) {
                                Logger.Debug(this + " - #" + Thread.CurrentThread.GetHashCode() + " Terminated");
                                return;
                            }
                        }
                        catch (ThreadInterruptedException ie) {
                            Logger.Error(ie.StackTrace);
                        }
                        finally {
                            --m_waiting;
                        }
                        Logger.Debug(this + " - " + Thread.CurrentThread.Name + " awaked");
                    }
                    if (m_isShutdown) {
                        return;
                    }

                    m_leader = Thread.CurrentThread;
                    Logger.Debug(this + " - #" + Thread.CurrentThread.GetHashCode() + " New Leader");
                }

                ++m_running;
                try {
                    do {
                        m_handler.Run(this);
                    } while (!m_isShutdown && m_leader == Thread.CurrentThread);
                }
                finally {
                    --m_running;
                }
            }
        }

        /// <summary>
        /// Promote a new m_leader thread
        /// </summary>
        /// <returns></returns>
        public virtual bool PromoteNewLeader() {
            if (m_isShutdown) {
                return false;
            }

            // only the current m_leader can promote the next m_leader
            if (m_leader != Thread.CurrentThread) {
                throw new SystemException();
            }

            m_leader = null;

            // notify (one) waiting thread in Join()
            lock (m_mutex) {
                if (m_waiting > 0) {
                    Logger.Debug(this + " - promote new m_leader by notify");
                    Monitor.Pulse(m_mutex);
                    return true;
                }
            }

            // if there is no waiting thread,
            // and the maximum number of running threads is not yet reached,
            if (m_running >= m_maxRunning) {
                Logger.Debug(this + " - Max number of threads reached");
                return false;
            }

            // start a new one
            Logger.Debug(this + " - promote new m_leader by add new Thread");

            AddThread();

            return true;
        }

        /// <summary>
        /// Shutdown this thread pool
        /// </summary>
        public virtual void Shutdown() {
            Logger.Debug(this + " - shutdown");
            m_isShutdown = true;
            m_leader = null;
            lock (m_mutex) {
                Monitor.PulseAll(m_mutex);
            }
        }

        /// <summary>
        /// Add this thread to the waiting list. This may be overloaded to take new thread from conventional thread pool
        /// </summary>
        protected virtual void AddThread() {
            //
            // Normal thread
            //
            //new Thread(new ThreadStart(this.Join)).Start();

            //
            // Pooled thread
            //
            ThreadPool.QueueUserWorkItem(Run);
        }

        #region Nested type: IThreadHandler

        /// <summary>
        /// Thread handling job
        /// </summary>
        public interface IThreadHandler {
            void Run(LeadFollowerThreadPool pool);
        }

        #endregion
    }
}