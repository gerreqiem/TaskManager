using Serilog;
using System;
using System.IO;
using System.Windows;
using TaskManagerApp.Services;
using TaskManagerApp.Views;

namespace TaskManagerApp
{
    public partial class App : Application
    {
        private OAuthListener _oauthListener;
        private FirebaseAuthService _authService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Запуск приложения в {0}", DateTime.Now);
            SQLitePCL.Batteries_V2.Init();
            Log.Information("SQLite инициализирован");
            _authService = new FirebaseAuthService();
            Log.Information("FirebaseAuthService создан");
            try
            {
                Log.Information("Принудительное открытие MainWindow с пустыми параметрами в {0}", DateTime.Now);
                var mainWindow = new MainWindow(null, null);
                Log.Information("MainWindow создан, отображение... в {0}", DateTime.Now);
                mainWindow.Show();
                Log.Information("MainWindow успешно открыт в {0}", DateTime.Now);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка открытия MainWindow в {0}", DateTime.Now);
                MessageBox.Show($"Ошибка открытия главного окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Выход из приложения в {0}", DateTime.Now);
            _oauthListener?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
//using Serilog;
//using System;
//using System.IO;
//using System.Windows;
//using TaskManagerApp.Services;
//using TaskManagerApp.Views;

//namespace TaskManagerApp
//{
//    public partial class App : Application
//    {
//        private OAuthListener _oauthListener;
//        private FirebaseAuthService _authService;

//        protected override void OnStartup(StartupEventArgs e)
//        {
//            base.OnStartup(e);

//            // Настройка логгера
//            Directory.CreateDirectory("logs");
//            Log.Logger = new LoggerConfiguration()
//                .MinimumLevel.Debug()
//                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
//                .CreateLogger();

//            Log.Information("Запуск приложения в {0}", DateTime.Now);
//            SQLitePCL.Batteries_V2.Init();
//            Log.Information("SQLite инициализирован");
//            _authService = new FirebaseAuthService();
//            Log.Information("FirebaseAuthService создан");
//            try
//            {
//                Log.Information("Открытие LoginWindow в {0}", DateTime.Now);
//                var loginWindow = new LoginWindow(_authService);
//                loginWindow.Show();
//                Log.Information("LoginWindow успешно открыт в {0}", DateTime.Now);
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex, "Ошибка открытия LoginWindow в {0}", DateTime.Now);
//                MessageBox.Show($"Ошибка открытия окна авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                Shutdown();
//            }
//        }

//        protected override void OnExit(ExitEventArgs e)
//        {
//            Log.Information("Выход из приложения в {0}", DateTime.Now);
//            _oauthListener?.Dispose();
//            Log.CloseAndFlush();
//            base.OnExit(e);
//        }
//    }
//}