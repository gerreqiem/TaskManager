using System.Windows;

namespace TaskManagerApp.Services
{
    public class NotificationService
    {
        public void ShowNotification(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}