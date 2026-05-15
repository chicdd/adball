using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IFcmTokenRepository _fcmTokens;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(IFcmTokenRepository fcmTokens, ILogger<PushNotificationService> logger)
    {
        _fcmTokens = fcmTokens;
        _logger = logger;
    }

    public async Task SendToUserAsync(long userId, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokens = await _fcmTokens.GetByUserIdAsync(userId);
        if (tokens.Count == 0) return;

        await SendBatchAsync(tokens.Select(t => t.Token), title, body, data);
    }

    public async Task SendBatchAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokenList = tokens.ToList();
        if (tokenList.Count == 0) return;

        const int batchSize = 500;
        for (int i = 0; i < tokenList.Count; i += batchSize)
        {
            var batch = tokenList.Skip(i).Take(batchSize).ToList();
            try
            {
                var messages = batch.Select(token => new Message
                {
                    Token = token,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data ?? new Dictionary<string, string>()
                }).ToList();

                var messaging = FirebaseMessaging.DefaultInstance;
                foreach (var msg in messages)
                {
                    try { await messaging.SendAsync(msg); }
                    catch (FirebaseMessagingException fex) when (fex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        await _fcmTokens.DeleteAsync(msg.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FCM 배치 발송 실패. Batch={Start}-{End}", i, i + batch.Count);
            }

            if (i + batchSize < tokenList.Count)
                await Task.Delay(1000);
        }
    }
}
