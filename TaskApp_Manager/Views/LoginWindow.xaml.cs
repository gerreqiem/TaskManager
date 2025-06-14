using Serilog;
using System;
using System.Windows;
using TaskManagerApp.Services;
using TaskManagerApp.ViewModels;

namespace TaskManagerApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(FirebaseAuthService authService)
        {
            try
            {
                InitializeComponent();

                var viewModel = new LoginViewModel(authService);
                DataContext = viewModel;

                PasswordBox.PasswordChanged += (s, e) => viewModel.Password = PasswordBox.Password;

                viewModel.LoginSuccessful += (idToken, userId) =>
                {
                    var mainWindow = new MainWindow(idToken, userId);
                    mainWindow.Show();
                    Close();
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка инициализации LoginWindow");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}