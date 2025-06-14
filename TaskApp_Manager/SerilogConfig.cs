using Serilog;

namespace TaskManagerApp
{
    public static class SerilogConfig
    {
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}