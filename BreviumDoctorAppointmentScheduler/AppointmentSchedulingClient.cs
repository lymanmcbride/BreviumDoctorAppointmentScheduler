using System.Net;
using System.Text;
using System.Text.Json;

namespace BreviumDoctorAppointmentScheduler;

public class AppointmentSchedulingClient : IAppointmentSchedulingClient
{
    private const string ApiKey = "YOUR_API_KEY_HERE";
    private const string BaseApiDomain = "https://scheduling.interviews.brevium.com";
    private readonly HttpClient _client = new();

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    public AppointmentSchedulingClient()
    {
        _client.BaseAddress = new Uri(BaseApiDomain);
    }

    public async Task Start()
    {
        await _client.PostAsync($"api/scheduling/start?token={ApiKey}", null);
    }
    
    public async Task CreateAppointment(DoctorAppointment appointment)
    {
        string requestBody = JsonSerializer.Serialize(appointment, _options);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        await _client.PostAsync($"api/scheduling/schedule?token={ApiKey}", content);
    }
    
    public async Task<IEnumerable<DoctorAppointment>> GetSchedule()
    {
        HttpResponseMessage response = await _client.GetAsync($"api/scheduling/schedule?token={ApiKey}");

        return await ValidateAndDeserialize<List<DoctorAppointment>>(response);
    }

    public async Task<SchedulingInquiry?> GetNextInquiry()
    {
        HttpResponseMessage response = await _client.GetAsync($"api/scheduling/appointmentrequest?token={ApiKey}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }
        
        return await ValidateAndDeserialize<SchedulingInquiry>(response);
    }
    
    private async Task<T> ValidateAndDeserialize<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new SchedulingApiException($"Request failed to {response.RequestMessage?.RequestUri} with status code {response.StatusCode} and message {response.ReasonPhrase}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new Exception("Failed to get schedule");
        }
        
        T? result = JsonSerializer.Deserialize<T>(responseJson, _options);

        if (result is null)
        {
            throw new Exception("Failed to get schedule");
        }

        return result;
    }
}