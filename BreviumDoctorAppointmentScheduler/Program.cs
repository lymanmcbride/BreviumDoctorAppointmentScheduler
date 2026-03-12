// See https://aka.ms/new-console-template for more information

using BreviumDoctorAppointmentScheduler;

Console.WriteLine("Hello, World!");

var appointmentSchedulingClient = new AppointmentSchedulingClient();

await appointmentSchedulingClient.Start();

var schedule = await appointmentSchedulingClient.GetSchedule();
