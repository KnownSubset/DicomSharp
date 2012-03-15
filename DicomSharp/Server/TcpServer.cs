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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;

namespace DicomSharp.Server {
    /// <summary>
    /// SCP Server
    /// </summary>
    public class TcpServer {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TcpServer));
        private readonly IHandler _handler;
        private bool _stop;
        private TcpListener _tcpListener;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler"></param>
        public TcpServer(IHandler handler) {
            if (handler == null) {
                throw new NullReferenceException();
            }

            this._handler = handler;
        }

        public bool Stopped
        {
            get { return _tcpListener == null && _stop; }
        }

        public virtual void StartServer(int port) {
            CheckNotRunning();
            Logger.Info("Start Server listening at port " + port);

            // Create the TCP listener
            _tcpListener = new TcpListener(DetermineIpAddress(), port);
            _tcpListener.Start();

            // Fire the thread to listen for incoming associations
            Run();
        }

        private static IPAddress DetermineIpAddress()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            return ipAddress;
        }

        public virtual void StopServer() {
            if (_tcpListener == null) {
                return;
            }

            IPAddress ipAddress = ((IPEndPoint) _tcpListener.LocalEndpoint).Address;
            int port = ((IPEndPoint) _tcpListener.LocalEndpoint).Port;
            Logger.Info("Stop Server listening at port " + port);

            try {
                _tcpListener.Stop();
            }
            catch (IOException ignore)
            {
                Logger.Error(ignore);
            }

            // try to connect to server port to ensure to leave blocking accept
            try {
                new TcpClient(new IPEndPoint(ipAddress, port)).Close();
            }
            catch (IOException ioException)
            {
                Logger.Error(ioException);
            }
            _stop = true;
            _tcpListener = null;
        }

        /// <summary>
        /// Run the server
        /// </summary>
        public virtual void Run() {
            if (_tcpListener == null) {
                return;
            }

            TcpClient tcpClient = null;
            while (!_stop) {
                try {
                    tcpClient = _tcpListener.AcceptTcpClient();
                    if (Logger.IsInfoEnabled) {
                        Logger.Info("handle - " + tcpClient);
                    }

                    // Fire up a new pooled thread to handle this socket.
                    ThreadPool.QueueUserWorkItem(_handler.Handle, tcpClient);
                }
                catch (Exception ioe) {
                    Logger.Error(ioe);
                    if (tcpClient != null) {
                        try {
                            tcpClient.Close();
                        }
                        catch (Exception ignore)
                        {
                            Logger.Error(ignore);
                        }
                    }
                }
                if (Logger.IsInfoEnabled) {
                    Logger.Info("finished - " + tcpClient);
                }
            }
        }

        private void CheckNotRunning() {
            if (_tcpListener != null) {
                throw new SystemException("Already Running");
            }
        }

        #region Nested type: IHandler

        public interface IHandler {
            void Handle(Object s);
            bool IsSockedClosedByHandler();
        }

        #endregion
    }
}