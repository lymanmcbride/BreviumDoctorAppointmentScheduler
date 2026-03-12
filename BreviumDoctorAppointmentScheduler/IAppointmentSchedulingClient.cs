namespace BreviumDoctorAppointmentScheduler;

public interface IAppointmentSchedulingClient
{
    Task Start();
    Task CreateAppointment(DoctorAppointment appointment);
    Task<IEnumerable<DoctorAppointment>> GetSchedule();
    Task<SchedulingInquiry?> GetNextInquiry();
}