using System;
using System.Windows;
using TaskManagerApp.ViewModels;

namespace TaskManagerApp.Views
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(StatisticsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

    }
}
