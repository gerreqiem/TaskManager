using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System;
using System.Linq;
using TaskManagerApp.Data;

public class StatisticsViewModel : INotifyPropertyChanged
{
    private SeriesCollection categorySeries;
    public SeriesCollection CategorySeries
    {
        get => categorySeries;
        set
        {
            if (categorySeries != value)
            {
                categorySeries = value;
                OnPropertyChanged(nameof(CategorySeries));
            }
        }
    }

    private SeriesCollection statusSeries;
    public SeriesCollection StatusSeries
    {
        get => statusSeries;
        set
        {
            if (statusSeries != value)
            {
                statusSeries = value;
                OnPropertyChanged(nameof(StatusSeries));
            }
        }
    }

    public StatisticsViewModel()
    {
        CategorySeries = new SeriesCollection();
        StatusSeries = new SeriesCollection();
    }

    public void UpdateStatistics()
    {
        var context = new AppDbContext();

        var tasks = context.Tasks.ToList();
        var categories = context.Categories.ToList();

        CategorySeries.Clear();
        StatusSeries.Clear();

        var tasksByCategory = tasks
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                Category = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Без категории",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        foreach (var item in tasksByCategory)
        {
            CategorySeries.Add(new PieSeries
            {
                Title = item.Category,
                Values = new ChartValues<int> { item.Count },
                DataLabels = true,
                LabelPoint = chartPoint => $"{item.Category}: {chartPoint.Y}"
            });
        }

        var tasksByStatus = tasks
            .GroupBy(t => t.IsCompleted)
            .Select(g => new
            {
                Status = g.Key ? "Выполнено" : "Не выполнено",
                Count = g.Count()
            })
            .ToList();

        foreach (var item in tasksByStatus)
        {
            StatusSeries.Add(new PieSeries
            {
                Title = item.Status,
                Values = new ChartValues<int> { item.Count },
                DataLabels = true,
                LabelPoint = chartPoint => $"{item.Status}: {chartPoint.Y}"
            });
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
