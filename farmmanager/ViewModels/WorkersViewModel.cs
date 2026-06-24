using farmmanager.Models;
using farmmanager.Services;
using farmmanager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace farmmanager.ViewModels
{
    public class WorkersViewModel : BaseViewModel
    {
        private readonly WorkerService _service = new();

        public ObservableCollection<Worker> Workers { get; } = new();

        private Worker? _selectedWorker;
        public Worker? SelectedWorker { get => _selectedWorker; set => SetField(ref _selectedWorker, value); }

        // Form
        private string _name = string.Empty;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private string _group = string.Empty;
        public string Group { get => _group; set => SetField(ref _group, value); }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set => SetField(ref _phone, value); }

        public bool IsEditing => SelectedWorker != null;

        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand DeactivateCommand { get; }
        public RelayCommand NewWorkerCommand { get; }

        public WorkersViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeactivateCommand = new AsyncRelayCommand(DeactivateAsync);
            NewWorkerCommand = new RelayCommand(StartNew);
        }

        public async Task InitializeAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            IsBusy = true;
            // Grab the UI dispatcher while we are still on the UI thread
            var currentDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            var workers = await _service.GetAllAsync();

            // Use the stored dispatcher to push updates back to the UI
            currentDispatcher?.TryEnqueue(() =>
            {
                Workers.Clear();
                foreach (var w in workers) Workers.Add(w);
            });
            IsBusy = false;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Group))
            {
                StatusMessage = "Name and Group are required."; return;
            }

            if (SelectedWorker != null)
            {
                SelectedWorker.Name = Name;
                SelectedWorker.Group = Group;
                SelectedWorker.PhoneNumber = Phone;
                await _service.UpdateAsync(SelectedWorker);
                StatusMessage = "Worker updated.";
            }
            else
            {
                await _service.CreateAsync(new Worker { Name = Name, Group = Group, PhoneNumber = Phone });
                StatusMessage = "Worker created.";
            }

            StartNew();
            await LoadAsync();
        }

        private async Task DeactivateAsync()
        {
            if (SelectedWorker == null) return;
            await _service.DeactivateAsync(SelectedWorker.Id);
            StartNew();
            await LoadAsync();
            StatusMessage = "Worker deactivated.";
        }

        public void SelectWorker(Worker w)
        {
            SelectedWorker = w;
            Name = w.Name;
            Group = w.Group;
            Phone = w.PhoneNumber;
            OnPropertyChanged(nameof(IsEditing));
        }

        private void StartNew()
        {
            SelectedWorker = null;
            Name = string.Empty;
            Group = string.Empty;
            Phone = string.Empty;
            OnPropertyChanged(nameof(IsEditing));
        }
    }

}
