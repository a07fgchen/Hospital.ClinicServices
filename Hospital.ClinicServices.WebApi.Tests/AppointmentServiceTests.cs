using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Tests;

public sealed class AppointmentServiceTests
{
    [Fact]
    public async Task RegisterAppointment_NormalizesIdAndCreatesNextSequence()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, registerCount: 2);
        var patient = await AddPatientAsync(database, "A123456789");

        var result = await new AppointmentService(database.Context)
            .RegisterAppointmentAsync("a123456789 ", schedule.ScheduleId, patient.BirthDate);

        Assert.Equal(3, result.SequenceNumber);
        Assert.Equal(patient.PatientId, result.PatientId);
        Assert.Equal(3, schedule.CurrentRegisterCount);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(1, await database.Context.Appointments.CountAsync());
    }

    [Fact]
    public async Task RegisterFirstVisit_NormalizesInputAndCreatesPatientAndAppointment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database);
        var request = new FirstVisitRegisterRequestDto
        {
            ScheduleId = schedule.ScheduleId,
            NationalId = " a223456789 ",
            PatientName = " 王小明 ",
            PhoneNumber = " 0912345678 ",
            BirthDate = new DateTime(2000, 1, 2)
        };

        var result = await new AppointmentService(database.Context).RegisterFirstVisitAsync(request);

        var patient = await database.Context.Patients.SingleAsync();
        Assert.Equal("A223456789", patient.NationalId);
        Assert.Equal("王小明", patient.Name);
        Assert.Equal("0912345678", patient.PhoneNumber);
        Assert.True(patient.IsFirstVisited);
        Assert.Equal(patient.PatientId, result.PatientId);
        Assert.Equal(1, result.SequenceNumber);
    }

    [Fact]
    public async Task RegisterFirstVisit_ExistingPatientRejectsWithoutAddingData()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database);
        await AddPatientAsync(database, "A123456789");
        var request = FirstVisitRequest(schedule.ScheduleId, " a123456789 ");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new AppointmentService(database.Context).RegisterFirstVisitAsync(request));

        Assert.Contains("請勿重複填寫", exception.Message);
        Assert.Equal(1, await database.Context.Patients.CountAsync());
        Assert.Empty(database.Context.Appointments);
    }

    [Fact]
    public async Task RegisterFirstVisit_UnavailableScheduleRollsBackNewPatient()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, status: 2);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterFirstVisitAsync(
                FirstVisitRequest(schedule.ScheduleId, "A223456789")));

        Assert.Empty(database.Context.Patients);
        Assert.Empty(database.Context.Appointments);
    }

    [Fact]
    public async Task RegisterAppointment_InvalidPatientRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterAppointmentAsync(
                "A123456789", 1, new DateTime(1990, 1, 1)));
        Assert.Contains("身分證字號或出生日期不正確", exception.Message);
    }

    [Fact]
    public async Task RegisterAppointment_MissingScheduleRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var patient = await AddPatientAsync(database, "A123456789");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterAppointmentAsync(
                patient.NationalId, 999, patient.BirthDate));
        Assert.Contains("找不到該門診", exception.Message);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task RegisterAppointment_ClosedOrFinishedScheduleRejects(int status)
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, status: status);
        var patient = await AddPatientAsync(database, "A123456789");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterAppointmentAsync(
                patient.NationalId, schedule.ScheduleId, patient.BirthDate));
        Assert.Contains("已休診或已結束", exception.Message);
    }

    [Fact]
    public async Task RegisterAppointment_FullScheduleRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, maxQuota: 2, registerCount: 2);
        var patient = await AddPatientAsync(database, "A123456789");
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterAppointmentAsync(
                patient.NationalId, schedule.ScheduleId, patient.BirthDate));
        Assert.Contains("名額已滿", exception.Message);
    }

    [Fact]
    public async Task RegisterAppointment_DuplicateActiveAppointmentRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, registerCount: 1);
        var patient = await AddPatientAsync(database, "A123456789");
        database.Context.Appointments.Add(new Appointment
        {
            ScheduleId = schedule.ScheduleId, PatientId = patient.PatientId,
            SequenceNumber = 1, AppointmentStatus = 0
        });
        await database.Context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new AppointmentService(database.Context).RegisterAppointmentAsync(
                patient.NationalId, schedule.ScheduleId, patient.BirthDate));

        Assert.Contains("請勿重複掛號", exception.Message);
        Assert.Equal(1, schedule.CurrentRegisterCount);
    }

    [Fact]
    public async Task RegisterAppointment_PreviousCancelledAppointmentAllowsNewRegistration()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AddScheduleAsync(database, registerCount: 1);
        var patient = await AddPatientAsync(database, "A123456789");
        database.Context.Appointments.Add(new Appointment
        {
            ScheduleId = schedule.ScheduleId, PatientId = patient.PatientId,
            SequenceNumber = 1, AppointmentStatus = 1
        });
        await database.Context.SaveChangesAsync();

        var result = await new AppointmentService(database.Context)
            .RegisterAppointmentAsync(patient.NationalId, schedule.ScheduleId, patient.BirthDate);

        Assert.Equal(2, result.SequenceNumber);
        Assert.Equal(2, await database.Context.Appointments.CountAsync());
    }

    private static FirstVisitRegisterRequestDto FirstVisitRequest(int scheduleId, string nationalId) => new()
    {
        ScheduleId = scheduleId,
        NationalId = nationalId,
        PatientName = "王小明",
        PhoneNumber = "0912345678",
        BirthDate = new DateTime(2000, 1, 2)
    };

    internal static async Task<Schedule> AddScheduleAsync(
        TestDatabase database, int status = 1, int maxQuota = 10, int registerCount = 0)
    {
        var doctor = new Doctor { Name = "陳醫師", DepartmentId = 1, RoomNumber = "101" };
        var schedule = new Schedule
        {
            Doctor = doctor, DoctorId = doctor.DoctorId, DepartmentId = 1,
            ServiceDate = DateTime.Today, Shift = 1, MaxQuota = maxQuota,
            CurrentRegisterCount = registerCount, Status = status
        };
        database.Context.Schedules.Add(schedule);
        await database.Context.SaveChangesAsync();
        return schedule;
    }

    internal static async Task<Patient> AddPatientAsync(TestDatabase database, string nationalId)
    {
        var patient = new Patient
        {
            NationalId = nationalId, Name = "既有病人", PhoneNumber = "0911111111",
            BirthDate = new DateTime(1990, 1, 1)
        };
        database.Context.Patients.Add(patient);
        await database.Context.SaveChangesAsync();
        return patient;
    }
}
