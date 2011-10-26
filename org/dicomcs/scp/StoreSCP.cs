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

namespace org.dicomcs.scp
{
	using System;
	using System.IO;
	using System.Reflection;
	using org.dicomcs.data;
	using org.dicomcs.dict;
	using org.dicomcs.net;
	
	/// <summary>
	/// SCP for C-STORE
	/// </summary>
	public class StoreSCP : DcmServiceBase
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		new private const int SUCCESS = 0x0000;
		private const int PROCESSING_FAILURE = 0x0101;
		private const int CLASS_INSTANCE_CONFLICT = 0x0119;
		private const int MISSING_UID = 0xA900;
		private const int MISMATCH_UID = 0xA901;
		private const int CANNOT_UNDERSTAND = 0xC000;
		
		protected DcmParserFactory parserFact = DcmParserFactory.Instance;
		private FileInfo archiveDir = new FileInfo("_archive");
		private int dirSplitLevel = 1;
		
		public StoreSCP()
		{
		}
		
		public virtual FileInfo ArchiveDir
		{
			get { return archiveDir; }			
			set
			{
				bool tmpBool;
				if (File.Exists(value.FullName))
					tmpBool = true;
				else
					tmpBool = Directory.Exists(value.FullName);
				if (!tmpBool)
				{
					Directory.CreateDirectory(value.FullName);
				}
				if (!Directory.Exists(value.FullName))
				{
					throw new System.ArgumentException("cannot access directory " + value);
				}
				this.archiveDir = value;
			}
			
		}
	
		protected override void DoCStore(ActiveAssociation assoc, Dimse rq, Command rspCmd)
		{
			Command rqCmd = rq.Command;
			Stream ins = rq.DataAsStream;
			try
			{
				String instUID = rqCmd.AffectedSOPInstanceUID;
				String classUID = rqCmd.AffectedSOPClassUID;
				DcmDecodeParam decParam = DcmDecodeParam.ValueOf(rq.TransferSyntaxUID);
				Dataset ds = objFact.NewDataset();
				DcmParser parser = parserFact.NewDcmParser(ins);
				parser.DcmHandler = ds.DcmHandler;
				parser.ParseDataset(decParam, Tags.PixelData);
				ds.SetFileMetaInfo( objFact.NewFileMetaInfo(classUID, instUID, rq.TransferSyntaxUID) );
				FileInfo file = toFile(ds);
				storeToFile(parser, ds, file, (DcmEncodeParam) decParam);
				rspCmd.PutUS(Tags.Status, SUCCESS);
			}
			catch (System.Exception e)
			{
				log.Error(e.Message, e);
				throw new DcmServiceException(PROCESSING_FAILURE, e);
			}
			finally
			{
				ins.Close();
			}
		}
		
		private Stream openOutputStream(FileInfo file)
		{
			DirectoryInfo parent = file.Directory;
			bool tmpBool;
			if (File.Exists(parent.FullName))
				tmpBool = true;
			else
				tmpBool = Directory.Exists(parent.FullName);

			if (!tmpBool)
			{
				Directory.CreateDirectory(parent.FullName);
				log.Info("M-WRITE " + parent);
			}

			log.Info("M-WRITE " + file);
			return new BufferedStream(new FileStream(file.FullName, FileMode.Create));
		}
		
		private void  storeToFile(DcmParser parser, Dataset ds, FileInfo file, DcmEncodeParam encParam)
		{
			Stream outs = openOutputStream(file);
			try
			{
				ds.WriteFile(outs, encParam);
				if (parser.ReadTag == Tags.PixelData)
				{
					ds.WriteHeader(outs, encParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
					copy(parser.InputStream, outs);
				}
			}
			finally
			{
				try
				{
					outs.Close();
				}
				catch (IOException ignore)
				{
				}
			}
		}
		
		private void  copy(Stream ins, Stream outs)
		{
			int c;
			byte[] buffer = new byte[512];
			while ((c = ins.Read( buffer, 0, buffer.Length)) != - 1)
			{
				outs.Write(buffer, 0, c);
			}
		}
		
		private FileInfo toFile(Dataset ds)
		{
			String studyInstUID = null;
			try
			{
				studyInstUID = ds.GetString(Tags.StudyInstanceUID);
				if (studyInstUID == null)
				{
					throw new DcmServiceException(MISSING_UID, "Missing Study Instance UID");
				}
				if (ds.vm(Tags.SeriesInstanceUID) <= 0)
				{
					throw new DcmServiceException(MISSING_UID, "Missing Series Instance UID");
				}
				String instUID = ds.GetString(Tags.SOPInstanceUID);
				if (instUID == null)
				{
					throw new DcmServiceException(MISSING_UID, "Missing SOP Instance UID");
				}
				String classUID = ds.GetString(Tags.SOPClassUID);
				if (classUID == null)
				{
					throw new DcmServiceException(MISSING_UID, "Missing SOP Class UID");
				}
				if (!instUID.Equals(ds.GetFileMetaInfo().MediaStorageSOPInstanceUID))
				{
					throw new DcmServiceException(MISMATCH_UID, "SOP Instance UID in Dataset differs from Affected SOP Instance UID");
				}
				if (!classUID.Equals(ds.GetFileMetaInfo().MediaStorageSOPClassUID))
				{
					throw new DcmServiceException(MISMATCH_UID, "SOP Class UID in Dataset differs from Affected SOP Class UID");
				}
			}
			catch (DcmValueException e)
			{
				throw new DcmServiceException(CANNOT_UNDERSTAND, e);
			}
			
			String pn = ToFileID(ds, Tags.PatientName) + "____";
			FileInfo dir = archiveDir;
			for (int i = 0; i < dirSplitLevel; ++i)
			{
				dir = new FileInfo(dir.FullName + "\\" + pn.Substring(0, (i + 1) - (0)));
			}
			dir = new FileInfo(dir.FullName + "\\" + studyInstUID);
			dir = new FileInfo(dir.FullName + "\\" + ToFileID(ds, Tags.SeriesNumber));
			FileInfo file = new FileInfo(dir.FullName + "\\" + ToFileID(ds, Tags.InstanceNumber) + ".dcm");						
			return file;
		}
		
		private String ToFileID(Dataset ds, uint tag)
		{
			try
			{
				String s = ds.GetString(tag);
				if (s == null || s.Length == 0)
					return "__NULL__";
				char[] ins = s.ToUpper().ToCharArray();
				char[] outs = new char[System.Math.Min(8, ins.Length)];
				for (int i = 0; i < outs.Length; ++i)
				{
					outs[i] = ins[i] >= '0' && ins[i] <= '9' || ins[i] >= 'A' && ins[i] <= 'Z'?ins[i]:'_';
				}
				return new String(outs);
			}
			catch (DcmValueException e)
			{
				return "__ERR__";
			}
		}
	}
}