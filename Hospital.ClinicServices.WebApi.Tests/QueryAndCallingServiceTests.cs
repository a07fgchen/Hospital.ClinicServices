using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Hubs;
using Hospital.ClinicServices.WebApi.Services;
using Hospital.ClinicServices.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hospital.ClinicServices.WebApi.Tests;

public sealed class QueryAndCallingServiceTests
{
    [Fact]
    public async Task PatientService_PersistsAllFieldsAndReturnsId()
    {
        await using var database = await TestDatabase.CreateAsync();
        var request = new PatientRegisterRequestDto
        {
            NationalId = "A123456789", Name = "王小明", PhoneNumber = "0912345678",
            BirthDate = new DateTime(1990, 1, 1)
        };

        var id = await new PatientService(database.Context).RegisterPatientAsync(request);

        var patient = await database.Context.Patients.SingleAsync();
        Assert.Equal(id, patient.PatientId);
        Assert.Equal(request.NationalId, patient.NationalId);
        Assert.Equal(request.Name, patient.Name);
        Assert.Equal(request.PhoneNumber, patient.PhoneNumber);
        Assert.Equal(request.BirthDate, patient.BirthDate);
    }

    [Fact]
    public async Task DepartmentService_ReturnsSeededDepartments()
    {
        await using var database = await TestDatabase.CreateAsync();
        var result = await new DepartmentService(database.Context).GetDepartmentsAsync();
        Assert.Equal(8, result.Count);
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(0, 2, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(-1, 1, 1)]
    [InlineData(0, 4, 1)]
    public async Task ScheduleService_AppliesWeekAndValidShiftFilters(
        int weekOffset, int shift, int expectedCount)
    {
        await using var database = await TestDatabase.CreateAsync();
        var monday = StartOfWeek(DateTime.Today);
        await AddScheduleForDateAsync(database, monday, shift: 1);
        await AddScheduleForDateAsync(database, monday, shift: 2, departmentId: 2);
        await AddScheduleForDateAsync(database, monday.AddDays(7), shift: 1);

        var result = await new ScheduleService(database.Context)
            .GetSchedulesAsync(1, weekOffset, shift);

        Assert.Equal(expectedCount, result.Count);
        if (result.Count > 0)
        {
            Assert.NotNull(result[0].Doctor);
            Assert.Equal(1, result[0].DepartmentId);
        }
    }

    [Fact]
    public async Task CallingNextClinic_IncrementsAndBroadcastsRoomAndNumber()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AppointmentServiceTests.AddScheduleAsync(database, registerCount: 2);
        var proxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>();
        clients.Setup(value => value.Group($"Clinic_{schedule.ScheduleId}")).Returns(proxy.Object);
        var hub = new Mock<IHubContext<QueueHub>>();
        hub.SetupGet(value => value.Clients).Returns(clients.Object);

        var result = await new CallingService(database.Context, hub.Object)
            .CallingNextClinicAsync(schedule.ScheduleId);

        Assert.Equal(1, result.CurrentCallingNumber);
        proxy.Verify(value => value.SendCoreAsync(
            "ReceiveNumberUpdate",
            It.Is<object?[]>(arguments => HasProperty(arguments[0]!, "RoomNumber", "101") &&
                HasProperty(arguments[0]!, "CurrentCallingNumber", 1)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CallingNextClinic_MissingScheduleRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        await Assert.ThrowsAsync<Exception>(() =>
            new CallingService(database.Context, Mock.Of<IHubContext<QueueHub>>())
                .CallingNextClinicAsync(999));
    }

    [Fact]
    public async Task CallingNextClinic_NotInProgressRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AppointmentServiceTests.AddScheduleAsync(database, status: 0);
        await Assert.ThrowsAsync<Exception>(() =>
            new CallingService(database.Context, Mock.Of<IHubContext<QueueHub>>())
                .CallingNextClinicAsync(schedule.ScheduleId));
    }

    [Fact]
    public async Task CallingNextClinic_NoWaitingPatientRejects()
    {
        await using var database = await TestDatabase.CreateAsync();
        var schedule = await AppointmentServiceTests.AddScheduleAsync(database, registerCount: 0);
        await Assert.ThrowsAsync<Exception>(() =>
            new CallingService(database.Context, Mock.Of<IHubContext<QueueHub>>())
                .CallingNextClinicAsync(schedule.ScheduleId));
    }

    private static object? Property(object instance, string name) =>
        instance.GetType().GetProperty(name)?.GetValue(instance);

    private static bool HasProperty(object instance, string name, object expected) =>
        Equals(Property(instance, name), expected);

    private static DateTime StartOfWeek(DateTime date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private static async Task AddScheduleForDateAsync(
        TestDatabase database, DateTime date, int shift, int departmentId = 1)
    {
        var doctor = new Doctor { Name = Guid.NewGuid().ToString(), DepartmentId = departmentId, RoomNumber = "1" };
        database.Context.Schedules.Add(new Schedule
        {
            Doctor = doctor, DepartmentId = departmentId, ServiceDate = date, Shift = shift,
            MaxQuota = 10, Status = 1
        });
        await database.Context.SaveChangesAsync();
    }
}
