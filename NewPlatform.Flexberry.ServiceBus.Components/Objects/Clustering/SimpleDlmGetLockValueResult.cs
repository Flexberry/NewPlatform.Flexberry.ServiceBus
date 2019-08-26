namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public class SimpleDlmGetLockValueResult : IDlmGetLockValueResult
    {
        public string Value { get; set; }
        public DlmGetLockValueResultState State { get; set; }

        public SimpleDlmGetLockValueResult() { }

        /// <summary>
        /// Success SimpleDlmGetLockValueResult constructor.
        /// </summary>
        /// <param name="value">Result value.</param>
        public SimpleDlmGetLockValueResult(string value)
        {
            Value = value;
            State = DlmGetLockValueResultState.Success;
        }

        /// <summary>
        /// Not success result constructor.
        /// </summary>
        /// <param name="state"></param>
        public SimpleDlmGetLockValueResult(DlmGetLockValueResultState state)
        {
            State = state;
        }
    }
}
