using Hospital.ClinicServices.WebApi.Services;
using Hospital.ClinicServices.WebApi.Services.Interfaces;

namespace Hospital.ClinicServices.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Scoped 意思為每次請求都會建立一個新的實例，並在請求結束釋放該實例。
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ICallingService, CallingService>();

        return services;
    }
}
