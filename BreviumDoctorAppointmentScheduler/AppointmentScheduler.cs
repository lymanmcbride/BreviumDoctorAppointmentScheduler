namespace BreviumDoctorAppointmentScheduler;

public class AppointmentScheduler(IAppointmentSchedulingClient client)
{
    private readonly HashSet<(int, DateTimeOffset)> _scheduledDoctorAppointments = []; 
    private readonly Dictionary<int, SortedSet<DateTimeOffset>> _patientAppointments = [];

    public async Task ExecuteAsync()
    {
        IEnumerable<DoctorAppointment> initialSchedule = await client.GetSchedule();

        foreach (DoctorAppointment doctorAppointment in initialSchedule)
        {
            AddScheduledDoctorAppointment(doctorAppointment);
            AddPatientAppointment(doctorAppointment);
        }
        
        SchedulingInquiry? inquiry = await client.GetNextInquiry();
        while (inquiry != null)
        {
            DateTimeOffset? appointmentTime = null;
            int doctorId = 0;
            foreach (var day in FilterPreferredDates(inquiry))
            {
                if (!IsPatientInScheduleableWindow(inquiry, day))
                {
                    continue;
                }
                var startTime = inquiry.IsNew ? 15 : 8;
                foreach (var timeSlot in GenerateSlotsForTheDay(day, startTime))
                {
                    for (int doctor = 1; doctor <= 3; doctor++)
                    {
                        if (IsTimeSlotAvailable(doctor, timeSlot))
                        {
                            appointmentTime = timeSlot;
                            doctorId = doctor;
                            break;
                        }
                        Console.WriteLine("slot not available");
                    }

                    if (appointmentTime is not null)
                        break;
                }

                if (appointmentTime is not null)
                    break;
            }

            if (appointmentTime is null)
            {
                Console.WriteLine($"No slots available for patient {inquiry.PersonId}");
            }
            else
            {
                var appointmentToSchedule = new DoctorAppointment
                {
                    AppointmentTime = appointmentTime.Value,
                    PersonId = inquiry.PersonId,
                    DoctorId = doctorId,
                    IsNewPatientAppointment = inquiry.IsNew
                };
                await client.CreateAppointment(appointmentToSchedule);
                AddScheduledDoctorAppointment(appointmentToSchedule);
                AddPatientAppointment(appointmentToSchedule);
            }
            inquiry = await client.GetNextInquiry();
        }
        Console.WriteLine("Done");
    }

    private bool IsPatientInScheduleableWindow(SchedulingInquiry inquiry, DateTimeOffset timeSlot)
    {
        return !_patientAppointments.ContainsKey(inquiry.PersonId) || _patientAppointments[inquiry.PersonId].GetViewBetween(timeSlot.AddDays(-6), timeSlot.AddDays(6)).Count == 0;
    }

    private bool IsTimeSlotAvailable(int doctorId, DateTimeOffset timeSlot)
    {
        return !_scheduledDoctorAppointments.Contains((doctorId, timeSlot));
    }

    private static HashSet<DateTimeOffset> GenerateSlotsForTheDay(DateTime day, int startTime)
    {
        var slots = new HashSet<DateTimeOffset>();
        var dateTime = new DateTimeOffset(day.Date, TimeSpan.Zero);
        dateTime = dateTime.AddHours(startTime);
        while (dateTime.Hour < 17)
        {
            slots.Add(dateTime);
            dateTime = dateTime.AddHours(1);
        }

        return slots;
    }

    private static IEnumerable<DateTime> FilterPreferredDates(SchedulingInquiry inquiry)
    {
        return inquiry.PreferredDays.Where(day => day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday
            && day.Date >= new DateTime(2021, 11, 1) && day.Date <= new DateTime(2021, 12, 31));
    }

    private void AddPatientAppointment(DoctorAppointment doctorAppointment)
    {
        if (_patientAppointments.ContainsKey(doctorAppointment.PersonId))
        {
            _patientAppointments[doctorAppointment.PersonId]
                .Add(doctorAppointment.AppointmentTime.ToUniversalTime());
        }
        else
        {
            _patientAppointments.Add(doctorAppointment.PersonId,
                [doctorAppointment.AppointmentTime.ToUniversalTime()]);
        }
    }

    private void AddScheduledDoctorAppointment(DoctorAppointment doctorAppointment)
    {
        _scheduledDoctorAppointments.Add((doctorAppointment.PersonId, doctorAppointment.AppointmentTime.ToUniversalTime()));
    }
}