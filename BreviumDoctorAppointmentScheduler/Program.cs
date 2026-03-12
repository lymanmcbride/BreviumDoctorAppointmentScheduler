// See https://aka.ms/new-console-template for more information

using BreviumDoctorAppointmentScheduler;

Console.WriteLine("Hello, World!");

var appointmentSchedulingClient = new AppointmentSchedulingClient();

await appointmentSchedulingClient.Start();

var appointmentScheduler = new AppointmentScheduler(appointmentSchedulingClient);

await appointmentScheduler.ExecuteAsync();

Console.WriteLine("Done");
