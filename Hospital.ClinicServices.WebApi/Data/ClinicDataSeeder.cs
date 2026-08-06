using Bogus;
using Hospital.ClinicServices.WebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi.Data;

public static class ClinicDataSeeder
{
    public static async Task SeedAllAsync(
        IServiceProvider services,
        int doctorCount = 8,
        int scheduleCount = 20,
        int patientCount = 30,
        int maxAppointmentsPerSchedule = 8)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

        await SeedDoctorsAsync(db, doctorCount);
        await SeedSchedulesAsync(db, scheduleCount);
        await SeedPatientsAsync(db, patientCount);
        await SeedAppointmentsAsync(db, maxAppointmentsPerSchedule);
    }

    private static async Task SeedDoctorsAsync(ClinicDbContext db, int count)
    {
        if (await db.Doctors.AnyAsync())
        {
            return;
        }

        var faker = new Faker<Doctor>("zh_TW")
            .RuleFor(d => d.Name, f => f.Name.FullName())
            .RuleFor(d => d.DepartmentId, f => f.Random.Int(1, 8))
            .RuleFor(d => d.RoomNumber, f => $"R{f.Random.Int(101, 399)}");

        db.Doctors.AddRange(faker.Generate(count));
        await db.SaveChangesAsync();
    }

    private static async Task SeedSchedulesAsync(ClinicDbContext db, int count)
    {
        if (await db.Schedules.AnyAsync())
        {
            return;
        }

        var doctors = await db.Doctors.ToListAsync();
        if (doctors.Count == 0)
        {
            return;
        }

        var faker = new Faker("zh_TW");
        var schedules = new List<Schedule>();
        var usedKeys = new HashSet<string>();

        while (schedules.Count < count)
        {
            var serviceDate = DateTime.Today.AddDays(faker.Random.Int(0, 14));
            var shift = faker.Random.Int(1, 3);
            var key = $"{serviceDate:yyyyMMdd}-{shift}";

            // Avoid duplicate (ServiceDate, Shift) combinations.
            if (!usedKeys.Add(key))
            {
                continue;
            }

            var doctor = faker.PickRandom(doctors);

            schedules.Add(new Schedule
            {
                DoctorId = doctor.DoctorId,
                DepartmentId = doctor.DepartmentId,
                ServiceDate = serviceDate,
                Shift = shift,
                MaxQuota = 30,
                CurrentRegisterCount = 0,
                CurrentCallingNumber = 0,
                Status = 0
            });
        }

        db.Schedules.AddRange(schedules);
        await db.SaveChangesAsync();
    }

    private static async Task SeedPatientsAsync(ClinicDbContext db, int count)
    {
        if (await db.Patients.AnyAsync())
        {
            return;
        }

        var nationalIdCounter = 100000000;

        var faker = new Faker<Patient>("zh_TW")
            .RuleFor(p => p.NationalId, _ => $"A{nationalIdCounter++}") // 10 碼，符合 varchar(10) 且唯一
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber("09########"))
            .RuleFor(p => p.BirthDate, f => f.Date.Between(DateTime.Today.AddYears(-90), DateTime.Today.AddYears(-1)))
            .RuleFor(p => p.IsFirstVisited, f => f.Random.Bool())
            .RuleFor(p => p.CreatedAt, _ => DateTime.Now)
            .RuleFor(p => p.UpdatedAt, _ => DateTime.Now);
        var patients = faker.Generate(count);

        db.Patients.AddRange(patients);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAppointmentsAsync(ClinicDbContext db, int maxAppointmentsPerSchedule)
    {
        if (await db.Appointments.AnyAsync())
        {
            return;
        }

        var schedules = await db.Schedules
            .OrderBy(s => s.ScheduleId)
            .ToListAsync();
        var patientIds = await db.Patients
            .Select(p => p.PatientId)
            .ToListAsync();

        if (schedules.Count == 0 || patientIds.Count == 0)
        {
            return;
        }

        var faker = new Faker("zh_TW");
        var appointments = new List<Appointment>();

        foreach (var schedule in schedules)
        {
            var registerCount = faker.Random.Int(2, Math.Min(maxAppointmentsPerSchedule, schedule.MaxQuota));

            for (var sequence = 1; sequence <= registerCount; sequence++)
            {
                appointments.Add(new Appointment
                {
                    ScheduleId = schedule.ScheduleId,
                    PatientId = faker.PickRandom(patientIds),
                    SequenceNumber = sequence,
                    AppointmentStatus = 0,
                    CreatedAt = DateTime.Now
                });
            }

            schedule.CurrentRegisterCount = registerCount;
        }

        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

    }
}