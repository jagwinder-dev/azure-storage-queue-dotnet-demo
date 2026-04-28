using QueueDemo.Shared.Constants;

namespace QueueDemo.Shared.Configuration;

public sealed class QueueSettings
{
    public const string SectionName = "QueueSettings";

    public string StorageConnectionString { get; set; } = "UseDevelopmentStorage=true";

    public string QueueName { get; set; } = QueueConstants.OrderProcessingQueueName;

    public int VisibilityTimeoutSeconds { get; set; } = 30;

    public int PollingIntervalSeconds { get; set; } = 5;

    public int ProcessingDelaySeconds { get; set; } = 3;

    public bool SimulateFailure { get; set; }

    public string ProcessedMessagesFilePath { get; set; } = "processed-messages.json";
}
