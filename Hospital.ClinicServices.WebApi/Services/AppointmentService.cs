using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi;

public class AppointmentService : IAppointmentService
{
    private const int RegisteredStatus = 0;
    private readonly ClinicDbContext _context;

    public AppointmentService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment> RegisterFirstVisitAsync(FirstVisitRegisterRequestDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var normalizedNationalId = request.NationalId.Trim().ToUpperInvariant();
            var patientExists = await _context.Patients
                .AnyAsync(patient => patient.NationalId == normalizedNationalId);

            if (patientExists)
            {
                throw new InvalidOperationException("該病人已填寫過初診資料，請勿重複填寫。");
            }

            var schedule = await GetScheduleForUpdateAsync(request.ScheduleId);

            EnsureScheduleIsAvailable(schedule);

            var newPatient = new Patient
            {
                NationalId = normalizedNationalId,
                Name = request.PatientName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                BirthDate = request.BirthDate,
                IsFirstVisited = true,
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            var appointment = await CreateAppointmentAsync(schedule, newPatient.PatientId);
            await transaction.CommitAsync();

            return appointment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<Appointment> RegisterAppointmentAsync(RegisterRequestDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var normalizedNationalId = request.NationalId.Trim().ToUpperInvariant();
            var patient = await _context.Patients
                .SingleOrDefaultAsync(patient => patient.NationalId == normalizedNationalId)
                ?? throw new InvalidOperationException("找不到病患資料，請使用初診掛號。");

            var schedule = await GetScheduleForUpdateAsync(request.ScheduleId);

            EnsureScheduleIsAvailable(schedule);

            var appointment = await CreateAppointmentAsync(schedule, patient.PatientId);
            await transaction.CommitAsync();

            return appointment;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Appointment> CreateAppointmentAsync(Schedule schedule, int patientId)
    {
        var isAlreadyRegistered = await _context.Appointments.AnyAsync(appointment =>
            appointment.ScheduleId == schedule.ScheduleId &&
            appointment.PatientId == patientId &&
            appointment.AppointmentStatus == RegisteredStatus);

        if (isAlreadyRegistered)
        {
            throw new InvalidOperationException("您已預約過此門診，請勿重複掛號。");
        }

        var sequenceNumber = schedule.CurrentRegisterCount + 1;
        schedule.CurrentRegisterCount = sequenceNumber;

        var appointment = new Appointment
        {
            ScheduleId = schedule.ScheduleId,
            PatientId = patientId,
            SequenceNumber = sequenceNumber,
            AppointmentStatus = RegisteredStatus,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return appointment;
    }

    private static void EnsureScheduleIsAvailable(Schedule schedule)
    {
        if (schedule.Status == 2 || schedule.Status == 3)
        {
            throw new InvalidOperationException("該門診已休診或已結束，無法掛號。");
        }

        if (schedule.CurrentRegisterCount >= schedule.MaxQuota)
        {
            throw new InvalidOperationException("非常抱歉，該門診預約名額已滿。");
        }
    }

    private async Task<Schedule> GetScheduleForUpdateAsync(int scheduleId)
    {
        return await _context.Schedules
            //相當於Mysql的SELECT ... FOR UPDATE，鎖定該筆排班資料，避免同時有多個使用者搶同一個排班
            .FromSqlRaw("SELECT * FROM Schedules WITH (UPDLOCK, ROWLOCK) WHERE ScheduleId = {0}", scheduleId)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException("找不到該門診排班資訊。");
    }
}
