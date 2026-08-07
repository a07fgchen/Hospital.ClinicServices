using Hospital.ClinicServices.WebApi;
using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.ExceptionHandlers;
using Hospital.ClinicServices.WebApi.Hubs;
using Hospital.ClinicServices.WebApi.Services;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 注入連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Add services to the container.
// Scoped 意思為每次請求都會建立一個新的實例，並在請求結束釋放該實例。
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ICallingService, CallingService>();
//加入 SignalR 服務
builder.Services.AddSignalR();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    await ClinicDataSeeder.SeedAllAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.MapHub<QueueHub>("/hub/queue");

app.Run();
