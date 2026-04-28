using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QueueDemo.Shared.Configuration;

namespace QueueConsumer.Worker.Idempotency;

public sealed class JsonFileProcessedMessageStore : IProcessedMessageStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly ILogger<JsonFileProcessedMessageStore> _logger;
    private readonly ConcurrentDictionary<string, byte> _processedMessageIds = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonFileProcessedMessageStore(
        IOptions<QueueSettings> options,
        ILogger<JsonFileProcessedMessageStore> logger)
    {
        _logger = logger;

        var configuredPath = options.Value.ProcessedMessagesFilePath;
        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath);

        LoadFromDisk();
    }

    public Task<bool> IsProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processedMessageIds.ContainsKey(messageId));
    }

    public async Task MarkProcessedAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        if (!_processedMessageIds.TryAdd(messageId, 0))
        {
            return;
        }

        await PersistAsync(cancellationToken);
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogInformation(
                "No idempotency store found. A new file will be created at {FilePath}",
                _filePath);
            return;
        }

        var json = File.ReadAllText(_filePath);
        var messageIds = JsonSerializer.Deserialize<HashSet<string>>(json, SerializerOptions) ?? [];

        foreach (var messageId in messageIds)
        {
            _processedMessageIds.TryAdd(messageId, 0);
        }

        _logger.LogInformation(
            "Loaded {ProcessedMessageCount} processed message ids from {FilePath}",
            _processedMessageIds.Count,
            _filePath);
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var messageIds = _processedMessageIds.Keys.OrderBy(messageId => messageId).ToArray();
            var json = JsonSerializer.Serialize(messageIds, SerializerOptions);

            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
