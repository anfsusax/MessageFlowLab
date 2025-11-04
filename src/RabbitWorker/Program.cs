using RabbitWorker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
