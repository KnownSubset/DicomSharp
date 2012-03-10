namespace DicomSharp.Net {
    public interface IActiveAssociation {
        /// <summary>
        /// Start a new pooled thread for handling this active association
        /// </summary>
        void Start();

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="rq"></param>
        /// <param name="dimseListener"></param>
        void Invoke(IDimse rq, IDimseListener dimseListener);

        /// <summary>
        /// Send a DIMSE message
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        FutureDimseResponse Invoke(IDimse rq);

        /// <summary>
        /// Send association release request and release this association
        /// </summary>
        /// <param name="waitOnRSP"></param>
        void Release(bool waitOnRSP);

        /// <summary>
        /// Wait on all responses)
        /// </summary>
        void WaitOnRSP();

        int Timeout { get; set; }
        IAssociation Association { get; }
    }
}