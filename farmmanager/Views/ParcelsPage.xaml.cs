using farmmanager.Models;
using farmmanager.ViewModels;
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
    public sealed partial class ParcelsPage : Page
    {
        private readonly ParcelsViewModel _vm = new();

        public ParcelsPage() { this.InitializeComponent(); }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _vm.InitializeAsync();
            ParcelsList.ItemsSource = _vm.Parcels;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Name = NameTxt.Text;
            _vm.Code = CodeTxt.Text;
            _vm.Area = double.IsNaN(AreaBox.Value) ? "0" : AreaBox.Value.ToString();
            _vm.Description = DescTxt.Text;
            _vm.SaveCommand.Execute(null);
            await Task.Delay(300);
            ParcelsList.ItemsSource = null;
            ParcelsList.ItemsSource = _vm.Parcels;
            StatusTxt.Text = _vm.StatusMessage;
            ClearForm();
        }

        private async void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Deactivate Parcel",
                Content = $"Deactivate parcel '{_vm.Selected?.Name}'?",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _vm.DeactivateCommand.Execute(null);
                await Task.Delay(300);
                ParcelsList.ItemsSource = null;
                ParcelsList.ItemsSource = _vm.Parcels;
                ClearForm();
            }
        }

        private void Parcels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParcelsList.SelectedItem is PlantationParcel p)
            {
                _vm.SelectParcel(p);
                NameTxt.Text = p.Name;
                CodeTxt.Text = p.Code;
                AreaBox.Value = p.AreaHectares;
                DescTxt.Text = p.Description;
                DeactivateBtn.Visibility = Visibility.Visible;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            _vm.NewCommand.Execute(null);
            ParcelsList.SelectedItem = null;
            NameTxt.Text = CodeTxt.Text = DescTxt.Text = string.Empty;
            AreaBox.Value = 0;
            DeactivateBtn.Visibility = Visibility.Collapsed;
        }
    }
}
