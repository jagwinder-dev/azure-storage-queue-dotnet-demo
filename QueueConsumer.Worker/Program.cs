using QueueConsumer.Worker;
using QueueConsumer.Worker.Idempotency;
using QueueDemo.Shared.Configuration;
using QueueDemo.Shared.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<QueueSettings>(
    builder.Configuration.GetSection(QueueSettings.SectionName));
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddSingleton<IProcessedMessageStore, JsonFileProcessedMessageStore>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
