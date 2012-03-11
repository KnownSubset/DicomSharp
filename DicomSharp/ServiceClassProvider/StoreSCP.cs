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
using System.Reflection;
using DicomSharp.Data;
using DicomSharp.Dictionary;
using DicomSharp.Net;
using log4net;

namespace DicomSharp.ServiceClassProvider {
    /// <summary>
    /// SCP for C-STORE
    /// </summary>
    public class StoreSCP : DcmServiceBase {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(StoreSCP));
        private new const int SUCCESS = 0x0000;
        private const int PROCESSING_FAILURE = 0x0101;
        private const int MISSING_UID = 0xA900;
        private const int MISMATCH_UID = 0xA901;
        private const int CANNOT_UNDERSTAND = 0xC000;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private FileInfo archiveDir = new FileInfo("_archive");
        private int dirSplitLevel = 1;
        protected DcmParserFactory parserFact = DcmParserFactory.Instance;

        public virtual FileInfo ArchiveDir {
            get { return archiveDir; }
            set {
                bool tmpBool;
                if (File.Exists(value.FullName)) {
                    tmpBool = true;
                }
                else {
                    tmpBool = Directory.Exists(value.FullName);
                }
                if (!tmpBool) {
                    Directory.CreateDirectory(value.FullName);
                }
                if (!Directory.Exists(value.FullName)) {
                    throw new ArgumentException("cannot access directory " + value);
                }
                archiveDir = value;
            }
        }

        protected override void DoCStore(ActiveAssociation activeAssociation, IDimse request, IDicomCommand responseCommand) {
            IDicomCommand rqCmd = request.DicomCommand;
            Stream ins = request.DataAsStream;
            try {
                String instanceUniqueId = rqCmd.AffectedSOPInstanceUniqueId;
                String classUniqueId = rqCmd.AffectedSOPClassUniqueId;
                DcmDecodeParam decParam = DcmDecodeParam.ValueOf(request.TransferSyntaxUniqueId);
                DataSet dataSet = objFact.NewDataset();
                DcmParser parser = parserFact.NewDcmParser(ins);
                parser.DcmHandler = dataSet.DcmHandler;
                parser.ParseDataset(decParam, Tags.PixelData);
                dataSet.FileMetaInfo = objFact.NewFileMetaInfo(classUniqueId, instanceUniqueId, request.TransferSyntaxUniqueId);
                FileInfo file = ToFile(dataSet);
                StoreToFile(parser, dataSet, file, (DcmEncodeParam) decParam);
                responseCommand.PutUS(Tags.Status, SUCCESS);
            }
            catch (Exception e) {
                log.Error(e.Message, e);
                throw new DcmServiceException(PROCESSING_FAILURE, e);
            }
            finally {
                ins.Close();
            }
        }

        private Stream openOutputStream(FileInfo file) {
            DirectoryInfo parent = file.Directory;
            bool tmpBool = File.Exists(parent.FullName) || Directory.Exists(parent.FullName);
            if (!tmpBool) {
                Directory.CreateDirectory(parent.FullName);
                log.Info("M-WRITE " + parent);
            }

            log.Info("M-WRITE " + file);
            return new BufferedStream(new FileStream(file.FullName, FileMode.Create));
        }

        private void StoreToFile(DcmParser parser, DataSet ds, FileInfo file, DcmEncodeParam encParam) {
            Stream outs = openOutputStream(file);
            try {
                ds.WriteFile(outs, encParam);
                if (parser.ReadTag == Tags.PixelData) {
                    ds.WriteHeader(outs, encParam, parser.ReadTag, parser.ReadVR, parser.ReadLength);
                    Copy(parser.InputStream, outs);
                }
            }
            finally {
                try {
                    outs.Close();
                }
                catch (IOException ignore)
                {
                    Logger.Error(ignore);
                    
                }
            }
        }

        private void Copy(Stream ins, Stream outs) {
            int c;
            var buffer = new byte[512];
            while ((c = ins.Read(buffer, 0, buffer.Length)) != - 1) {
                outs.Write(buffer, 0, c);
            }
        }

        private FileInfo ToFile(DataSet ds) {
            String studyInstanceUniqueId = null;
            try {
                studyInstanceUniqueId = ds.GetString(Tags.StudyInstanceUniqueId);
                if (studyInstanceUniqueId == null) {
                    throw new DcmServiceException(MISSING_UID, "Missing Study Instance UID");
                }
                if (ds.Vm(Tags.SeriesInstanceUniqueId) <= 0) {
                    throw new DcmServiceException(MISSING_UID, "Missing Series Instance UID");
                }
                String instanceUniqueId = ds.GetString(Tags.SOPInstanceUniqueId);
                if (instanceUniqueId == null) {
                    throw new DcmServiceException(MISSING_UID, "Missing SOP Instance UID");
                }
                String classUniqueId = ds.GetString(Tags.SOPClassUniqueId);
                if (classUniqueId == null) {
                    throw new DcmServiceException(MISSING_UID, "Missing SOP Class UID");
                }
                if (!instanceUniqueId.Equals(ds.FileMetaInfo.MediaStorageSOPInstanceUniqueId)) {
                    throw new DcmServiceException(MISMATCH_UID,
                                                  "SOP Instance UID in DataSet differs from Affected SOP Instance UID");
                }
                if (!classUniqueId.Equals(ds.FileMetaInfo.MediaStorageSOPClassUniqueId)) {
                    throw new DcmServiceException(MISMATCH_UID,
                                                  "SOP Class UID in DataSet differs from Affected SOP Class UID");
                }
            }
            catch (DcmValueException e) {
                throw new DcmServiceException(CANNOT_UNDERSTAND, e);
            }

            String pn = ToFileID(ds, Tags.PatientName) + "____";
            FileInfo dir = archiveDir;
            for (int i = 0; i < dirSplitLevel; ++i) {
                dir = new FileInfo(dir.FullName + "\\" + pn.Substring(0, (i + 1) - (0)));
            }
            dir = new FileInfo(dir.FullName + "\\" + studyInstanceUniqueId);
            dir = new FileInfo(dir.FullName + "\\" + ToFileID(ds, Tags.SeriesNumber));
            var file = new FileInfo(dir.FullName + "\\" + ToFileID(ds, Tags.InstanceNumber) + ".dcm");
            return file;
        }

        private String ToFileID(DataSet ds, uint tag) {
            try {
                String s = ds.GetString(tag);
                if (string.IsNullOrEmpty(s)) {
                    return "__NULL__";
                }
                char[] ins = s.ToUpper().ToCharArray();
                var outs = new char[Math.Min(8, ins.Length)];
                for (int i = 0; i < outs.Length; ++i) {
                    outs[i] = ins[i] >= '0' && ins[i] <= '9' || ins[i] >= 'A' && ins[i] <= 'Z' ? ins[i] : '_';
                }
                return new String(outs);
            }
            catch (DcmValueException dcmValueException) {
                Logger.Error(dcmValueException);
                return "__ERR__";
            }
        }
    }
}