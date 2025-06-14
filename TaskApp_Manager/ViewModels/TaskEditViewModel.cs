using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Input;
using TaskManagerApp.Models;
namespace TaskManagerApp.ViewModels
{
    public class TaskEditViewModel : ObservableObject
    {
        private readonly Task _task;
        private readonly Window _window;
        public Task GetTask() => _task;
        public string Title
        {
            get => _task.Title;
            set => SetProperty(_task.Title, value, _task, (t, v) => t.Title = v);
        }

        public string Description
        {
            get => _task.Description;
            set => SetProperty(_task.Description, value, _task, (t, v) => t.Description = v);
        }

        public DateTime? Deadline
        {
            get => _task.Deadline;
            set => SetProperty(_task.Deadline, value, _task, (t, v) => t.Deadline = v);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public TaskEditViewModel(Task task, Window window)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
            _window = window ?? throw new ArgumentNullException(nameof(window));

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Название задачи не может быть пустым", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _task.UpdatedAt = DateTime.UtcNow;
            _window.DialogResult = true;
            _window.Close();
        }

        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}