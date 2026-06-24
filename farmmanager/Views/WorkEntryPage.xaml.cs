//using DocumentFormat.OpenXml.Wordprocessing;
using farmmanager.Models;
using farmmanager.ViewModels;
using farmmanager.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.Views
{
    public sealed partial class WorkEntryPage : Page
    {
        private readonly WorkEntryViewModel _vm = new();

        // 1. Keep track of the actual database objects chosen via suggestions
        private Worker? _selectedWorker;
        private PlantationParcel? _selectedParcel;
        private Activite? _selectedActivity;

        public WorkEntryPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // Ensure the View Model finishes database reads completely
                await _vm.InitializeAsync();

                // Bind the UI controls only after data collections exist in memory
                BindUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load work entry collections: {ex.Message}");
                StatusText.Text = "Error communicating with database services. Please reload page.";
            }
        }

        private void BindUI()
        {
            // Set default calendar tracking dates
            EntryDatePicker.Date = DateTimeOffset.Now;
            FilterFromPicker.Date = DateTimeOffset.Now.AddMonths(-1);
            FilterToPicker.Date = DateTimeOffset.Now;

            RefreshList();
        }

        private void RefreshList()
        {
            // Guard against UI thread execution mismatch if viewmodel collection reloaded
            EntriesListView.ItemsSource = null;
            EntriesListView.ItemsSource = _vm.Entries;
        }

        private void Amount_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            UpdateComputedAmount();
        }

        private void UpdateComputedAmount()
        {
            decimal attained = double.IsNaN(AttainedBox.Value) ? 0 : (decimal)AttainedBox.Value;
            decimal planned = double.IsNaN(PlannedBox.Value) ? 0 : (decimal)PlannedBox.Value;
            decimal dailyWage = double.IsNaN(DailyWageBox.Value) ? 0 : (decimal)DailyWageBox.Value;

            // FIXED: Using our tracking property instead of SelectedItem
            decimal unitRate = _selectedActivity?.UnitRate ?? 0;

            // Piece rate formula: uses override wage if provided, otherwise calculates based on tasks completed
            decimal computed = dailyWage > 0 ? dailyWage : (planned > 0 ? (attained / planned) * unitRate : 0);
            ComputedAmountText.Text = computed.ToString("N0") + " FCFA";
        }

        private async void SaveEntry_Click(object sender, RoutedEventArgs e)
        {
            // FIXED: Validating tracking fields instead of old .SelectedItem targets
            if (_selectedWorker == null || _selectedParcel == null || _selectedActivity == null)
            {
                StatusText.Text = "Validation Error: Please select a Worker, Parcel, and Activity from suggestions.";
                return;
            }

            _vm.EntryDate = EntryDatePicker.Date?.DateTime ?? DateTime.Today;
            _vm.SelectedWorker = _selectedWorker;
            _vm.SelectedParcel = _selectedParcel;
            _vm.SelectedActivity = _selectedActivity;
            _vm.ObjectiveOfDay = ObjectiveTxt.Text;
            _vm.ObjectivePlanned = double.IsNaN(PlannedBox.Value) ? 0 : (decimal)PlannedBox.Value;
            _vm.ObjectiveAttained = double.IsNaN(AttainedBox.Value) ? 0 : (decimal)AttainedBox.Value;
            _vm.DailyWage = double.IsNaN(DailyWageBox.Value) ? 0 : (decimal)DailyWageBox.Value;



            // 2. Execute Async Save Command cleanly
            await _vm.SaveCommand.ExecuteAsync(null!);

            // 3. Update text status from the viewmodel execution result
            StatusText.Text = _vm.StatusMessage;

            // 4. Safely clear fields and repaint UI lists with guaranteed data sequence
            ClearForm();
            RefreshList();
        }

        private async void DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Entry",
                Content = "Are you sure you want to delete this entry?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
                DefaultButton = ContentDialogButton.Close,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _vm.LoadCommand.ExecuteAsync(null!);
                CancelEdit();
                RefreshList();
            }
        }

        private async void Filter_Click(object sender, RoutedEventArgs e)
        {
            _vm.FilterFrom = FilterFromPicker.Date?.DateTime ?? DateTime.Today.AddMonths(-1);
            _vm.FilterTo = FilterToPicker.Date?.DateTime ?? DateTime.Today;
            await _vm.LoadCommand.ExecuteAsync(null!);
            RefreshList();
        }

        private void EntriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EntriesListView.SelectedItem is WorkEntry entry)
            {
                _vm.StartEdit(entry);
                PopulateForm(entry);
            }
        }

        private void PopulateForm(WorkEntry entry)
        {
            if (entry == null) return;

            FormTitle.Text = "Edit Work Entry";
            CancelEditBtn.Visibility = Visibility.Visible;
            DeleteBtn.Visibility = Visibility.Visible;

            EntryDatePicker.Date = new DateTimeOffset(entry.Date);

            // FIXED: Extract matching items out of database lists to update tracking states
            _selectedWorker = _vm.Workers?.FirstOrDefault(w => w.Id == entry.WorkerId);
            _selectedParcel = _vm.Parcels?.FirstOrDefault(p => p.Id == entry.ParcelId);
            _selectedActivity = _vm.Activities?.FirstOrDefault(a => a.Id == entry.ActivityId);

            // FIXED: Set text displays using strings, not full items
            WorkerCombo.Text = _selectedWorker?.Name ?? string.Empty;
            ParcelCombo.Text = _selectedParcel?.Code ?? string.Empty; // Using Code or Name depending on model fields
            ActivityCombo.Text = _selectedActivity?.Name ?? string.Empty;

            ObjectiveTxt.Text = entry.ObjectiveOfDay;
            PlannedBox.Value = (double)entry.ObjectivePlanned;
            AttainedBox.Value = (double)entry.ObjectiveAttained;
            DailyWageBox.Value = (double)entry.DailyWage;
            ComputedAmountText.Text = entry.AmountEarned.ToString("N0") + " FCFA";
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e) => CancelEdit();

        private void CancelEdit()
        {
            _vm.CancelEditCommand.Execute(null);
            EntriesListView.SelectedItem = null;
            ClearForm();
        }

        private void ClearForm()
        {
            FormTitle.Text = "New Work Entry";
            CancelEditBtn.Visibility = Visibility.Collapsed;
            DeleteBtn.Visibility = Visibility.Collapsed;
            EntryDatePicker.Date = DateTimeOffset.Now;

            // FIXED: Nullify tracker instances instead of looking for .SelectedItem properties
            _selectedWorker = null;
            _selectedParcel = null;
            _selectedActivity = null;

            // FIXED: Clear string fields on AutoSuggestBoxes safely
            WorkerCombo.Text = string.Empty;
            ParcelCombo.Text = string.Empty;
            ActivityCombo.Text = string.Empty;

            ObjectiveTxt.Text = string.Empty;
            PlannedBox.Value = 0;
            AttainedBox.Value = 0;
            DailyWageBox.Value = 0;
            ComputedAmountText.Text = "0 FCFA";
            StatusText.Text = string.Empty;
        }

        // --- AUTOSUGGESTBOX ROUTING EVENTS ---

        private void WorkerSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchTerm = sender.Text.ToLower();
                sender.ItemsSource = _vm.Workers?.Where(w => w.Name.ToLower().Contains(searchTerm)).ToList();
            }
        }

        private void WorkerSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Worker selectedWorker)
            {
                _selectedWorker = selectedWorker;
                sender.Text = selectedWorker.Name;
            }
        }

        private void ParcelSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchTerm = sender.Text.ToLower();
                // Assumed property fallback: Using '.Code' or '.Name' based on model config
                sender.ItemsSource = _vm.Parcels?.Where(p => p.Name.ToLower().Contains(searchTerm)).ToList();
            }
        }

        private void ParcelSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is PlantationParcel selectedParcel)
            {
                _selectedParcel = selectedParcel;
                sender.Text = selectedParcel.Code;
            }
        }

        private void ActivitySuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchTerm = sender.Text.ToLower();
                sender.ItemsSource = _vm.Activities?.Where(a => a.Name.ToLower().Contains(searchTerm)).ToList();
            }
        }

        private void ActivitySuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Activite selectedActivity)
            {
                _selectedActivity = selectedActivity;
                sender.Text = selectedActivity.Name;

                // Recalculate amount right away when activity suggestions are confirmed
                UpdateComputedAmount();
            }
        }
    }
}