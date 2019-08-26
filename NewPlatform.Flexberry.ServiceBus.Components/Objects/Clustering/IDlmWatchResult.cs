namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public interface IDlmWatchResult
    {
        string NewValue { get; set; }

        DlmWatchNodeState State { get; set; }
    }
}
