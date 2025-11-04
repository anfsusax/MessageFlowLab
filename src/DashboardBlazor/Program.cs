using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using DashboardBlazor.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Dashboard services
builder.Services.AddHttpClient<DashboardBlazor.Services.RabbitMqService>();
builder.Services.AddSingleton<DashboardBlazor.Services.KafkaService>();
builder.Services.AddScoped<DashboardBlazor.Services.MessageLogService>();

// DbContext for reading MessageLogs
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "server=localhost;port=3306;database=messageflow;user=user;password=password";
builder.Services.AddDbContext<DashboardBlazor.Data.AppDbContext>(opt => opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
