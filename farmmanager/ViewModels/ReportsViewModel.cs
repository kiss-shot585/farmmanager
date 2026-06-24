using farmmanager.Reports;
using farmmanager.Services;
using farmmanager.Helpers;
using System.IO;    
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly WorkEntryService _entryService = new();
        private readonly ExcelReportService _reportService = new();

        public ObservableCollection<WorkerSummary> WorkerSummaries { get; } = new();
        public ObservableCollection<ParcelSummary> ParcelSummaries { get; } = new();
        public ObservableCollection<ActivitySummary> ActivitySummaries { get; } = new();

        private DateTime _fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        public DateTime FromDate { get => _fromDate; set { SetField(ref _fromDate, value); } }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate { get => _toDate; set { SetField(ref _toDate, value); } }

        private decimal _grandTotal;
        public decimal GrandTotal { get => _grandTotal; set => SetField(ref _grandTotal, value); }

        private int _totalEntries;
        public int TotalEntries { get => _totalEntries; set => SetField(ref _totalEntries, value); }

        private int _totalWorkers;
        public int TotalWorkers { get => _totalWorkers; set => SetField(ref _totalWorkers, value); }

        public AsyncRelayCommand RefreshCommand { get; }
        public AsyncRelayCommand ExportWorkerReportCommand { get; }
        public AsyncRelayCommand ExportParcelReportCommand { get; }
        public AsyncRelayCommand ExportActivityReportCommand { get; }
        public AsyncRelayCommand ExportFullReportCommand { get; }

        public ReportsViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadSummariesAsync);
            ExportWorkerReportCommand = new AsyncRelayCommand(ExportWorkerReportAsync);
            ExportParcelReportCommand = new AsyncRelayCommand(ExportParcelReportAsync);
            ExportActivityReportCommand = new AsyncRelayCommand(ExportActivityReportAsync);
            ExportFullReportCommand = new AsyncRelayCommand(ExportFullReportAsync);
        }

        public async Task InitializeAsync() => await LoadSummariesAsync();

        private async Task LoadSummariesAsync()
        {
            IsBusy = true;
            try
            {
                var workers = await _entryService.GetWorkerSummariesAsync(FromDate, ToDate);
                var parcels = await _entryService.GetParcelSummariesAsync(FromDate, ToDate);
                var activities = await _entryService.GetActivitySummariesAsync(FromDate, ToDate);

                WorkerSummaries.Clear(); foreach (var w in workers) WorkerSummaries.Add(w);
                ParcelSummaries.Clear(); foreach (var p in parcels) ParcelSummaries.Add(p);
                ActivitySummaries.Clear(); foreach (var a in activities) ActivitySummaries.Add(a);

                GrandTotal = workers.Sum(w => w.TotalAmountEarned);
                TotalEntries = workers.Sum(w => w.TotalDays);
                TotalWorkers = workers.Count;

                StatusMessage = $"Summary for {FromDate:dd MMM} – {ToDate:dd MMM yyyy}";
            }
            catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        private string GetSavePath(string prefix)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filename = $"{prefix}_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
            return Path.Combine(desktop, filename);
        }

        private async Task ExportWorkerReportAsync()
        {
            IsBusy = true;
            try
            {
                var entries = await _entryService.GetEntriesAsync(FromDate, ToDate);
                var summaries = await _entryService.GetWorkerSummariesAsync(FromDate, ToDate);
                var path = GetSavePath("WorkerReport");
                await _reportService.GenerateWorkerSummaryReportAsync(entries, summaries, FromDate, ToDate, path);
                StatusMessage = $"Worker report saved: {Path.GetFileName(path)}";
            }
            catch (Exception ex) { StatusMessage = $"Export failed: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        private async Task ExportParcelReportAsync()
        {
            IsBusy = true;
            try
            {
                var summaries = await _entryService.GetParcelSummariesAsync(FromDate, ToDate);
                var path = GetSavePath("ParcelReport");
                _reportService.GenerateParcelSummaryReport(summaries, FromDate, ToDate, path);
                StatusMessage = $"Parcel report saved: {Path.GetFileName(path)}";
            }
            catch (Exception ex) { StatusMessage = $"Export failed: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        private async Task ExportActivityReportAsync()
        {
            IsBusy = true;
            try
            {
                var summaries = await _entryService.GetActivitySummariesAsync(FromDate, ToDate);
                var path = GetSavePath("ActivityReport");
                _reportService.GenerateActivitySummaryReport(summaries, FromDate, ToDate, path);
                StatusMessage = $"Activity report saved: {Path.GetFileName(path)}";
            }
            catch (Exception ex) { StatusMessage = $"Export failed: {ex.Message}"; }
            finally { IsBusy = false; }
        }

        private async Task ExportFullReportAsync()
        {
            IsBusy = true;
            try
            {
                var entries = await _entryService.GetEntriesAsync(FromDate, ToDate);
                var workers = await _entryService.GetWorkerSummariesAsync(FromDate, ToDate);
                var parcels = await _entryService.GetParcelSummariesAsync(FromDate, ToDate);
                var activities = await _entryService.GetActivitySummariesAsync(FromDate, ToDate);
                var path = GetSavePath("FullReport");
                await _reportService.GenerateFullReportAsync(workers, parcels, activities, entries, FromDate, ToDate, path);
                StatusMessage = $"Full report saved to Desktop: {Path.GetFileName(path)}";
            }
            catch (Exception ex) { StatusMessage = $"Export failed: {ex.Message}"; }
            finally { IsBusy = false; }
        }
    }

}
