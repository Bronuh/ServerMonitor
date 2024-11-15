namespace ServerMonitor.Notifiers;

public interface INotifier
{
    Task SendMessageAsync(string message);
}