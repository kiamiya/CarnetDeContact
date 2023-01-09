using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using CarnetDeContact.Models;
using CarnetDeContact.Repo;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CarnetDeContact.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Contacts = new ObservableCollection<Contact>();
            IsRunning = false;
            StatusLabel = "Starting...";
            EnableBtnLabel = "";
            LoadingVisibility = Visibility.Hidden;

            DeleteCommand = new RelayCommand(
                (o) => Delete(),
                (o) => !IsRunning && SelectedContact != null);

            SaveCommand = new RelayCommand(
            (o) => SaveChanges(),
            (o) => !IsRunning &&

                        (Contacts.Any(p => p.HasPendingDelete)
                            || Contacts.Any(p => p.IsNew)
                            || Contacts.Any(p => p.HasPendingChanges)));

            EnableCommand = new RelayCommand(
                (o) => EnableOrDisable(),
                (o) => !IsRunning && SelectedContact != null);

            LoadContacts();
        }
        #region Properties and fields

        private APIClient _apiClient = new APIClient();

        public ObservableCollection<Contact> Contacts { get => _contacts; set { _contacts = value; OnPropertyChanged(); } }
        private ObservableCollection<Contact> _contacts;

        public Contact SelectedContact
        {
            get => _selectedContact;
            set
            {
                _selectedContact = value;
                OnPropertyChanged();

                if (_selectedContact != null)
                    EnableBtnLabel = _selectedContact.IsEnabled ? "Disable" : "Enable";
            }
        }
        private Contact _selectedContact;

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();

                LoadingVisibility = value == true ? Visibility.Visible : Visibility.Hidden;

                //if (value == true)
                //    LoadingVisibility = Visibility.Visible;
                //else
                //    LoadingVisibility = Visibility.Hidden;
            }
        }
        private bool _isRunning { get; set; }

        public Visibility LoadingVisibility { get => _loadingVisibility; set { _loadingVisibility = value; OnPropertyChanged(); } }
        private Visibility _loadingVisibility { get; set; }

        public string StatusLabel { get => _statusLabel; set { _statusLabel = value; OnPropertyChanged(); } }
        private string _statusLabel { get; set; }

        public string PendingLabel { get => _pendingLabel; set { _pendingLabel = value; OnPropertyChanged(); } }
        private string _pendingLabel { get; set; }

        public string EnableBtnLabel { get => _enableBtnLabel; set { _enableBtnLabel = value; OnPropertyChanged(); } }
        private string _enableBtnLabel { get; set; }

        //INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Commands
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand EnableCommand { get; set; }

        public async Task EnableOrDisable()
        {
            SelectedContact.SetStatus(!SelectedContact.IsEnabled);
            EnableBtnLabel = _selectedContact.IsEnabled ? "Disable" : "Enable";

            UpdatePendingLabel();
        }

        public async Task Delete()
        {
            if (!SelectedContact.IsNew)
                SelectedContact.SetForDeletion();
            else
                Contacts.Remove(SelectedContact);

            UpdatePendingLabel();
        }

        public async Task SaveChanges()
        {
            try
            {
                IsRunning = true;

                var confirmMsg = GetPendingChangesLabel();

                var confirmation = MessageBox.Show($"Are you sure?\r\n{confirmMsg}",
                   "Commit?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmation == MessageBoxResult.Yes)
                {
                    StatusLabel = "Saving...";
                    PendingLabel = "";

                    await Task.Run(async () =>
                    {
                        var newContacts = GetPendingNewContacts();
                        var updatedContacts = GetPendingUpdatedContacts();
                        var deletedContacts = GetPendingDeletedContacts();

                        //add
                        foreach (var newContact in newContacts)
                        {
                            await _apiClient.AddContactAsync(newContact);
                        }

                        //delete
                        foreach (var deletedContact in deletedContacts)
                        {
                            await _apiClient.DeleteContactAsync(deletedContact);
                        }

                        //update               
                        foreach (var updatedContact in updatedContacts)
                        {
                            await _apiClient.UpdateContactAsync(updatedContact);
                        }

                        Thread.Sleep(2000);
                    });
                }

                //refresh
                await LoadContacts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong while saving changes : {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRunning = false;
                StatusLabel = "";
            }
        }

        #endregion

        #region Helper Methods
        public async Task LoadContacts()
        {
            try
            {
                IsRunning = true;
                StatusLabel = "Loading contacts...";

                await Task.Run(async () =>
                {
                    Thread.Sleep(5000);
                    var p = await _apiClient.GetContactsAsync();
                    Contacts = new ObservableCollection<Contact>(p);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Something went wrong while loading contacts : {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRunning = false;
                StatusLabel = "";
            }
        }

        public void UpdatePendingLabel()
        {
            PendingLabel = GetPendingChangesLabel();
        }

        private string GetPendingChangesLabel()
        {
            return
                $"Pending Changes :" +
                $" ({GetPendingNewContacts().Count}) New " +
                $" | ({GetPendingDeletedContacts().Count}) Deleted" +
                $" | ({GetPendingUpdatedContacts().Count}) Updated";
        }

        private List<Contact> GetPendingNewContacts()
        {
            return Contacts.Where(p => p.IsNew).ToList();
        }

        private List<Contact> GetPendingUpdatedContacts()
        {
            return Contacts.Where(p => p.HasPendingChanges).ToList();
        }

        private List<Contact> GetPendingDeletedContacts()
        {
            return Contacts.Where(p => p.HasPendingDelete).ToList();
        }

        #endregion
    }
}
