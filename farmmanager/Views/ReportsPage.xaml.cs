using DocumentFormat.OpenXml.Wordprocessing;
using farmmanager.ViewModels;
using farmmanager.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.Views
{
    public sealed partial class ReportsPage : Page
    {
        private readonly ReportsViewModel _vm = new();

        public ReportsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FromDatePicker.Date = new DateTimeOffset(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            ToDatePicker.Date = DateTimeOffset.Now;
            await _vm.InitializeAsync();
            UpdateUI();
        }

        private void UpdateUI()
        {
            WorkerSummaryList.ItemsSource = _vm.WorkerSummaries;
            ParcelSummaryList.ItemsSource = _vm.ParcelSummaries;
            ActivitySummaryList.ItemsSource = _vm.ActivitySummaries;

            TotalText.Text = _vm.GrandTotal.ToString("N0");
            DaysText.Text = _vm.TotalEntries.ToString();
            WorkersActiveText.Text = _vm.TotalWorkers.ToString();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _vm.FromDate = FromDatePicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.ToDate = ToDatePicker.Date?.DateTime ?? DateTime.Today;
            SetBusy(true);
            await _vm.RefreshCommand.ExecuteAsync(null!);
            UpdateUI();
            ShowStatus(_vm.StatusMessage);
            SetBusy(false);
        }

        private async void ExportWorkers_Click(object sender, RoutedEventArgs e)
        {
            _vm.FromDate = FromDatePicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.ToDate = ToDatePicker.Date?.DateTime ?? DateTime.Today;
            SetBusy(true);
            await _vm.ExportWorkerReportCommand.ExecuteAsync(null!);
            ShowStatus(_vm.StatusMessage);
            SetBusy(false);
        }

        private async void ExportParcels_Click(object sender, RoutedEventArgs e)
        {
            _vm.FromDate = FromDatePicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.ToDate = ToDatePicker.Date?.DateTime ?? DateTime.Today;
            SetBusy(true);
            await _vm.ExportParcelReportCommand.ExecuteAsync(null!);
            ShowStatus(_vm.StatusMessage);
            SetBusy(false);
        }

        private async void ExportActivities_Click(object sender, RoutedEventArgs e)
        {
            _vm.FromDate = FromDatePicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.ToDate = ToDatePicker.Date?.DateTime ?? DateTime.Today;
            SetBusy(true);
            await _vm.ExportActivityReportCommand.ExecuteAsync(null!);
            ShowStatus(_vm.StatusMessage);
            SetBusy(false);
        }

        private async void ExportFull_Click(object sender, RoutedEventArgs e)
        {
            _vm.FromDate = FromDatePicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.ToDate = ToDatePicker.Date?.DateTime ?? DateTime.Today;
            SetBusy(true);
            await _vm.ExportFullReportCommand.ExecuteAsync(null!);
            ShowStatus(_vm.StatusMessage);
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            ProgressBar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowStatus(string message)
        {
            StatusText.Text = message;
            StatusText.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
