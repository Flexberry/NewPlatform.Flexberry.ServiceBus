namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public class SimpleDlmWatchResult : IDlmWatchResult
    {
        public string NewValue { get; set; }

        public DlmWatchNodeState State { get; set; } = DlmWatchNodeState.NotChanged;

        public SimpleDlmWatchResult() { }

        public SimpleDlmWatchResult(DlmWatchNodeState watchedNodeState)
        {
            State = watchedNodeState;
        }

        public SimpleDlmWatchResult(string watchedNodeValue, DlmWatchNodeState watchedNodeState)
        {
            NewValue = watchedNodeValue;
            State = watchedNodeState;
        }
    }
}
