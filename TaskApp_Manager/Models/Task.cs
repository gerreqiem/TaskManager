using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TaskManagerApp.Models
{
    public class Task : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public int SortOrder { get; set; }
        public DateTime? TaskDate { get; set; } = DateTime.UtcNow;

        public List<string> Tags { get; set; } = new List<string>();

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    IsCompletedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler IsCompletedChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
