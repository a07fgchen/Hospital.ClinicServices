using Hospital.ClinicServices.WebApi;
using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.Hubs;
using Hospital.ClinicServices.WebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 注入連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicDbContext>(options => options.UseSqlServer(connectionString));

// Add services to the container.
// Scoped 意思為每次請求都會建立一個新的實例，並在請求結束釋放該實例。
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

//加入 SignalR 服務
builder.Services.AddSignalR();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<QueueHub>("/hub/queue");

app.Run();
