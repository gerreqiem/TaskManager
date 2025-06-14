using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Data;
using TaskManagerApp.Models;
using TaskManagerApp.Services;
using TaskManagerApp.Views;

namespace TaskManagerApp.ViewModels
{
    public class TaskReorderArgs
    {
        public TaskManagerApp.Models.Task Source { get; set; }
        public TaskManagerApp.Models.Task Target { get; set; }

        public TaskReorderArgs(TaskManagerApp.Models.Task source, TaskManagerApp.Models.Task target)
        {
            Source = source;
            Target = target;
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NotificationService _notificationService;
        private readonly FirebaseSyncService _firebaseSyncService;
        private readonly string _userId;

        private ObservableCollection<TaskManagerApp.Models.Task> _tasks = new ObservableCollection<TaskManagerApp.Models.Task>();
        private ObservableCollection<TaskManagerApp.Models.Task> _filteredTasks = new ObservableCollection<TaskManagerApp.Models.Task>();
        private ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        private Category _selectedCategory;
        private string _searchText;
        private bool _isDarkTheme;
        private TaskManagerApp.Models.Task _selectedTask;
        public StatisticsViewModel StatisticsViewModelInstance { get; } = new StatisticsViewModel();

        public ICommand ShowStatisticsCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskStatusCommand { get; }

        private readonly AppDbContext _context = new AppDbContext();
        public ICommand ToggleThemeCommand { get; }

        public ICommand ReorderTasksCommand { get; }


        public MainViewModel(string firebaseToken, string userId, NotificationService notificationService)
        {
            Log.Information("Инициализация MainViewModel для пользователя {UserId}", userId ?? "неизвестно");

            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _userId = userId;

            ShowStatisticsCommand = new RelayCommand(ShowStatistics);
            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditDeleteTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanEditDeleteTask);
            ToggleTaskStatusCommand = new RelayCommand<TaskManagerApp.Models.Task>(task =>
            {
                if (task == null) return;

                task.IsCompleted = !task.IsCompleted;
                task.UpdatedAt = DateTime.UtcNow;

                _context.Tasks.Update(task);
                _context.SaveChanges();

                RefreshTasks();
                RefreshStatistics();
            });
            ToggleThemeCommand = new RelayCommand(ToggleTheme);

            try
            {
                Log.Information("Создание FirebaseSyncService с токеном длиной {Length}", firebaseToken?.Length ?? 0);
                _firebaseSyncService = string.IsNullOrEmpty(firebaseToken) ? null : new FirebaseSyncService(firebaseToken, userId);
                Log.Information("FirebaseSyncService успешно создан или пропущен (токен пустой)");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка создания FirebaseSyncService");
                _firebaseSyncService = null;
            }
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedCategory) || e.PropertyName == nameof(SearchText))
                    FilterTasks();
            };
            ReorderTasksCommand = new RelayCommand<object>(async param =>
            {
                if (param is TaskReorderArgs args)
                {
                    await ReorderTasks(args.Source, args.Target);
                }
            });
            LoadData();
        }
        public ObservableCollection<TaskManagerApp.Models.Task> Tasks
        {
            get => _tasks;
            private set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged(nameof(Tasks));
                }
            }
        }

        public ObservableCollection<TaskManagerApp.Models.Task> FilteredTasks
        {
            get => _filteredTasks;
            private set
            {
                if (_filteredTasks != value)
                {
                    _filteredTasks = value;
                    OnPropertyChanged(nameof(FilteredTasks));
                }
            }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            private set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }
        private void LoadTasks()
        {
            var context = new AppDbContext();
            var loadedTasks = context.Tasks.Include(t => t.Category).OrderBy(t => t.SortOrder).ToList();

            Tasks.Clear();
            foreach (var task in loadedTasks)
                Tasks.Add(task);

            SubscribeToTasksEvents(); 
        }


        private void SubscribeToTasksEvents()
        {
            foreach (var task in Tasks)
            {
                task.IsCompletedChanged += Task_IsCompletedChanged;
            }
        }

        private void Task_IsCompletedChanged(object sender, EventArgs e)
        {
            var task = sender as TaskManagerApp.Models.Task;
            if (task == null) return;

            try
            {
                task.UpdatedAt = DateTime.UtcNow;

                var context = new AppDbContext();
                context.Tasks.Update(task);
                context.SaveChanges();

                var index = Tasks.IndexOf(task);
                if (index >= 0)
                    Tasks[index] = task;

                FilterTasks();

                StatisticsViewModelInstance.UpdateStatistics();

                if (_firebaseSyncService != null)
                    _ = _firebaseSyncService.SyncTasksAsync(new List<TaskManagerApp.Models.Task> { task });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обновлении статуса задачи");
                _notificationService.ShowNotification("Ошибка", "Не удалось изменить статус задачи");
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged(nameof(IsDarkTheme));
                }
            }
        }

        public TaskManagerApp.Models.Task SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != value)
                {
                    _selectedTask = value;
                    OnPropertyChanged(nameof(SelectedTask));
                    ((RelayCommand)EditTaskCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)DeleteTaskCommand).NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanEditDeleteTask() => SelectedTask != null;

        private async void LoadData()
        {
            try
            {
                Log.Information("Загрузка локальных данных");

                var context = new AppDbContext();
                await context.Database.EnsureCreatedAsync();

                if (!await context.Categories.AnyAsync())
                {
                    var defaultCategories = new List<Category>
                    {
                        new Category { Id = Guid.NewGuid().ToString(), Name = "Работа" },
                        new Category { Id = Guid.NewGuid().ToString(), Name = "Личное" },
                        new Category { Id = Guid.NewGuid().ToString(), Name = "Учеба" }
                    };

                    await context.Categories.AddRangeAsync(defaultCategories);
                    await context.SaveChangesAsync();
                }

                var localTasks = await context.Tasks
                    .Include(t => t.Category)
                    .OrderBy(t => t.SortOrder)
                    .ToListAsync();

                UpdateCollection(Tasks, localTasks);

                var dbCategories = await context.Categories.ToListAsync();
                UpdateCollection(Categories, dbCategories);

                Log.Information("Загружено {TasksCount} задач и {CategoriesCount} категорий", Tasks.Count, Categories.Count);

                await LoadFromFirebaseAsync();
                FilterTasks();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка загрузки данных");
                _notificationService.ShowNotification("Ошибка загрузки данных", ex.Message);
                Tasks.Clear();
                Categories.Clear();
                FilterTasks();
            }
        }

        private async System.Threading.Tasks.Task LoadFromFirebaseAsync()
        {
            if (_firebaseSyncService == null) return;

            try
            {
                Log.Information("Синхронизация данных с Firebase");
                var remoteTasks = await _firebaseSyncService.GetTasksAsync() ?? new List<TaskManagerApp.Models.Task>();
                Log.Information("Получено {Count} задач из Firebase", remoteTasks.Count);

                var context = new AppDbContext();

                foreach (var task in remoteTasks)
                {
                    var localTask = await context.Tasks.FindAsync(task.Id);
                    if (localTask == null)
                    {
                        context.Tasks.Add(task);
                    }
                    else if (task.UpdatedAt > localTask.UpdatedAt)
                    {
                        context.Entry(localTask).CurrentValues.SetValues(task);
                    }
                }

                await context.SaveChangesAsync();

                var updatedTasks = await context.Tasks
                    .Include(t => t.Category)
                    .OrderBy(t => t.SortOrder)
                    .ToListAsync();

                UpdateCollection(Tasks, updatedTasks);

                FilterTasks();

                Log.Information("Синхронизация с Firebase завершена");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка синхронизации с Firebase");
                _notificationService.ShowNotification("Ошибка синхронизации", ex.Message);
            }
        }

        private void ShowStatistics()
        {
            StatisticsViewModelInstance.UpdateStatistics();
            var statsWindow = new StatisticsWindow(StatisticsViewModelInstance);
            statsWindow.ShowDialog();
        }



        private void AddTask()
        {
            try
            {
                var newTask = new TaskManagerApp.Models.Task
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Новая задача",
                    Description = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CategoryId = SelectedCategory?.Id,
                    IsCompleted = false,
                    TaskDate = DateTime.UtcNow 
                };

                var editWindow = new TaskEditWindow(newTask);

                if (editWindow.ShowDialog() == true && editWindow.DataContext is TaskEditViewModel vm)
                {
                    var updatedTask = vm.GetTask();

                    if (string.IsNullOrWhiteSpace(updatedTask.Title))
                    {
                        _notificationService.ShowNotification("Ошибка", "Заголовок не может быть пустым");
                        return;
                    }

                    if (updatedTask.TaskDate.HasValue && updatedTask.TaskDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        updatedTask.IsCompleted = true;
                    }


                    var context = new AppDbContext();
                    context.Tasks.Add(updatedTask);
                    context.SaveChanges();

                    Tasks.Add(updatedTask);
                    FilterTasks();
                    RefreshStatistics();

                    if (_firebaseSyncService != null)
                        _ = _firebaseSyncService.SyncTasksAsync(new List<TaskManagerApp.Models.Task> { updatedTask });

                    SelectedTask = updatedTask;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка добавления задачи");
                _notificationService.ShowNotification("Ошибка", "Не удалось добавить задачу");
            }
        }


        private void EditTask()
        {
            if (SelectedTask.TaskDate.HasValue && SelectedTask.TaskDate.Value.Date < DateTime.UtcNow.Date)
            {
                SelectedTask.IsCompleted = true;
            }
            else
            {
                SelectedTask.IsCompleted = false;
            }


            try
            {
                var editWindow = new TaskEditWindow(SelectedTask);

                if (editWindow.ShowDialog() == true)
                {
                    SelectedTask.UpdatedAt = DateTime.UtcNow;

                    var context = new AppDbContext();
                    context.Tasks.Update(SelectedTask);
                    context.SaveChanges();

                    var index = Tasks.IndexOf(SelectedTask);
                    if (index >= 0)
                        Tasks[index] = SelectedTask;
                    FilterTasks();
                    RefreshStatistics();

                    if (_firebaseSyncService != null)
                        _ = _firebaseSyncService.SyncTasksAsync(new List<TaskManagerApp.Models.Task> { SelectedTask });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка редактирования задачи");
                _notificationService.ShowNotification("Ошибка", "Не удалось отредактировать задачу");
            }
        }

        private void DeleteTask()
        {
            if (SelectedTask == null) return;
            try
            {
                var result = MessageBox.Show($"Вы действительно хотите удалить задачу '{SelectedTask.Title}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var context = new AppDbContext();
                    context.Tasks.Remove(SelectedTask);
                    context.SaveChanges();

                    Tasks.Remove(SelectedTask);
                    FilterTasks();
                    RefreshStatistics();

                    if (_firebaseSyncService != null)
                        _ = _firebaseSyncService.DeleteTaskAsync(SelectedTask.Id);

                    SelectedTask = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка удаления задачи");
                _notificationService.ShowNotification("Ошибка", "Не удалось удалить задачу");
            }
        }

        private void RefreshTasks()
        {
            var allTasks = _context.Tasks.Include(t => t.Category).OrderBy(t => t.SortOrder).ToList();

            FilteredTasks.Clear();
            foreach (var t in allTasks)
                FilteredTasks.Add(t);
        }

        private void RefreshStatistics()
        {
            StatisticsViewModelInstance?.UpdateStatistics();
        }

        private async System.Threading.Tasks.Task ReorderTasks(TaskManagerApp.Models.Task source, TaskManagerApp.Models.Task target)
        {
            if (source == null || target == null || source == target)
                return;

            try
            {
                int sourceIndex = Tasks.IndexOf(source);
                int targetIndex = Tasks.IndexOf(target);

                if (sourceIndex < 0 || targetIndex < 0)
                    return;

                Tasks.Move(sourceIndex, targetIndex);

                for (int i = 0; i < Tasks.Count; i++)
                {
                    var task = Tasks[i];
                    if (task.SortOrder != i)
                    {
                        task.SortOrder = i;
                        task.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var context = new AppDbContext();

                foreach (var task in Tasks)
                {
                    context.Tasks.Update(task);
                }

                await context.SaveChangesAsync();

                if (_firebaseSyncService != null)
                {
                    await _firebaseSyncService.SyncTasksAsync(Tasks.ToList());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при перестановке задач");
                _notificationService.ShowNotification("Ошибка", "Не удалось изменить порядок задач");
            }
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        private void FilterTasks()
        {
            var filtered = Tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearch = SearchText.ToLowerInvariant();
                filtered = filtered.Where(t => (t.Title != null && t.Title.ToLowerInvariant().Contains(lowerSearch))
                                           || (t.Description != null && t.Description.ToLowerInvariant().Contains(lowerSearch)));
            }

            if (SelectedCategory != null)
            {
                filtered = filtered.Where(t => t.CategoryId == SelectedCategory.Id);
            }

            UpdateCollection(FilteredTasks, filtered.OrderBy(t => t.SortOrder).ToList());
        }

        private void UpdateCollection<T>(ObservableCollection<T> collection, IList<T> items)
        {
            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}