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
    public class ParcelsViewModel : BaseViewModel
    {
        private readonly ParcelService _service = new();
        public ObservableCollection<PlantationParcel> Parcels { get; } = new();

        private PlantationParcel? _selected;
        public PlantationParcel? Selected { get => _selected; set => SetField(ref _selected, value); }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private string _code = string.Empty;
        public string Code { get => _code; set => SetField(ref _code, value); }

        private string _description = string.Empty;
        public string Description { get => _description; set => SetField(ref _description, value); }

        private string _area = string.Empty;
        public string Area { get => _area; set => SetField(ref _area, value); }

        public bool IsEditing => Selected != null;

        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand DeactivateCommand { get; }
        public RelayCommand NewCommand { get; }

        public ParcelsViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeactivateCommand = new AsyncRelayCommand(DeactivateAsync);
            NewCommand = new RelayCommand(StartNew);
        }

        public async Task InitializeAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            IsBusy = true;
            var items = await _service.GetAllAsync();
            Parcels.Clear();
            foreach (var p in items) Parcels.Add(p);
            IsBusy = false;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name)) { StatusMessage = "Name is required."; return; }
            double.TryParse(Area, out double area);

            if (Selected != null)
            {
                Selected.Name = Name; Selected.Code = Code;
                Selected.Description = Description; Selected.AreaHectares = area;
                await _service.UpdateAsync(Selected);
            }
            else
            {
                await _service.CreateAsync(new PlantationParcel
                { Name = Name, Code = Code, Description = Description, AreaHectares = area });
            }
            StartNew(); await LoadAsync();
            StatusMessage = "Parcel saved.";
        }

        private async Task DeactivateAsync()
        {
            if (Selected == null) return;
            await _service.DeactivateAsync(Selected.Id);
            StartNew(); await LoadAsync();
            StatusMessage = "Parcel deactivated.";
        }

        public void SelectParcel(PlantationParcel p)
        {
            Selected = p; Name = p.Name; Code = p.Code;
            Description = p.Description; Area = p.AreaHectares.ToString("F2");
            OnPropertyChanged(nameof(IsEditing));
        }

        private void StartNew()
        {
            Selected = null; Name = string.Empty; Code = string.Empty;
            Description = string.Empty; Area = string.Empty;
            OnPropertyChanged(nameof(IsEditing));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    public class ActivitiesViewModel : BaseViewModel
    {
        private readonly ActivityService _service = new();
        public ObservableCollection<Activite> Activities { get; } = new();

        private Activite? _selected;
        public Activite? Selected { get => _selected; set => SetField(ref _selected, value); }

        private string _name = string.Empty;
        public string Name { get => _name; set => SetField(ref _name, value); }

        private string _description = string.Empty;
        public string Description { get => _description; set => SetField(ref _description, value); }

        private string _unitRate = string.Empty;
        public string UnitRate { get => _unitRate; set => SetField(ref _unitRate, value); }

        private string _unit = "day";
        public string Unit { get => _unit; set => SetField(ref _unit, value); }

        public bool IsEditing => Selected != null;

        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public AsyncRelayCommand DeactivateCommand { get; }
        public RelayCommand NewCommand { get; }

        public ActivitiesViewModel()
        {
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            DeactivateCommand = new AsyncRelayCommand(DeactivateAsync);
            NewCommand = new RelayCommand(StartNew);
        }

        public async Task InitializeAsync() => await LoadAsync();

        private async Task LoadAsync()
        {
            IsBusy = true;
            var items = await _service.GetAllAsync();
            Activities.Clear();
            foreach (var a in items) Activities.Add(a);
            IsBusy = false;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name)) { StatusMessage = "Name is required."; return; }
            decimal.TryParse(UnitRate, out decimal rate);

            if (Selected != null)
            {
                Selected.Name = Name; Selected.Description = Description;
                Selected.UnitRate = rate; Selected.Unit = Unit;
                await _service.UpdateAsync(Selected);
            }
            else
            {
                await _service.CreateAsync(new Activite
                { Name = Name, Description = Description, UnitRate = rate, Unit = Unit });
            }
            StartNew(); await LoadAsync();
            StatusMessage = "Activity saved.";
        }

        private async Task DeactivateAsync()
        {
            if (Selected == null) return;
            await _service.DeactivateAsync(Selected.Id);
            StartNew(); await LoadAsync();
        }

        public void SelectActivity(Activite a)
        {
            Selected = a; Name = a.Name; Description = a.Description;
            UnitRate = a.UnitRate.ToString("F2"); Unit = a.Unit;
            OnPropertyChanged(nameof(IsEditing));
        }

        private void StartNew()
        {
            Selected = null; Name = string.Empty; Description = string.Empty;
            UnitRate = string.Empty; Unit = "day";
            OnPropertyChanged(nameof(IsEditing));
        }
    }
}
