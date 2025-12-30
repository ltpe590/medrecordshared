using Core.DTOs;
using Core.Services;
using Domain.Models;
using Infrastructure.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WPF.Configuration;
using WPF.Helpers;
using WPF.ViewModels;

namespace WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ✅ UI-Only Dependencies
        private readonly MainViewModel _viewModel;
        private readonly IUserService _userService;
        private readonly IApiConnectionProvider _api;
        private readonly AppSettings _settings;

        // ✅ UI State
        private string _authToken = "";
        private string _currentUserId = "";
        private List<PatientViewModel> _allPatients = new();

        public bool IsLeftPanelVisible { get; set; }
        public bool IsRightPanelVisible { get; set; }

        // ✅ DI Constructor - NO business logic!
        public MainWindow(
            MainViewModel viewModel,
            IUserService userService,
            IApiConnectionProvider api,
            AppSettings settings)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _userService = userService;
            _api = api;
            _settings = settings;

            DataContext = _viewModel;
            SetupDefaults();
            SetupEvents();
        }

        /* =========================================================
         * 1. UI SETUP (NO BUSINESS LOGIC)
         * =======================================================*/
        private void SetupDefaults()
        {
            TxtApiUrl.Text = _settings.ApiBaseUrl;
            TxtUsername.Text = _settings.DefaultUser;
            TxtPassword.Password = _settings.DefaultPassword;
            IsLeftPanelVisible = false;
            IsRightPanelVisible = false;
        }

        private void SetupEvents() => _viewModel.PropertyChanged += OnSelectedPatientChanged;

        /* =========================================================
         * 2. UI EVENT HANDLERS (1-LINERS)
         * =======================================================*/
        // ✅ All business logic moved to services
        private async void LoginButton_Click(object sender, RoutedEventArgs e) =>
            await LoginAsync();

        private async void RefreshPatientsButton_Click(object sender, RoutedEventArgs e) =>
            await RefreshPatientsAsync();

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e) =>
            await SaveVisitAsync();

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e) =>
            await RegisterPatientAsync();

        // ✅ UI-only navigation
        private void ToggleLeftPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsLeftPanelVisible = !IsLeftPanelVisible;

        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsRightPanelVisible = !IsRightPanelVisible;

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _viewModel.SelectedPatient = PatientListBox.SelectedItem as Patient;

        /* =========================================================
         * 3. BUSINESS WORKFLOWS (MOVED TO SERVICES)
         * =======================================================*/
        private async Task LoginAsync()
        {
            try
            {
                _authToken = await _userService.LoginAsync(
                    TxtUsername.Text,
                    TxtPassword.Password,
                    _settings.ApiBaseUrl);

                _api.SetAuthToken(_authToken);
                LoginExpander.IsExpanded = false;
                MessageBox.Show($"Welcome, {TxtUsername.Text}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshPatientsAsync()
        {
            try
            {
                var patients = await _userService.GetPatientsAsync(_settings.ApiBaseUrl, _authToken);
                _allPatients = patients;
                UpdatePatientList();
                StatusText.Text = $"Loaded {patients.Count} patients";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
            ";
            }
        }

        private async Task SaveVisitAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var visit = new VisitDto(
                    PatientId: _viewModel.SelectedPatient?.PatientId ?? 0,
                    Diagnosis: TxtDiagnosis.Text,
                    Notes: TxtNotes.Text,
                    CreatedAt: DateTime.Now,
                    DoctorId: _currentUserId);

                await _userService.SaveVisitAsync(visit, _settings.ApiBaseUrl, _authToken);
                MessageBox.Show("Visit saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearVisitForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RegisterPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtPatientName.Text))
            {
                MessageBox.Show("Please enter patient name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var patient = new PatientCreateDto(
                    Name: TxtPatientName.Text,
                    Sex: CmbPatientGender.SelectedItem?.ToString() ?? "Male",
                    DateOfBirth: DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30),
                    PhoneNumber: TxtPatientContact.Text,
                    Address: TxtPatientAddress.Text,
                    BloodGroup: TxtBloodGroup.Text,
                    Allergies: TxtAllergies.Text);

                await _userService.CreatePatientAsync(patient, _settings.ApiBaseUrl, _authToken);
                MessageBox.Show("Patient registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearRegistrationForm();
                await RefreshPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /* =========================================================
         * 4. UI HELPERS (NO BUSINESS LOGIC)
         * =======================================================*/
        private void ClearRegistrationForm()
        {
            TxtPatientName.Clear();
            DatePatientBirth.SelectedDate = null;
            CmbPatientGender.SelectedIndex = 0;
            TxtPatientContact.Clear();
            TxtPatientAddress.Clear();
            TxtBloodGroup.Clear();
            TxtAllergies.Clear();
        }

        private void ClearVisitForm()
        {
            TxtDiagnosis.Clear();
            TxtNotes.Clear();
        }

        private void UpdatePatientList()
        {
            var searchText = TxtPatientSearch.Text?.ToLower() ?? "";
            var filtered = _allPatients.Where(p =>
                string.IsNullOrWhiteSpace(searchText) ||
                p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                p.ContactNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            PatientListBox.ItemsSource = filtered.OrderBy(p => p.Name);
        }

        private void OnSelectedPatientChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedPatient) && _viewModel.SelectedPatient != null)
            {
                IsRightPanelVisible = true;
                UpdateSelectedPatientInfo(_viewModel.SelectedPatient);
            }
        }

        private void UpdateSelectedPatientInfo(Patient patient)
        {
            if (patient == null) return;

            SelectedPatientBorder.Visibility = Visibility.Visible;
            SelectedPatientInfo.Text = patient.Name;
            SelectedPatientDetails.Text =
                $"Age: {AgeCalculator.Calculate(patient.DateOfBirth)} | Gender: {patient.Sex} | Contact: {patient.PhoneNumber}";
        }

        private void UpdatePanelVisibility()
        {
            if (FindName("LeftPanelBorder") is UIElement left) left.Visibility = IsLeftPanelVisible ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("RightPanelBorder") is UIElement right) right.Visibility = IsRightPanelVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        /* =========================================================
         * 5. PROPERTY CHANGED
         * =======================================================*/
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}