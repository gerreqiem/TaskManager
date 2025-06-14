using Firebase.Database;
using Firebase.Database.Query;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagerApp.Models;

namespace TaskManagerApp.Services
{
    public class FirebaseSyncService : IDisposable
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly string _userId;

        public FirebaseSyncService(string firebaseToken, string userId)
        {
            if (string.IsNullOrEmpty(firebaseToken))
                throw new ArgumentNullException(nameof(firebaseToken), "Токен авторизации Firebase не может быть пустым");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "Идентификатор пользователя не может быть пустым");

            _userId = userId;

            try
            {
                _firebaseClient = new FirebaseClient(
                    "https://taskmanager-f2983-default-rtdb.firebaseio.com/",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => System.Threading.Tasks.Task.FromResult(firebaseToken)
                    });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка инициализации FirebaseClient");
                throw;
            }
        }

        public async Task<List<TaskManagerApp.Models.Task>> GetTasksAsync()
        {
            try
            {
                var firebaseTasks = await _firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .Child("tasks")
                    .OnceAsync<TaskManagerApp.Models.Task>();

                return firebaseTasks.Select(x => x.Object).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка получения задач из Firebase");
                throw;
            }
        }

        public async System.Threading.Tasks.Task SyncTasksAsync(List<TaskManagerApp.Models.Task> tasks)
        {
            try
            {
                foreach (var task in tasks)
                {
                    await _firebaseClient
                        .Child("users")
                        .Child(_userId)
                        .Child("tasks")
                        .Child(task.Id)
                        .PutAsync(task);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка синхронизации с Firebase");
                throw;
            }
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(string taskId)
        {
            try
            {
                await _firebaseClient
                    .Child("users")
                    .Child(_userId)
                    .Child("tasks")
                    .Child(taskId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка удаления задачи из Firebase");
                throw;
            }
        }

        public void Dispose()
        {
            _firebaseClient?.Dispose();
        }
    }
}