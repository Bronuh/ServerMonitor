using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;

namespace ServerMonitor;

class Program
{
    private static HttpClient client = new HttpClient();
    private static string serverUrl;
    private static int checkInterval;
    private static string telegramToken;
    private static string telegramChatId;
    private static string pingTarget;
    
    private static bool serverWasAvailable = true;
    private static bool networkWasAvailable = true;
    
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        serverUrl = config["SERVER_URL"];
        checkInterval = int.Parse(config["CHECK_INTERVAL"] ?? "10");
        telegramToken = config["TELEGRAM_TOKEN"];
        telegramChatId = config["TELEGRAM_CHAT_ID"];
        pingTarget = config["PING_TARGET"];
        
        while (true)
        {
            bool isNetworkAvailable = await PingRemoteGate();
            bool isServerAvailable = await CheckServerAvailability();
            Console.WriteLine($"Remote availability:\n Network: {isNetworkAvailable}\n Server: {isServerAvailable}");
            
            if (isServerAvailable && !serverWasAvailable)
            {
                Console.WriteLine($"Server {serverUrl} is available. Sending notification.");
                await SendTelegramMessage($"✅ [{GetTime()}] Домашний сервер доступен снова.");
            }
            
            else if (!isServerAvailable && serverWasAvailable)
            {
                Console.WriteLine($"Server {serverUrl} is NOT available. Sending notification.");
                await SendTelegramMessage($"⚠️ [{GetTime()}] Домашний сервер недоступен.");
            }

            if (isNetworkAvailable && !networkWasAvailable)
            {
                Console.WriteLine($"Network {serverUrl} is available. Sending notification.");
                await SendTelegramMessage($"✅ [{GetTime()}] Домашняя сеть доступна");
            }

            if (!isNetworkAvailable && networkWasAvailable)
            {
                Console.WriteLine($"Network {serverUrl} is NOT available. Sending notification.");
                await SendTelegramMessage($"⚠️ [{GetTime()}] Домашняя сеть недоступна");
            }
            
            serverWasAvailable = isServerAvailable;
            networkWasAvailable = isNetworkAvailable;

            Thread.Sleep(checkInterval * 1000);
        }
    }
    
    static async Task<bool> CheckServerAvailability()
    {
        try
        {
            Console.WriteLine($"Checking server status at {serverUrl}");
            HttpResponseMessage response = await client.GetAsync(serverUrl);
            Console.WriteLine($"Server response status: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    static async Task<bool> PingRemoteGate()
    {
        using (Ping ping = new Ping())
        {
            try
            {
                PingReply reply = await ping.SendPingAsync(pingTarget, 1000); // 1000ms timeout
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Ping to {pingTarget} successful:");
                    Console.WriteLine($"  Address: {reply.Address}");
                    Console.WriteLine($"  RoundTrip time: {reply.RoundtripTime} ms");
                    Console.WriteLine($"  Time to live: {reply.Options?.Ttl}");
                    Console.WriteLine($"  Buffer size: {reply.Buffer.Length}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Ping to {pingTarget} failed: {reply.Status}");
                    return false;
                }
            }
            catch (PingException ex)
            {
                Console.WriteLine($"Ping failed: {ex.Message}");
                return false;
            }
        }
    }

    static async Task SendTelegramMessage(string message)
    {
        var url = $"https://api.telegram.org/bot{telegramToken}/sendMessage";
        var content = new FormUrlEncodedContent(new []
        {
            new KeyValuePair<string, string>("chat_id", telegramChatId),
            new KeyValuePair<string, string>("text", message)
        });
        await client.PostAsync(url, content);
    }

    static string GetTime()
    {
        return $"{DateTime.Now:d.M.yyyy hh:mm:ss}";
    }
}