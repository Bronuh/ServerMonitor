namespace ServerMonitor.Notifiers;

public class TelegramNotifier : INotifier
{
    private HttpClient _client = new HttpClient();
    private string _telegramToken;
    private string _telegramChatId;

    public TelegramNotifier(string telegramToken, string telegramChatId)
    {
        _telegramToken = telegramToken;
        _telegramChatId = telegramChatId;
    }

    public async Task SendMessageAsync(string message)
    {
        var url = $"https://api.telegram.org/bot{_telegramToken}/sendMessage";
        var content = new FormUrlEncodedContent(new []
        {
            new KeyValuePair<string, string>("chat_id", _telegramChatId),
            new KeyValuePair<string, string>("text", message)
        });
        await _client.PostAsync(url, content);
    }
}