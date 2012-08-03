namespace DicomSharp.Net {
    public interface IActiveAssociation {
        /// <summary>
        /// Start a new pooled thread for handling this active association
        /// </summary>
        void Start();

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dimseListener"></param>
        void Invoke(IDimse request, IDimseListener dimseListener);

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        FutureDimseResponse Invoke(IDimse request);

        /// <summary>
        /// Send association release request and release this association
        /// </summary>
        /// <param name="waitOnResponse"></param>
        void Release(bool waitOnResponse);

        /// <summary>
        /// Wait on all responses)
        /// </summary>
        void WaitOnResponse();

        int Timeout { get; set; }
        IAssociation Association { get; }
    }
}