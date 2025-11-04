using MessageGenerator;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog for structured console logging
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

// Integrate Serilog with the generic host logging pipeline
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
