namespace AdBallApi.Application.Services;

public interface IPushNotificationService
{
    Task SendToUserAsync(long userId, string title, string body, Dictionary<string, string>? data = null);
    Task SendBatchAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null);
}
