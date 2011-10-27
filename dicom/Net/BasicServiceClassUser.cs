#region Copyright
// 
// This library is based on dcm4che see http://www.sourceforge.net/projects/dcm4che
// Copyright (c) 2002 by TIANI MEDGRAPH AG. All rights reserved.
//
// Modifications Copyright (C) 2002,2008 Fang Yang. All rights reserved.
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
// 8/29/08: Added BasicSCU by Maarten JB van Ettinger based on TestSCU. Added a couple of SCU functions.
#endregion

namespace org.dicomcs.net
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using System.Net.Sockets;
	using org.dicomcs.dict;
	using org.dicomcs.data;
	using org.dicomcs.net;

	/// <summary>
	/// Summary description for BasicSCU.
	/// </summary>
	public class BasicSCU
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static AssociationFactory aFact = AssociationFactory.Instance;
		private static DcmObjectFactory oFact= DcmObjectFactory.Instance;
		private static DcmParserFactory pFact= DcmParserFactory.Instance;

		private static String[] DEF_TS = new String[]{UIDs.ImplicitVRLittleEndian};
		private int PCID_START = 1;

		private AAssociateRQ m_assocRQ = aFact.NewAAssociateRQ();
		private String m_host = "localhost";
		private int    m_port = 104;
		private int m_assocTimeOut = 0;
		private string m_tsUID = UIDs.ExplicitVRLittleEndian;

		public BasicSCU(String callingAET, String calledAET, String host, int port)
		{
			m_assocRQ.CalledAET = calledAET;
			m_assocRQ.CallingAET = callingAET;
			m_assocRQ.AsyncOpsWindow = aFact.NewAsyncOpsWindow(0, 1);
			m_assocRQ.MaxPduLength = 16352;
			m_host = host;
			m_port = port;
		}

		public BasicSCU(String callingAET, String calledAET, String host, int port, int timeout) : this(callingAET, calledAET, host, port)
		{
			m_assocTimeOut = timeout;
		}

		private ActiveAssociation OpenAssoc( )
		{
			Association assoc = aFact.NewRequestor(new TcpClient(m_host, m_port));

			try
			{
				PduI assocAC = assoc.Connect(m_assocRQ, m_assocTimeOut);
				if (!(assocAC is AAssociateAC))
				{
					return null;
				}
			}
			catch
			{
				return null;
			}

			ActiveAssociation active = aFact.NewActiveAssociation(assoc, null);

			active.Timeout = m_assocTimeOut;
			active.Start();
			return active;
		}

		/// <summary>
		/// Send C-ECHO
		/// </summary>
		public void CEcho()
		{
			int pcid = PCID_START;
			PCID_START += 2;

			try
			{
				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, UIDs.Verification, DEF_TS));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					if (active.Association.GetAcceptedTransferSyntaxUID(pcid) == null)
					{
						log.Error( "Verification SOP class is not supported" );
					}
					else
					{
						active.Invoke(aFact.NewDimse(pcid, oFact.NewCommand().InitCEchoRQ(0)), null);
					}
				
					active.Release(true);
				}
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}
		}

		/// <summary>
		/// Send C-FIND for study
		/// </summary>
		public Dataset[] CFindStudy(string patid, string[] modalities)
		{
			int pcid = PCID_START;
			PCID_START += 2;

			Dataset[] ret = null;

			try
			{
				string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, sopClassUID, new String[]{ m_tsUID }));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					Dataset ds = new Dataset();

					FileMetaInfo fmi = new FileMetaInfo();
					fmi.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
					fmi.PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
					fmi.PutUI(Tags.TransferSyntaxUID, m_tsUID);
					fmi.PutSH(Tags.ImplementationVersionName, "dicomcs-SCU");

					ds.SetFileMetaInfo(fmi);

					ds.PutDA(Tags.StudyDate);
					ds.PutTM(Tags.StudyTime);
					ds.PutSH(Tags.AccessionNumber);
					ds.PutCS(Tags.QueryRetrieveLevel, "STUDY");
					ds.PutCS(Tags.ModalitiesInStudy, modalities);
					ds.PutLO(Tags.InstitutionName);
					ds.PutPN(Tags.ReferringPhysicianName);
					ds.PutLO(Tags.StudyDescription);
					ds.PutPN(Tags.PatientName);
					ds.PutLO(Tags.PatientID, patid);
					ds.PutDA(Tags.PatientBirthDate);
					ds.PutCS(Tags.PatientSex);
					ds.PutAS(Tags.PatientAge);
					ds.PutUI(Tags.StudyInstanceUID);

					PresContext pc = null;
					Association assoc = active.Association;

					if ((ds.GetFileMetaInfo() != null)
					&&	(ds.GetFileMetaInfo().TransferSyntaxUID != null))
					{
						String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
						if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
						{
							log.Error( "SOP class UID not supported" );
							return null;
						}
					}
					else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}

					Dimse rq = aFact.NewDimse(pcid, oFact.NewCommand().InitCFindRQ(assoc.NextMsgID(), sopClassUID, 0), ds);
					 
					FutureRSP rsp = active.Invoke(rq);	
					active.Release(true);

					if (rsp.IsReady())
					{
						ArrayList al = rsp.ListPending();

						if (al.Count > 0)
						{
							int len = al.Count;

							ret = new Dataset[len];

							for (int i=0;i < len;i++)
								ret[i] = (Dataset) ((Dimse) al[i]).Dataset;
						}
					}
				}
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}

			return ret;
		}

		/// <summary>
		/// Send C-FIND for series
		/// </summary>
		public Dataset[] CFindSeries(string studyInstanceUID, string[] modalities)
		{
			int pcid = PCID_START;
			PCID_START += 2;

			Dataset[] ret = null;

			try
			{
				string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, sopClassUID, new String[]{ m_tsUID }));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					Dataset ds = new Dataset();

					FileMetaInfo fmi = new FileMetaInfo();
					fmi.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
					fmi.PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
					fmi.PutUI(Tags.TransferSyntaxUID, m_tsUID);
					fmi.PutSH(Tags.ImplementationVersionName, "dicomcs-SCU");

					ds.SetFileMetaInfo(fmi);

					ds.PutCS(Tags.QueryRetrieveLevel, "SERIES");
					ds.PutCS(Tags.Modality);
					ds.PutUI(Tags.StudyInstanceUID, studyInstanceUID);
					ds.PutUI(Tags.SeriesInstanceUID);

					PresContext pc = null;
					Association assoc = active.Association;

					if ((ds.GetFileMetaInfo() != null)
					&&	(ds.GetFileMetaInfo().TransferSyntaxUID != null))
					{
						String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
						if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
						{
							log.Error( "SOP class UID not supported" );
							return null;
						}
					}
					else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}

					Dimse rq = aFact.NewDimse(pcid, oFact.NewCommand().InitCFindRQ(assoc.NextMsgID(), sopClassUID, 0), ds);
					 
					FutureRSP rsp = active.Invoke(rq);	
					active.Release(true);

					if (rsp.IsReady())
					{
						ArrayList al = rsp.ListPending();

						if ((al != null)
						&&	(al.Count > 0)
						&&	(modalities != null)
						&&	(modalities.Length > 0))
						{
							for (int i=0;i < al.Count;i++)
							{
								try
								{
									string mod = ((Dimse) al[i]).Dataset.GetString(Tags.Modality);
									int j=0;

									for (;j < modalities.Length;j++)
										if (string.Compare(modalities[j], mod) == 0)
											break;

									if (j == modalities.Length)
										al.RemoveAt(i--);
								}
								catch
								{
									al.RemoveAt(i--);
								}
							}
						}

						if (al.Count > 0)
						{
							int len = al.Count;

							ret = new Dataset[len];

							for (int i=0;i < len;i++)
								ret[i] = (Dataset) ((Dimse) al[i]).Dataset;
						}
					}
				}
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}

			return ret;
		}

		/// <summary>
		/// Send C-FIND for instance
		/// </summary>
		public Dataset[] CFindInstance(string seriesInstanceUID)
		{
			int pcid = PCID_START;
			PCID_START += 2;

			Dataset[] ret = null;

			try
			{
				string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelFIND;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, sopClassUID, new String[]{ m_tsUID }));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					Dataset ds = new Dataset();

					FileMetaInfo fmi = new FileMetaInfo();
					fmi.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
					fmi.PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
					fmi.PutUI(Tags.TransferSyntaxUID, m_tsUID);
					fmi.PutSH(Tags.ImplementationVersionName, "dicomcs-SCU");

					ds.SetFileMetaInfo(fmi);

					ds.PutUI(Tags.SOPInstanceUID);
					ds.PutCS(Tags.QueryRetrieveLevel, "IMAGE");
					ds.PutUI(Tags.SeriesInstanceUID, seriesInstanceUID);

					PresContext pc = null;
					Association assoc = active.Association;

					if ((ds.GetFileMetaInfo() != null)
					&&	(ds.GetFileMetaInfo().TransferSyntaxUID != null))
					{
						String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
						if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
						{
							log.Error( "SOP class UID not supported" );
							return null;
						}
					}
					else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}

					Dimse rq = aFact.NewDimse(pcid, oFact.NewCommand().InitCFindRQ(assoc.NextMsgID(), sopClassUID, 0), ds);
					 
					FutureRSP rsp = active.Invoke(rq);	
					active.Release(true);

					if (rsp.IsReady())
					{
						ArrayList al = rsp.ListPending();

						if (al.Count > 0)
						{
							int len = al.Count;

							ret = new Dataset[len];

							for (int i=0;i < len;i++)
								ret[i] = (Dataset) ((Dimse) al[i]).Dataset;
						}
					}
				}
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}

			return ret;
		}

		/// <summary>
		/// Send C-GET
		/// </summary>
		public Dataset[] CGet(string studyInstanceUID, string seriesInstanceUID, string sopInstanceUID)
		{
			if ((studyInstanceUID == null)
			&&	(seriesInstanceUID == null)
			&&	(sopInstanceUID == null))
				return null;

			int pcid = PCID_START;
			PCID_START += 2;

			Dataset[] ret = null;

			try
			{
				string sopClassUID = UIDs.StudyRootQueryRetrieveInformationModelGET;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, sopClassUID, new String[]{ m_tsUID }));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					Dataset ds = new Dataset();

					FileMetaInfo fmi = new FileMetaInfo();
					fmi.PutOB(Tags.FileMetaInformationVersion, new byte[] {0, 1});
					fmi.PutUI(Tags.MediaStorageSOPClassUID, sopClassUID);
					fmi.PutUI(Tags.TransferSyntaxUID, m_tsUID);
					fmi.PutSH(Tags.ImplementationVersionName, "dicomcs-SCU");

					ds.SetFileMetaInfo(fmi);

					if (studyInstanceUID != null)
						ds.PutUI(Tags.StudyInstanceUID, studyInstanceUID);

					if (seriesInstanceUID != null)
						ds.PutUI(Tags.SeriesInstanceUID, seriesInstanceUID);

					if (sopInstanceUID != null)
						ds.PutUI(Tags.SOPInstanceUID, sopInstanceUID);

					PresContext pc = null;
					Association assoc = active.Association;

					if ((ds.GetFileMetaInfo() != null)
					&&	(ds.GetFileMetaInfo().TransferSyntaxUID != null))
					{
						String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
						if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
						{
							log.Error( "SOP class UID not supported" );
							return null;
						}
					}
					else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}

					Dimse rq = aFact.NewDimse(pcid, oFact.NewCommand().InitCGetRQ(assoc.NextMsgID(), sopClassUID, 0), ds);
					 
					FutureRSP rsp = active.Invoke(rq);	
					active.Release(true);

					if (rsp.IsReady())
					{
						ArrayList al = rsp.ListPending();

						if (al.Count > 0)
						{
							ret = new Dataset[al.Count];

							for (int i=0;i < ret.Length;i++)
								ret[i] = ((Dimse)al[i]).Dataset;
						}
						else
						{
							ret = new Dataset[1];
							ret[0] = rsp.Get().Dataset;
						}
					}

					m_assocRQ.RemovePresContext(pcid);
				}
			}
			catch
			{
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}

			return ret;
		}

		/// <summary>
		/// Perform a WADO-GET
		/// </summary>
		public Dataset WADOGet(string url, string studyInstanceUID, string seriesInstanceUID, string sopInstanceUID)
		{
			if ((studyInstanceUID != null)
			&&	(seriesInstanceUID == null)
			||	(sopInstanceUID == null))
			{
				if (seriesInstanceUID == null)
				{
					Dataset[] ds = CFindSeries(studyInstanceUID, null);
					if ((ds != null)
					&&  (ds.Length == 1))
					{
						seriesInstanceUID = ds[0].GetString(Tags.SeriesInstanceUID);
					}
				}

				if (seriesInstanceUID != null)
				{
					Dataset[] ds = CFindInstance(seriesInstanceUID);
					if ((ds != null)
					&&  (ds.Length == 1))
					{
						sopInstanceUID = ds[0].GetString(Tags.SOPInstanceUID);
					}
				}
			}

			if ((url == null)
			||	(studyInstanceUID == null)
			||	(seriesInstanceUID == null)
			||	(sopInstanceUID == null)
			||	(url.IndexOf('?') >= 0))
				return null;

			Dataset ret = null;
			System.Net.WebResponse response = null;

			try
			{
				StringBuilder sb = new StringBuilder(url);

				sb.Append("?requestType=WADO&studyUID=");
				sb.Append(System.Web.HttpUtility.UrlEncode(studyInstanceUID));
				sb.Append("&seriesUID=");
				sb.Append(System.Web.HttpUtility.UrlEncode(seriesInstanceUID));
				sb.Append("&objectUID=");
				sb.Append(System.Web.HttpUtility.UrlEncode(sopInstanceUID));

				System.Net.WebRequest request = System.Net.WebRequest.Create(sb.ToString());

				response = request.GetResponse();
				
				if (string.Compare(response.ContentType, "application/dicom") == 0)
				{
					DcmParser parser = new DcmParser(response.GetResponseStream());

					FileFormat format = parser.DetectFileFormat();
					if (format != null)
					{
						ret = new Dataset();
						parser.DcmHandler = ret.DcmHandler;
						parser.ParseDcmFile(format, Tags.PixelData);
					}
				}
			}
			finally
			{
				if (response != null)
					response.Close();
			}

			return ret;
		}

		/// <summary>
		/// Send C-STORE
		/// </summary>
		/// <param name="fileName"></param>
		public bool CStore( String fileName )
		{
			int pcid = PCID_START;
			PCID_START += 2;

			Stream ins = null;
			DcmParser parser = null;
			Dataset ds = null;

			try
			{
				//
				// Load DICOM file
				//
				FileInfo file = new FileInfo( fileName );
				try
				{
					ins = new BufferedStream(new FileStream( fileName, FileMode.Open, FileAccess.Read));
					parser = pFact.NewDcmParser(ins);
					FileFormat format = parser.DetectFileFormat();
					if (format != null)
					{
						ds = oFact.NewDataset();
						parser.DcmHandler = ds.DcmHandler;
						parser.ParseDcmFile(format, Tags.PixelData);
						log.Debug( "Reading done" );
					}
					else
					{
						log.Error( "Unknown format!" );
					}
				}
				catch (IOException e)
				{
					log.Error( "Reading failed", e );
				}

				//
				// Prepare association
				//
				String classUID = ds.GetString( Tags.SOPClassUID);
				String tsUID = ds.GetString(Tags.TransferSyntaxUID);

				if( (tsUID == null || tsUID.Equals( "" )) && (ds.GetFileMetaInfo() != null) )
					tsUID = ds.GetFileMetaInfo().GetString(Tags.TransferSyntaxUID);

				if( tsUID == null || tsUID.Equals( "" ) )
					tsUID = UIDs.ImplicitVRLittleEndian;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, 
							classUID, 
							new String[]{ tsUID } ));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					bool bResponse = false;

					FutureRSP frsp = SendDataset(active, parser, ds);
					if (frsp != null)
					{
						active.WaitOnRSP();

						bResponse = true;
					}

					active.Release(true);

					return bResponse;
				}
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);

				if (ins != null)
				{
					try
					{
						ins.Close();
					}
					catch (IOException ignore)
					{
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Send C-STORE
		/// </summary>
		/// <param name="ds"></param>
		public bool CStore( Dataset ds )
		{
			int pcid = PCID_START;
			PCID_START += 2;

			try
			{
				//
				// Prepare association
				//
				String classUID = ds.GetString( Tags.SOPClassUID);
				String tsUID = ds.GetString(Tags.TransferSyntaxUID);

				if( (tsUID == null || tsUID.Equals( "" )) && (ds.GetFileMetaInfo() != null) )
					tsUID = ds.GetFileMetaInfo().GetString(Tags.TransferSyntaxUID);

				if( tsUID == null || tsUID.Equals( "" ) )
					tsUID = UIDs.ImplicitVRLittleEndian;

				m_assocRQ.AddPresContext(aFact.NewPresContext(pcid, 
					classUID, 
					new String[]{ tsUID } ));
				ActiveAssociation active = OpenAssoc();
				if (active != null)
				{
					bool bResponse = false;

					FutureRSP frsp = SendDataset(active, null, ds);
					if (frsp != null)
					{
						active.WaitOnRSP();

						bResponse = true;
					}

					active.Release(true);

					return bResponse;
				}				
			}
			finally
			{
				m_assocRQ.RemovePresContext(pcid);
			}

			return false;
		}
	
		private FutureRSP SendDataset(ActiveAssociation active, DcmParser parser, Dataset ds)
		{
			String sopInstUID = ds.GetString(Tags.SOPInstanceUID);
			if (sopInstUID == null)
			{
				log.Error( "SOP instance UID is null" );
				return null;
			}
			String sopClassUID = ds.GetString(Tags.SOPClassUID);
			if (sopClassUID == null)
			{
				log.Error( "SOP class UID is null" );
				return null;
			}
			PresContext pc = null;
			Association assoc = active.Association;

			if (parser != null)
			{
				if (parser.DcmDecodeParam.encapsulated)
				{
					String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
					if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}
				}
				else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
				{
					log.Error( "SOP class UID not supported" );
					return null;
				}

				return active.Invoke(aFact.NewDimse(pc.pcid(), oFact.NewCommand().InitCStoreRQ(assoc.NextMsgID(), sopClassUID, sopInstUID, 0), new FileDataSource(parser, ds, new byte[2048])));
			}
			else
			{
				if ((ds.GetFileMetaInfo() != null)
				&&	(ds.GetFileMetaInfo().TransferSyntaxUID != null))
				{
					String tsuid = ds.GetFileMetaInfo().TransferSyntaxUID;
					if ((pc = assoc.GetAcceptedPresContext(sopClassUID, tsuid)) == null)
					{
						log.Error( "SOP class UID not supported" );
						return null;
					}
				}
				else if ((pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ImplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRLittleEndian)) == null && (pc = assoc.GetAcceptedPresContext(sopClassUID, UIDs.ExplicitVRBigEndian)) == null)
				{
					log.Error( "SOP class UID not supported" );
					return null;
				}

				return active.Invoke(aFact.NewDimse(pc.pcid(), oFact.NewCommand().InitCStoreRQ(assoc.NextMsgID(), sopClassUID, sopInstUID, 0), ds));
			}

			return null;
		}

		/// <summary>
		/// File Data source
		/// </summary>
		public sealed class FileDataSource : DataSourceI
		{
			private DcmParser parser;
			private Dataset ds;
			private byte[] buffer;

			public FileDataSource(DcmParser parser, Dataset ds, byte[] buffer)
			{
				this.parser = parser;
				this.ds = ds;
				this.buffer = buffer;
			}
			
			public void  WriteTo(Stream outs, String tsUID)
			{
				DcmEncodeParam netParam = (DcmEncodeParam) DcmDecodeParam.ValueOf(tsUID);
				ds.WriteDataset(outs, netParam);
				if (parser.ReadTag == Tags.PixelData)
				{
					DcmDecodeParam fileParam = parser.DcmDecodeParam;
					ds.WriteHeader(outs, netParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
					if (netParam.encapsulated)
					{
						parser.ParseHeader();
						while (parser.ReadTag == Tags.Item)
						{
							ds.WriteHeader(outs, netParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
							copy(parser.InputStream, outs, parser.ReadLength, false, buffer);
						}
						if (parser.ReadTag != Tags.SeqDelimitationItem)
						{
							throw new DcmParseException("Unexpected Tag:" + Tags.ToHexString(parser.ReadTag));
						}
						if (parser.ReadLength != 0)
						{
							throw new DcmParseException("(fffe,e0dd), Length:" + parser.ReadLength);
						}
						ds.WriteHeader(outs, netParam, Tags.SeqDelimitationItem, VRs.NONE, 0);
					}
					else
					{
						bool swap = fileParam.byteOrder != netParam.byteOrder && parser.ReadVR == VRs.OW;
						copy(parser.InputStream, outs, parser.ReadLength, swap, buffer);
					}
					ds.Clear();
					parser.ParseDataset(fileParam, 0);
					ds.WriteDataset(outs, netParam);
				}
			}
		}
	
		private static void copy(Stream ins, Stream outs, int len, bool swap, byte[] buffer)
		{
			if (swap && (len & 1) != 0)
			{
				throw new DcmParseException("Illegal length of OW Pixel Data: " + len);
			}
			if (buffer == null)
			{
				if (swap)
				{
					int tmp;
					for (int i = 0; i < len; ++i, ++i)
					{
						tmp = ins.ReadByte();
						outs.WriteByte((System.Byte) ins.ReadByte());
						outs.WriteByte((System.Byte) tmp);
					}
				}
				else
				{
					for (int i = 0; i < len; ++i)
					{
						outs.WriteByte((System.Byte) ins.ReadByte());
					}
				}
			}
			else
			{
				byte tmp;
				int c, remain = len;
				while (remain > 0)
				{
					c = ins.Read( buffer, 0, System.Math.Min(buffer.Length, remain));
					if (swap)
					{
						if ((c & 1) != 0)
						{
							buffer[c++] = (byte) ins.ReadByte();
						}
						for (int i = 0; i < c; ++i, ++i)
						{
							tmp = buffer[i];
							buffer[i] = buffer[i + 1];
							buffer[i + 1] = tmp;
						}
					}
					outs.Write(buffer, 0, c);
					remain -= c;
				}
			}
		}
	}
}
