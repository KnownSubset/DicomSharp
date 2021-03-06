using System.Collections.Generic;
using DicomSharp.Data;

namespace DicomSharp.Net {
    /// <summary>
    /// This interface defines the basic operations that a service class user needs to operate against a service class provider
    /// </summary>
    public interface IServiceClassUser {
        /// <summary>
        /// Send C-ECHO, <see cref="SetUpForOperation"/> to specify the endpoint for the echo
        /// </summary>
        bool CEcho();

        /// <summary>
        /// Find all the studies for a specific patient
        /// <param name="patientId">The id of the patient</param>
        /// <param name="patientName">The name of the patient</param>
        /// </summary>
        IList<DataSet> CFindStudy(string patientId, string patientName);

        /// <summary>
        /// Find a the study for the study Instance UIDs
        /// <param name="studyInstanceUID">The studu instance UIDs</param>
        /// </summary>
        IList<DataSet> CFindStudy(string studyInstanceUID);

        /// <summary>
        /// Find all the studies for the studies Instance UIDs
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// </summary>
        IList<DataSet> CFindStudies(IEnumerable<string> studyInstanceUIDs);

        /// <summary>
        /// Send C-FIND for series
        /// <param name="studyInstanceUniqueId">A series instance UID</param>
        /// </summary>
        IList<DataSet> CFindSeriesForStudy(string studyInstanceUniqueId);

        /// <summary>
        /// Send C-FIND for series
        /// <param name="studiesInstanceUniqueIds">The series' instance UIDs</param>
        /// </summary>
        IList<DataSet> CFindSeriesForStudies(IEnumerable<string> studiesInstanceUniqueIds);
        
        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUniqueId">A series instance UID</param>
        /// </summary>
        IList<DataSet> CFindSeries(string seriesInstanceUniqueId);

        /// <summary>
        /// Send C-FIND for series
        /// <param name="seriesInstanceUniqueIds">The series' instance UIDs</param>
        /// </summary>
        IList<DataSet> CFindSeries(IEnumerable<string> seriesInstanceUniqueIds);

        /// <summary>
        /// Send C-FIND for instance
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// <param name="seriesInstanceUIDs">The series' instance UIDs</param>
        /// </summary>
        IList<DataSet> CFindInstance(IEnumerable<string> studyInstanceUIDs, IEnumerable<string> seriesInstanceUIDs);

        /// <summary>
        /// Send C-Move for studies and series to be stored in the specified applicationEntityDestination
        /// <param name="studyInstanceUIDs">The studies' instance UIDs</param>
        /// <param name="seriesInstanceUIDs">The series' instance UIDs</param>
        /// <param name="applicationEntityDestination">The SCP that will store the files</param>
        /// </summary>
        IList<DataSet> CMove(IEnumerable<string> studyInstanceUIDs, IEnumerable<string> seriesInstanceUIDs,
                             string applicationEntityDestination);

        /// <summary>
        /// Send C-GET
        /// <param name="studyInstanceUID">A study instance UID</param>
        /// <param name="seriesInstanceUID">A series instance UID</param>
        /// <param name="sopInstanceUID">A sop instance instance UID</param>
        /// </summary>
        IList<DataSet> CGet(string studyInstanceUID, string seriesInstanceUID, string sopInstanceUID);

        /*/// <summary>
        /// Send C-STORE
        /// </summary>
        /// <param name="dataSet"></param>
        bool CStore(DicomDataSet dataSet);*/

        /// <summary>
        /// This method configures the Service Class User to operate against a specific Service Class Provider
        /// <param name="name">The name of the SCU that will be sent to the SCP</param>
        /// <param name="title">The AE Title of the SCP</param>
        /// <param name="newHostName">The hostname of the SCP</param>
        /// <param name="newPort">The newPort of the SCP</param>
        /// </summary>
        void SetUpForOperation(string name, string title, string newHostName, int newPort);

        /// <summary>
        /// Sends a cancel message to SCP for the current operation
        /// </summary>
        bool Cancel();

        string Name { get; set; }
        string HostName { get; set; }
        string Title { get; set; }
        uint Port { get; set; }
    }
}