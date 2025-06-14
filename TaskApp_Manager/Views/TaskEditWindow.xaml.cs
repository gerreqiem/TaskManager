using System.Windows;
using TaskManagerApp.Models;
using TaskManagerApp.ViewModels;

namespace TaskManagerApp.Views
{
    public partial class TaskEditWindow : Window
    {
        public TaskEditWindow(Task task)
        {
            InitializeComponent();
            DataContext = new TaskEditViewModel(task, this);
        }
    }
}