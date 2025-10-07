using System.Text;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddRabbitMQClient(connectionName: "messaging");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/hello", async Task<string> (IConnection connection, CancellationToken cancellationToken) =>
{
    using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

    var queueName = "hello-queue";
    await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

    var body = Encoding.UTF8.GetBytes("hello");
    await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body, cancellationToken: cancellationToken);

    // Try to get a message from the queue (simulate "receive results here")
    var result = await channel.BasicGetAsync(queue: queueName, autoAck: true, cancellationToken: cancellationToken);
    var received = result != null ? Encoding.UTF8.GetString(result.Body.ToArray()) : "No message";
    return received;
})
.WithName("GetHello");

await app.RunAsync();