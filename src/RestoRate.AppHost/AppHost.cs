using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Define certificate parameters

var consumer = builder.AddProject<RestoRate_MessageConsumer>("message-consumer");

var sender = builder.AddProject<RestoRate_MessageSender>("message-sender");

await builder.Build().RunAsync();
