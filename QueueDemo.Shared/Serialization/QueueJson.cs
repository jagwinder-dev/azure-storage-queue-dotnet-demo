using System.Text.Json;

namespace QueueDemo.Shared.Serialization;

public static class QueueJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };
}
