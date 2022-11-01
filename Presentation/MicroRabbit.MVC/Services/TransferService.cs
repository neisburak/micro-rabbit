using System.Text;
using System.Text.Json;
using MicroRabbit.MVC.Models.Dto;

namespace MicroRabbit.MVC.Services;

public class TransferService : ITransferService
{
    private readonly HttpClient _client;

    public TransferService(HttpClient client)
    {
        _client = client;
    }

    public async Task Transfer(TransferDto dto)
    {
        var uri = "https://localhost:5001/api/banking";
        var transferContent = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(uri, transferContent);
        response.EnsureSuccessStatusCode();
    }
}