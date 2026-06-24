using farmmanager.ViewModels;
using farmmanager.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace farmmanager.Views
{
    public sealed partial class ActivitiesPage : Page
    {
        private readonly ActivitiesViewModel _vm = new();

        public ActivitiesPage() { this.InitializeComponent(); }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _vm.InitializeAsync();
            ActivitiesList.ItemsSource = _vm.Activities;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Name = NameTxt.Text;
            _vm.Unit = (UnitCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "day";
            _vm.UnitRate = double.IsNaN(RateBox.Value) ? "0" : RateBox.Value.ToString();
            _vm.Description = DescTxt.Text;
            _vm.SaveCommand.Execute(null);
            await Task.Delay(300);
            ActivitiesList.ItemsSource = null;
            ActivitiesList.ItemsSource = _vm.Activities;
            StatusTxt.Text = _vm.StatusMessage;
            ClearForm();
        }

        private async void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Deactivate Activity",
                Content = $"Deactivate '{_vm.Selected?.Name}'?",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                _vm.DeactivateCommand.Execute(null);
                await Task.Delay(300);
                ActivitiesList.ItemsSource = null;
                ActivitiesList.ItemsSource = _vm.Activities;
                ClearForm();
            }
        }

        private void Activities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActivitiesList.SelectedItem is Activite a)
            {
                _vm.SelectActivity(a);
                NameTxt.Text = a.Name;
                DescTxt.Text = a.Description;
                RateBox.Value = (double)a.UnitRate;
                // Select matching unit in combo
                foreach (ComboBoxItem item in UnitCombo.Items)
                    if (item.Content?.ToString() == a.Unit) { UnitCombo.SelectedItem = item; break; }
                DeactivateBtn.Visibility = Visibility.Visible;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            _vm.NewCommand.Execute(null);
            ActivitiesList.SelectedItem = null;
            NameTxt.Text = DescTxt.Text = string.Empty;
            RateBox.Value = 0;
            UnitCombo.SelectedIndex = 0;
            DeactivateBtn.Visibility = Visibility.Collapsed;
        }
    }

}
