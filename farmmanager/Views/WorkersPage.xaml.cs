using DocumentFormat.OpenXml.Wordprocessing;
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
    public sealed partial class WorkersPage : Page
    {
        private readonly WorkersViewModel _vm = new();

        public WorkersPage() { this.InitializeComponent(); }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _vm.InitializeAsync();
            WorkersList.ItemsSource = _vm.Workers;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Name = NameTxt.Text;
            _vm.Group = GroupTxt.Text;
            _vm.Phone = PhoneTxt.Text;
            await Task.Run(() => _vm.SaveCommand.Execute(null));
            await Task.Delay(200);
            WorkersList.ItemsSource = null;
            WorkersList.ItemsSource = _vm.Workers;
            StatusTxt.Text = _vm.StatusMessage;
            ClearForm();
        }

        private async void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Deactivate Worker",
                Content = $"Deactivate {_vm.SelectedWorker?.Name}? They won't appear in new entries.",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await Task.Run(() => _vm.DeactivateCommand.Execute(null));
                await Task.Delay(200);
                WorkersList.ItemsSource = null;
                WorkersList.ItemsSource = _vm.Workers;
                ClearForm();
            }
        }

        private void Workers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkersList.SelectedItem is Worker w)
            {
                _vm.SelectWorker(w);
                NameTxt.Text = w.Name;
                GroupTxt.Text = w.Group;
                PhoneTxt.Text = w.PhoneNumber;
                DeactivateBtn.Visibility = Visibility.Visible;
            }
        }

        private void NewWorker_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            _vm.NewWorkerCommand.Execute(null);
            WorkersList.SelectedItem = null;
            NameTxt.Text = GroupTxt.Text = PhoneTxt.Text = string.Empty;
            DeactivateBtn.Visibility = Visibility.Collapsed;
        }
    }

}
