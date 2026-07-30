using Hospital.ClinicServices.WebApi.Data;
using Hospital.ClinicServices.WebApi.DTOs;
using Hospital.ClinicServices.WebApi.Entities;
using Hospital.ClinicServices.WebApi.Services;
using Microsoft.EntityFrameworkCore;

namespace Hospital.ClinicServices.WebApi;

public class AppointmentService : IAppointmentService
{
    private readonly ClinicDbContext _context;

    public AppointmentService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment> RegisterAppointmentAsync(RegisterRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var schedule = await _context.Schedules
            //相當於Mysql的SELECT ... FOR UPDATE，鎖定該筆排班資料，避免同時有多個使用者搶同一個排班
            .FromSqlRaw("SELECT * FROM Schedules WITH (UPDLOCK, ROWLOCK) WHERE ScheduleId = {0}", request.ScheduleId)
            .FirstOrDefaultAsync();

            if (schedule == null)
            {
                throw new Exception("找不到該門診排班資訊。");
            }

            if (schedule.Status == 2 || schedule.Status == 3)
            {
                throw new Exception("該門診已休診或已結束，無法掛號。");
            }

            if (schedule.CurrentRegisterCount >= schedule.MaxQuota)
            {
                throw new Exception("非常抱歉，該門診預約名額已滿。");
            }
            // 4. 檢查該病人是否已經掛過這一診，避免重複掛號
            bool isAlreadyRegisterd = await _context.Appointments
            .AnyAsync(a => a.ScheduleId == request.ScheduleId && a.PatientId == request.PatientId && a.AppointmentStatus == 1);

            if (isAlreadyRegisterd)
            {
                throw new Exception("您已預約過此門診，請勿重複掛號。");
            }

            // 5. 計算看診號碼：目前的已掛號人數 + 1
            int sequenceNumber = schedule.CurrentRegisterCount + 1;
            // 6. 更新排班表上的已掛號人數
            schedule.CurrentRegisterCount = sequenceNumber;
            _context.Schedules.Update(schedule);

            // 7. 新增掛號紀錄
            var newAppointment = new Appointment
            {
                ScheduleId = request.ScheduleId,
                PatientId = request.PatientId,
                SequenceNumber = sequenceNumber,
                AppointmentStatus = 1, // 1:預約成功
                CreatedAt = DateTime.UtcNow
            };
            await _context.Appointments.AddAsync(newAppointment);

            // 8. 儲存變更
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return newAppointment;
        }
        catch (Exception)
        {
            // 發生任何錯誤，自動回滾，確保資料庫不會有髒資料
            await transaction.RollbackAsync();
            throw;
        }
    }
}
