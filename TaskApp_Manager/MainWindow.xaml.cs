using System;
using System.Windows;
using TaskManagerApp.Services;
using TaskManagerApp.ViewModels;
using Serilog;

namespace TaskManagerApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(string idToken, string userId)
        {
            Log.Information("Инициализация MainWindow для пользователя {0}, длина токена: {1}", userId, idToken?.Length ?? 0);
            InitializeComponent();

            try
            {
                Log.Information("Создание MainViewModel");
                DataContext = new MainViewModel(idToken, userId, new NotificationService());
                Log.Information("MainViewModel успешно установлен в качестве DataContext");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка инициализации MainViewModel");
                MessageBox.Show($"Ошибка инициализации главного окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}