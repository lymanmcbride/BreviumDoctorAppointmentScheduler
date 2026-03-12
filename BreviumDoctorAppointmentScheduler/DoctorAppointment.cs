namespace BreviumDoctorAppointmentScheduler;

public class DoctorAppointment
{
    public int DoctorId { get; set; }
    public int PersonId { get; set; }
    public DateTimeOffset AppointmentTime { get; set; }
    public bool IsNewPatientAppointment { get; set; }
}