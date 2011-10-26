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

[assembly: log4net.Config.DOMConfigurator(ConfigFileExtension="config")]
namespace org.dicomcs.server
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Threading;
	using System.Net;
	using System.Net.Sockets;
	using log4net;

	/// <summary>
	/// SCP Server
	/// </summary>
	public class Server
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public interface HandlerI
		{
			void Handle(Object s);
			bool IsSockedClosedByHandler();
		}
		
		private HandlerI handler;
		private TcpListener ss;
		private int port = 104;
		private bool m_Stop = false;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handler"></param>
		public Server(HandlerI handler)
		{
			if (handler == null)
				throw new System.NullReferenceException();
			
			this.handler = handler;
		}
		
		public virtual void  Start(int port)
		{
			CheckNotRunning();
			log.Info("Start Server listening at port " + port);

			// Create the TCP listener
			ss = new TcpListener( port );
			ss.Start();

			// Fire the thread to listen for incoming associations
			Run();
		}
		
		public virtual void  Stop()
		{
			if (ss == null)
				return ;
			
			IPAddress ia = ((IPEndPoint) ss.LocalEndpoint).Address;
			int port = ((IPEndPoint) ss.LocalEndpoint).Port;
			log.Info("Stop Server listening at port " + port);
			
			try
			{
				ss.Stop();
			}
			catch (IOException ignore)
			{
			}
			
			// try to connect to server port to ensure to leave blocking accept
			try
			{
				new TcpClient(new IPEndPoint(ia, port)).Close();
			}
			catch (IOException ignore)
			{
			}
			m_Stop = true;
			ss = null;
		}
		
		/// <summary>
		/// Run the server
		/// </summary>
		public virtual void  Run()
		{
			if (ss == null)
				return ;
			
			TcpClient s = null;
			while( !m_Stop )
			{
				try
				{
					s = ss.AcceptTcpClient();
					if (log.IsInfoEnabled)
					{
						log.Info("handle - " + s);
					}
					
					// Fire up a new pooled thread to handle this socket.
					ThreadPool.QueueUserWorkItem(new WaitCallback(handler.Handle), s);
				}
				catch( Exception ioe)
				{
					log.Error(ioe);
					if (s != null)
					{
						try
						{
							s.Close();
						}
						catch (Exception ignore)
						{
						}
					}
				}
				if (log.IsInfoEnabled)
				{
					log.Info("finished - " + s);
				}
			}			
		}
		
		private void  CheckNotRunning()
		{
			if (ss != null)
			{
				throw new SystemException("Already Running");
			}
		}
	}
}