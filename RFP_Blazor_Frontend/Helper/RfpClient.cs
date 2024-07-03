using Microsoft.AspNetCore.Components.Forms;
using RFP_Blazor_Frontend.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RFP_Blazor_Frontend.Helper;
public class RfpClient
{
    private readonly HttpClient _httpClient;
    private string _selectedRFP = string.Empty;
    public List<string>? ListOfRfps { get; set; }
    public string SelectedRFP { 
        get
        {
            return _selectedRFP;
        }
        set
        {
            _selectedRFP = value;
        } 
    }
    public bool NewUpload { get; set; } 

    public RfpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<List<string>> GetRfpsAsync()
    {
        var response = await _httpClient.GetAsync("/rfps");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        ListOfRfps = JsonSerializer.Deserialize<List<string>>(content) ?? new List<string>();
        // return JsonSerializer.Deserialize<List<string>>(content) ?? new List<string>();
        return ListOfRfps;
    }

    public async Task<string> GetStatusAsync()
    {
        var response = await _httpClient.GetAsync("/status");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content ?? string.Empty;
    }

    public async Task<string> SelectRfpAsync(string rfpId)
    {
        var payload = new SelectRfpRequest { RfpId = rfpId };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/select-rfp", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SelectRfpResponse>(responseContent);
        return result?.Name ?? string.Empty; ;
    }

    public async Task<HttpResponseMessage> UploadFileAsync(string filePath, IBrowserFile file)
    {

        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        var formData = new MultipartFormDataContent();
        formData.Add(fileContent, "file", Path.GetFileName(filePath));

        return await _httpClient.PostAsync("/upload", formData);
    }

    public async Task<HttpResponseMessage> UploadFileAsync2(IBrowserFile file)
    {
        try
        {
            // Read stream from IBrowserFile
            // replace spaces in filename with _ 
            var filename = file.Name.Replace(" ", "_");
            using (var stream = file.OpenReadStream(maxAllowedSize: 10485760))
            {
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                var formData = new MultipartFormDataContent();
                formData.Add(fileContent, "file", filename);
                NewUpload = true;
                return await _httpClient.PostAsync("/uploadtoblob", formData);
            }
            
        }
        catch (Exception ex)
        {
            // Handle exception as needed
            NewUpload = false;  
            Console.WriteLine($"Error uploading file: {ex.Message}");
            throw;
        }
    }

    public async Task<string> ChatAsync(string message)
    {
        var payload = new ChatRequest { Message = message };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/chat", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatResponse>(responseContent);
        return result?.AiResponse ?? string.Empty;
    }

    // RDC come back to this I think I need to do some form of a yeild using 
    // IAsyncEnumerable<WeatherForecast> GetForecasts() 
    // review the following:  https://khalidabuhakmeh.com/how-to-use-iasyncenumerable-with-blazor-stream-rendering
    public async Task<Stream> ChatStreamAsync(string message)
    {
        var payload = new ChatRequest { Message = message };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/stream_chat", content);
        response.EnsureSuccessStatusCode();

        Stream responseStream = await response.Content.ReadAsStreamAsync();
        
        return responseStream;
    }

    public async Task<string> GetArtifactDataAsync(string artifactType)
    {
        var response = await _httpClient.GetAsync($"/artifact-data?artifactType={artifactType}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ArtifactDataResponse>(content);
        return result?.Details ?? string.Empty;
    }

    public async Task<List<string>> GetArtifactsAsync()
    {
        var response = await _httpClient.GetAsync("/artifacts");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<string>>(content) ?? new List<string>();
    }
}
