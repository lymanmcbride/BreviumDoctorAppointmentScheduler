namespace BreviumDoctorAppointmentScheduler;

public class SchedulingInquiry
{
    public int RequestId { get; set; }
    public int PersonId { get; set; }
    public List<DateTime> PreferredDays { get; set; }
    public List<int> PreferredDocs { get; set; }
    public bool IsNew { get; set; }
}