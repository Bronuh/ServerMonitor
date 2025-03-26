using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using NtpClient;
using ServerMonitor.Notifiers;

namespace ServerMonitor;

class Program
{
    private static HttpClient client = new HttpClient();
    private static string serverUrl;
    private static int checkInterval;
    
    private static bool serverWasAvailable = true;
    private static bool networkWasAvailable = true;
    private static bool pingEnabled;
    private static string pingTarget;
    private static int pingAttempts;
    private static int failedAttempts = 0;
    
    private static string telegramToken;
    private static string telegramChatId;
    private static INotifier notifier;
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        serverUrl = config["SERVER_URL"];
        checkInterval = Int32.Parse(config["CHECK_INTERVAL"] ?? "10");
        telegramToken = config["TELEGRAM_TOKEN"];
        telegramChatId = config["TELEGRAM_CHAT_ID"];
        
        pingEnabled = Boolean.Parse(config["DO_PING_CHECK"] ?? "true");
        pingTarget = config["PING_TARGET"];
        pingAttempts = Int32.Parse(config["PING_ATTEMPTS"] ?? "3");

        notifier = new TelegramNotifier(telegramToken, telegramChatId);

        Console.WriteLine($"Running monitor for server at {serverUrl}.\n" +
                          $"Interval: {checkInterval}\n" +
                          $"Ping enabled: {pingEnabled}\n" +
                          $"Ping target: {pingTarget}\n" +
                          $"Ping attempts: {pingAttempts}");
        
        while (true)
        {
            bool isServerAvailable = await CheckServerAvailability();
            
            // http success after fail
            if (isServerAvailable && !serverWasAvailable)
            {
                Console.WriteLine($"Server {serverUrl} is available. Sending notification.");
                await SendTelegramMessage($"✅🖥 [{GetTime()}] Домашний сервер доступен снова.");
            }
            
            // http fail after success
            else if (!isServerAvailable && serverWasAvailable)
            {
                Console.WriteLine($"Server {serverUrl} is NOT available. Sending notification.");
                await SendTelegramMessage($"⚠️🖥 [{GetTime()}] Домашний сервер недоступен.");
            }

            bool isNetworkAvailable = false;
            if (pingEnabled)
            {
                isNetworkAvailable = await PingRemoteGate();
                // ping success after fail event
                if (isNetworkAvailable && !networkWasAvailable)
                {
                    Console.WriteLine($"Network {serverUrl} is available. Sending notification.");
                    networkWasAvailable = true;
                    failedAttempts = 0;
                    await SendTelegramMessage($"✅🌐 [{GetTime()}] Домашняя сеть доступна");
                }

                // ping fail after success event
                if (!isNetworkAvailable && networkWasAvailable)
                {
                    failedAttempts++;
                    Console.WriteLine($"$Failed ping {failedAttempts}/{pingAttempts}");
                    if (failedAttempts >= pingAttempts)
                    {
                        Console.WriteLine($"Network {serverUrl} is NOT available. Sending notification.");
                        await SendTelegramMessage($"⚠️🌐 [{GetTime()}] Домашняя сеть недоступна");
                        networkWasAvailable = isNetworkAvailable;
                        networkWasAvailable = false;
                    }
                }
            }
            
            
            Console.WriteLine($"Remote availability:\n " +
                              $"Server: {isServerAvailable} " +
                              $"{(pingEnabled ? $"\n Network: {isNetworkAvailable}" : "")}"); // only show network if ping is enabled
            serverWasAvailable = isServerAvailable;

            Thread.Sleep(checkInterval * 1000);
        }
    }
    
    static async Task<bool> CheckServerAvailability()
    {
        try
        {
            //Console.WriteLine($"Checking server status at {serverUrl}");
            HttpResponseMessage response = await client.GetAsync(serverUrl);
            //Console.WriteLine($"Server response status: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Checking server status at {serverUrl} errored: {ex.Message}");
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
                    Console.WriteLine($"Ping to {pingTarget} successful: delay {reply.RoundtripTime}ms, TTL {reply.Options?.Ttl}");
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
        await notifier.SendMessageAsync(message);
    }

    static string GetTime()
    {
        var connection = new NtpConnection("time.google.com");
        var utcNow = connection.GetUtc(); 
        
        TimeZoneInfo moscowZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        DateTime moscowTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, moscowZone);
        
        return $"{moscowTime:d.MM.yyyy HH:mm:ss}";
    }
}