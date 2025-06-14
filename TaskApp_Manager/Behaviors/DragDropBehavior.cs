using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskManagerApp.ViewModels;
using ModelTask = TaskManagerApp.Models.Task;

namespace TaskManagerApp.Behaviors
{
    public class DragDropBehavior : Behavior<ListView>
    {
        private ModelTask _draggedItem;
        private Point _startPoint;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.AllowDrop = true; 
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource == null) return;

            var listViewItem = FindAncestor<ListViewItem>(originalSource);
            if (listViewItem == null) return;

            var task = listViewItem.DataContext as ModelTask;
            if (task != null)
            {
                _draggedItem = task;
                _startPoint = e.GetPosition(null);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
                return;

            Point currentPosition = e.GetPosition(null);
            Vector diff = _startPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                DragDrop.DoDragDrop(AssociatedObject, _draggedItem, DragDropEffects.Move);
                _draggedItem = null;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                    return (T)current;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var listView = AssociatedObject;
            var viewModel = listView.DataContext as MainViewModel;
            if (viewModel == null) return;

            Point position = e.GetPosition(listView);
            var element = listView.InputHitTest(position) as DependencyObject;
            var listViewItem = FindAncestor<ListViewItem>(element);

            if (listViewItem == null) return;

            var droppedTask = listViewItem.DataContext as ModelTask;
            if (droppedTask == null || droppedTask == _draggedItem) return;

            int draggedIndex = viewModel.FilteredTasks.IndexOf(_draggedItem);
            int droppedIndex = viewModel.FilteredTasks.IndexOf(droppedTask);

            if (draggedIndex >= 0 && droppedIndex >= 0 && draggedIndex != droppedIndex)
            {
                viewModel.FilteredTasks.Move(draggedIndex, droppedIndex);

                if (viewModel.ReorderTasksCommand?.CanExecute(new TaskReorderArgs(_draggedItem, droppedTask)) == true)
                {
                    viewModel.ReorderTasksCommand.Execute(new TaskReorderArgs(_draggedItem, droppedTask));
                }
            }

            _draggedItem = null;
        }
    }
}
