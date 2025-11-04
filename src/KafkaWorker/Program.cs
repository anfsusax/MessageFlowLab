using KafkaWorker;
using Serilog;
using Microsoft.EntityFrameworkCore;
using KafkaWorker.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger);

// Configure DbContext for MySQL (default connection string used if not provided)
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "server=localhost;port=3306;database=messageflow;user=user;password=password";

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
