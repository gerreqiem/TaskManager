using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Services;
using Serilog;

namespace TaskManagerApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly FirebaseAuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        public string Password { get; set; }
        public string Email { get; set; }
        public ICommand SignInCommand { get; }
        public ICommand SignUpCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand SignInWithGoogleCommand { get; }

        public event Action<string, string> LoginSuccessful;

        public LoginViewModel(FirebaseAuthService authService)
        {
            _authService = authService;

            SignInCommand = new RelayCommand(async () => await SignInAsync());
            SignUpCommand = new RelayCommand(async () => await SignUpAsync());
            ForgotPasswordCommand = new RelayCommand(async () => await ForgotPasswordAsync());
            SignInWithGoogleCommand = new RelayCommand(StartGoogleSignIn);

            Log.Information("LoginViewModel инициализирован");
        }

        private async Task SignInAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Log.Warning("Попытка входа с пустыми полями Email или Password");
                MessageBox.Show("Введите email и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Log.Information("Попытка входа с email: {0}", Email);
                var result = await _authService.SignInWithEmailAsync(Email, Password);
                Log.Information("Успешный вход, UserId: {0}", result.UserId);
                LoginSuccessful?.Invoke(result.IdToken, result.UserId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка входа с email: {0}", Email);
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SignUpAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Log.Warning("Попытка регистрации с пустыми полями Email или Password");
                MessageBox.Show("Введите email и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Log.Information("Попытка регистрации с email: {0}", Email);
                var result = await _authService.SignUpWithEmailAsync(Email, Password);
                Log.Information("Успешная регистрация, UserId: {0}", result.UserId);
                MessageBox.Show("Регистрация успешна! Проверьте email.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка регистрации с email: {0}", Email);
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ForgotPasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Log.Warning("Попытка сброса пароля с пустым Email");
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Log.Information("Отправка запроса на сброс пароля для email: {0}", Email);
                await _authService.SendPasswordResetEmailAsync(Email);
                Log.Information("Письмо для сброса пароля отправлено для email: {0}", Email);
                MessageBox.Show("Письмо для сброса пароля отправлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка сброса пароля для email: {0}", Email);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartGoogleSignIn()
        {
            try
            {
                Log.Information("Запуск авторизации через Google");
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://accounts.google.com/o/oauth2/auth?" +
                               $"client_id=122113243479-tfq6i06dmib5fti3m6svmr4mfgibm06e.apps.googleusercontent.com&" +
                               $"redirect_uri=http://localhost:8080/oauth-callback&" +
                               $"response_type=code&" +
                               $"scope=email profile&" +
                               $"state=state_parameter_passthrough_value", 
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка запуска авторизации через Google");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}