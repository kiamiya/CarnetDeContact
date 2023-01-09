using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CarnetDeContact.Models
{
    internal class Contact : INotifyPropertyChanged
    {
        public Contact()
        {
            StatusLabel = "New";
            IsEnabled = true;
        }
        public int Id { get; set; }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;

                if (!IsNew)  //make sure we don't count a new record as an updated one
                    HasPendingChanges = true;

                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;

                if (!IsNew)  //make sure we don't count a new record as an updated one
                    HasPendingChanges = true;

                OnPropertyChanged();
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;

                if (!IsNew)  //make sure we don't count a new record as an updated one
                    HasPendingChanges = true;

                OnPropertyChanged();
            }
        }
        
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                if (!IsNew)  //make sure we don't count a new record as an updated one
                    HasPendingChanges = true;

                OnPropertyChanged();
            }
        }

        private string _statusLabel;
        public string StatusLabel
        {
            get => _statusLabel;
            set
            {
                _statusLabel = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                HasPendingChanges = true;

                OnPropertyChanged();
            }
        }

        //used for status tracking
        public bool HasPendingChanges = false;
        public bool HasPendingDelete = false;
        public bool IsNew { get => Id == 0; }

        private void UpdateStatusLabel()
        {
            if (IsNew)
                StatusLabel = "New";
            else if (HasPendingChanges)
                StatusLabel = "Updated";
            else if (HasPendingDelete)
                StatusLabel = "Deleted";
            else
                StatusLabel = "No Change";
        }

        public void SetStatus(bool s)
        {
            IsEnabled = s;
            UpdateStatusLabel();
        }

        public void SetForDeletion()
        {
            HasPendingDelete = true;
            UpdateStatusLabel();
        }

        public void SetPendingChanges(bool value)
        {
            HasPendingChanges = value;
            UpdateStatusLabel();
        }

        public void SetInitialLoadValue()
        {
            HasPendingChanges = false;
            HasPendingDelete = false;
            UpdateStatusLabel();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            //force the status to update
            if (name != "StatusLabel")
                UpdateStatusLabel();
        }
    }
}
