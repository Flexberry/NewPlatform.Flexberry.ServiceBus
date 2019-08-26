namespace NewPlatform.Flexberry.ServiceBus.Clustering
{
    public interface IDlmGetLockValueResult
    {
        string Value { get; set; }

        DlmGetLockValueResultState State { get; set; }
    }
}
