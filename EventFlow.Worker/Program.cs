using EventFlow.Infrastructure.Extensions;
using EventFlow.Worker;
using EventFlow.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddHostedService<EventConsumer>();
builder.Services.AddHostedService<NotificationConsumer>();

var host = builder.Build();
host.Run();
