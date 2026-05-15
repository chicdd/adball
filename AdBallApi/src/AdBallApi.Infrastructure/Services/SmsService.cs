using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public class AligoSmsService : ISmsService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<AligoSmsService> _logger;

    public AligoSmsService(HttpClient http, IConfiguration cfg, ILogger<AligoSmsService> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        var apiKey = _cfg["Sms:AligoApiKey"];
        var userId = _cfg["Sms:AligoUserId"];
        var sender = _cfg["Sms:SenderPhone"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("[SMS-DEV] {Phone}: {Msg}", phoneNumber, message);
            return true;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["key"] = apiKey!,
                ["user_id"] = userId!,
                ["sender"] = sender!,
                ["receiver"] = phoneNumber,
                ["msg"] = message,
                ["testmode_yn"] = "N"
            });

            var response = await _http.PostAsync("https://apis.aligo.in/send/", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS 발송 실패. Phone={Phone}", phoneNumber);
            return false;
        }
    }
}
