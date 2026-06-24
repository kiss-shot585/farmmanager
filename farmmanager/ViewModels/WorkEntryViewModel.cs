using farmmanager.Models;
using farmmanager.Services;
using farmmanager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.ViewModels
{
    public class WorkEntryViewModel : BaseViewModel
    {
        private readonly WorkEntryService _entryService = new();
        private readonly WorkerService _workerService = new();
        private readonly ParcelService _parcelService = new();
        private readonly ActivityService _activityService = new();

        public ObservableCollection<WorkEntry> Entries { get; } = new();
        public ObservableCollection<Worker> Workers { get; } = new();
        public ObservableCollection<PlantationParcel> Parcels { get; } = new();
        public ObservableCollection<Activite> Activities { get; } = new();

        // ── Form fields ────────────────────────────────────────────────────────

        private DateTime _entryDate = DateTime.Today;
        public DateTime EntryDate { get => _entryDate; set => SetField(ref _entryDate, value); }

        private Worker? _selectedWorker;
        public Worker? SelectedWorker { get => _selectedWorker; set => SetField(ref _selectedWorker, value); }

        private PlantationParcel? _selectedParcel;
        public PlantationParcel? SelectedParcel { get => _selectedParcel; set => SetField(ref _selectedParcel, value); }

        private Activite? _selectedActivity;
        public Activite? SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                SetField(ref _selectedActivity, value);
                if (value != null && DailyWage == 0)
                    OnPropertyChanged(nameof(ComputedAmount));
            }
        }

        private string _objectiveOfDay = string.Empty;
        public string ObjectiveOfDay { get => _objectiveOfDay; set => SetField(ref _objectiveOfDay, value); }

        private decimal _objectivePlanned;
        public decimal ObjectivePlanned
        {
            get => _objectivePlanned;
            set { SetField(ref _objectivePlanned, value); OnPropertyChanged(nameof(ComputedAmount)); }
        }

        private decimal _objectiveAttained;
        public decimal ObjectiveAttained
        {
            get => _objectiveAttained;
            set { SetField(ref _objectiveAttained, value); OnPropertyChanged(nameof(ComputedAmount)); }
        }

        private decimal _dailyWage;
        public decimal DailyWage
        {
            get => _dailyWage;
            set { SetField(ref _dailyWage, value); OnPropertyChanged(nameof(ComputedAmount)); }
        }

        private string _notes = string.Empty;
        public string Notes { get => _notes; set => SetField(ref _notes, value); }

        private WorkEntry? _editingEntry;
        public WorkEntry? EditingEntry { get => _editingEntry; set { SetField(ref _editingEntry, value); OnPropertyChanged(nameof(IsEditing)); } }
        public bool IsEditing => EditingEntry != null;

        // ── Filters ────────────────────────────────────────────────────────────
        private DateTime _filterFrom = DateTime.Today.AddMonths(-1);
        public DateTime FilterFrom { get => _filterFrom; set => SetField(ref _filterFrom, value); }

        private DateTime _filterTo = DateTime.Today;
        public DateTime FilterTo { get => _filterTo; set => SetField(ref _filterTo, value); }

        private Worker? _filterWorker;
        public Worker? FilterWorker { get => _filterWorker; set => SetField(ref _filterWorker, value); }

        // ── Computed ───────────────────────────────────────────────────────────

        public decimal ComputedAmount => DailyWage > 0
            ? DailyWage
            : (SelectedActivity?.UnitRate ?? 0) * (ObjectiveAttained / ObjectivePlanned);

        public decimal TotalAmount => Entries.Sum(e => e.AmountEarned);

        // ── Commands ───────────────────────────────────────────────────────────

        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public WorkEntryViewModel()
        {
            SaveCommand = new AsyncRelayCommand(SaveEntryAsync);
            LoadCommand = new AsyncRelayCommand(LoadDataAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteEntryAsync);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        public async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                var workers = await _workerService.GetAllAsync();
                var parcels = await _parcelService.GetAllAsync();
                var activities = await _activityService.GetAllAsync();
                var entries = await _entryService.GetEntriesAsync(FilterFrom, FilterTo,
                    FilterWorker?.Id);

                Workers.Clear(); foreach (var w in workers) Workers.Add(w);
                Parcels.Clear(); foreach (var p in parcels) Parcels.Add(p);
                Activities.Clear(); foreach (var a in activities) Activities.Add(a);
                Entries.Clear(); foreach (var e in entries) Entries.Add(e);

                OnPropertyChanged(nameof(TotalAmount));
                StatusMessage = $"Loaded {entries.Count} entries";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task SaveEntryAsync()
        {
            if (SelectedWorker == null || SelectedParcel == null || SelectedActivity == null)
            {
                StatusMessage = "Please select Worker, Parcel, and Activity.";
                return;
            }

            IsBusy = true;
            try
            {
                if (EditingEntry != null)
                {
                    EditingEntry.Date = EntryDate;
                    EditingEntry.WorkerId = SelectedWorker.Id;
                    EditingEntry.ParcelId = SelectedParcel.Id;
                    EditingEntry.ActivityId = SelectedActivity.Id;
                    EditingEntry.ObjectiveOfDay = ObjectiveOfDay;
                    EditingEntry.ObjectivePlanned = ObjectivePlanned;
                    EditingEntry.ObjectiveAttained = ObjectiveAttained;
                    EditingEntry.DailyWage = DailyWage;
                    EditingEntry.Notes = Notes;
                    await _entryService.UpdateAsync(EditingEntry);
                    StatusMessage = "Entry updated successfully.";
                }
                else
                {
                    var entry = new WorkEntry
                    {
                        Date = EntryDate,
                        WorkerId = SelectedWorker.Id,
                        WorkerName = SelectedWorker.Name,
                        ParcelId = SelectedParcel.Id,
                        ParcelName = SelectedParcel.Name,
                        ActivityId = SelectedActivity.Id,
                        ActivityName = SelectedActivity.Name,
                        ObjectiveOfDay = ObjectiveOfDay,
                        ObjectivePlanned = ObjectivePlanned,
                        ObjectiveAttained = ObjectiveAttained,
                        DailyWage = DailyWage,
                        Notes = Notes
                    };
                    await _entryService.CreateAsync(entry);
                    StatusMessage = "Entry saved successfully.";
                }

                //CancelEdit();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save failed: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task DeleteEntryAsync()
        {
            if (EditingEntry == null) return;
            await _entryService.DeleteAsync(EditingEntry.Id);
            CancelEdit();
            await LoadDataAsync();
            StatusMessage = "Entry deleted.";
        }

        public void StartEdit(WorkEntry entry)
        {
            EditingEntry = entry;
            EntryDate = entry.Date;
            SelectedWorker = Workers.FirstOrDefault(w => w.Id == entry.WorkerId);
            SelectedParcel = Parcels.FirstOrDefault(p => p.Id == entry.ParcelId);
            SelectedActivity = Activities.FirstOrDefault(a => a.Id == entry.ActivityId);
            ObjectiveOfDay = entry.ObjectiveOfDay;
            ObjectivePlanned = entry.ObjectivePlanned;
            ObjectiveAttained = entry.ObjectiveAttained;
            DailyWage = entry.DailyWage;
            Notes = entry.Notes;
        }

        private void CancelEdit()
        {
            EditingEntry = null;
            EntryDate = DateTime.Today;
            SelectedWorker = null;
            SelectedParcel = null;
            SelectedActivity = null;
            ObjectiveOfDay = string.Empty;
            ObjectivePlanned = 0;
            ObjectiveAttained = 0;
            DailyWage = 0;
            Notes = string.Empty;
        }
    }

}
