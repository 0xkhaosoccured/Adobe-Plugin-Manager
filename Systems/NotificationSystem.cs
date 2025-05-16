namespace PluginManager;

public interface INotificationSystem
{
      void Message(string message);
}

public class NotificationSystem : INotificationSystem
{
      public void Message(string message) => Console.WriteLine(message);
}