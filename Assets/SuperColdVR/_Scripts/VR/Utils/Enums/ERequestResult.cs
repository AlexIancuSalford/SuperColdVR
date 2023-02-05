namespace SuperColdVR.VR.Utils.Enums
{
    public enum ERequestResult
    {
        /// <summary>
        /// The locomotion request was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The locomotion request failed due to the system being currently busy.
        /// </summary>
        Busy,

        /// <summary>
        /// The locomotion request failed due to an unknown error.
        /// </summary>
        Error,
    }
}
