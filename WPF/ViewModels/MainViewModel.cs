using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IPatientRepository _patientRepository;

        private List<Patient> _patients = new();
        private Patient _selectedPatient;
        private bool _isLeftPanelVisible;
        private bool _isRightPanelVisible;
        private string _searchText = "";
        private string _statusMessage = "Ready";

        public List<Patient> Patients
        {
            get => _patients;
            set { _patients = value; OnPropertyChanged(); }
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
                if (value != null)
                {
                    IsRightPanelVisible = true;
                }
            }
        }

        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set { _isLeftPanelVisible = value; OnPropertyChanged(); }
        }

        public bool IsRightPanelVisible
        {
            get => _isRightPanelVisible;
            set { _isRightPanelVisible = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterPatients(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public List<Patient> FilteredPatients =>
            string.IsNullOrWhiteSpace(SearchText)
                ? Patients
                : Patients.Where(p =>
                    (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.PhoneNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();

        public MainViewModel(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
            IsLeftPanelVisible = false;
            IsRightPanelVisible = false;
        }

        public async Task LoadPatientsAsync()
        {
            try
            {
                StatusMessage = "Loading patients...";
                Patients = (await _patientRepository.GetAllAsync()).ToList();
                StatusMessage = $"Loaded {Patients.Count} patients";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading patients: {ex.Message}";
            }
        }

        private void FilterPatients()
        {
            OnPropertyChanged(nameof(FilteredPatients));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}