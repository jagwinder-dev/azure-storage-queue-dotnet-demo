using QueueDemo.Shared.Configuration;
using QueueDemo.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<QueueSettings>(
    builder.Configuration.GetSection(QueueSettings.SectionName));
builder.Services.AddSingleton<IQueueService, QueueService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
    await queueService.CreateQueueIfNotExistsAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
