using farmmanager.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.Views
{
    public sealed partial class DashboardPage : Page
    {
        // Services are declared cleanly
        private readonly WorkEntryService _entryService = new();
        private readonly WorkerService _workerService = new();
        private readonly ParcelService _parcelService = new();

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                // Protects against database-not-ready or missing table exceptions on initial boot
                System.Diagnostics.Debug.WriteLine($"Dashboard loading failed: {ex.Message}");
                SubtitleText.Text = "Error loading dashboard metrics. Please restart the application.";
            }
        }

        private async Task LoadDashboardAsync()
        {
            SubtitleText.Text = $"Today is {DateTime.Today:dddd, dd MMMM yyyy}";

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var today = DateTime.Today;

            // 1. Load basic counters safely
            var totalMonth = await _entryService.GetTotalSpentAsync(monthStart, today);
            TotalMonthText.Text = totalMonth.ToString("N0");

            var workers = await _workerService.GetAllAsync();
            if (workers != null)
            {
                WorkerCountText.Text = workers.Count.ToString();
                var groups = workers.Select(w => w.Group).Distinct().Count();
                WorkerGroupsText.Text = $"in {groups} group{(groups != 1 ? "s" : "")}";
            }

            var todayEntries = await _entryService.GetEntriesAsync(today, today);
            if (todayEntries != null)
            {
                TodayEntriesText.Text = todayEntries.Count.ToString();
            }

            var parcels = await _parcelService.GetAllAsync();
            if (parcels != null)
            {
                ParcelCountText.Text = parcels.Count.ToString();
            }

            // 2. Recent entries (last 15)
            var recent = await _entryService.GetEntriesAsync(monthStart, today);
            if (recent != null)
            {
                RecentEntriesList.ItemsSource = recent.Take(15).ToList();
            }

            // 3. Top workers this month
            var workerSummaries = await _entryService.GetWorkerSummariesAsync(monthStart, today);
            if (workerSummaries != null)
            {
                TopWorkersList.ItemsSource = workerSummaries
                    .OrderByDescending(w => w.TotalAmountEarned)
                    .Take(8)
                    .ToList();
            }
        }

        private void QuickAddEntry_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Navigate to Work Entry page safely using the current Frame container
            this.Frame.Navigate(typeof(WorkEntryPage));
        }
    }
}
