using Core.DTOs;
using Core.Helpers;
using Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.Configuration;
using WPF.ViewModels;

namespace WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly AppSettings _settings;
        private string _authToken = "";
        private List<PatientViewModel> _allPatients = new();

        private bool _isLeftVisible, _isRightVisible;
        public bool IsLeftPanelVisible
        {
            get => _isLeftVisible;
            set { _isLeftVisible = value; OnPropertyChanged(); UpdatePanel(LeftPanelBorder, value); }
        }
        public bool IsRightPanelVisible
        {
            get => _isRightVisible;
            set { _isRightVisible = value; OnPropertyChanged(); UpdatePanel(RightPanelBorder, value); }
        }

        public MainWindow(IUserService userService, AppSettings settings)
        {
            InitializeComponent();
            _userService = userService;
            _settings = settings;
            SetupDefaults();
        }

        private void SetupDefaults()
        {
            TxtApiUrl.Text = _settings.ApiBaseUrl ?? "";
            TxtUsername.Text = _settings.DefaultUser ?? "";
            TxtPassword.Password = _settings.DefaultPassword ?? "";
            IsLeftPanelVisible = false;
            IsRightPanelVisible = false;
        }

        /* ------------------  EVENT HANDLERS  ------------------ */
        private async void LoginButton_Click(object sender, RoutedEventArgs e) =>
            await LoginAsync();

        private async void RefreshPatientsButton_Click(object sender, RoutedEventArgs e) =>
            await LoadPatientsAsync();

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e) =>
            await SaveVisitAsync();

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e) =>
            await RegisterPatientAsync();

        private void ToggleLeftPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsLeftPanelVisible = !IsLeftPanelVisible;

        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsRightPanelVisible = !IsRightPanelVisible;

        private void CloseLeftPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsLeftPanelVisible = false;

        private void CloseRightPanelButton_Click(object sender, RoutedEventArgs e) =>
            IsRightPanelVisible = false;

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e) =>
            UpdatePatientList();

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // optional scroll tweak
        }
        /* ---- Missing handlers wired by XAML ---- */
        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: ping API & show result
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: load visits / update detail pane
        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e) =>
            IsLeftPanelVisible = true;

        private void CancelRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ClearRegistrationForm();
            IsLeftPanelVisible = false;
        }

        /* ------------------  BUSINESS WORKFLOWS  ------------------ */
        private async Task LoginAsync()
        {
            try
            {
                _authToken = await _userService.LoginAsync(TxtUsername.Text, TxtPassword.Password, _settings.ApiBaseUrl);
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPatientsAsync()
        {
            try
            {
                var dtos = await _userService.GetPatientsAsync(_settings.ApiBaseUrl, _authToken);
                _allPatients = dtos.Select(d => new PatientViewModel
                {
                    PatientId = d.PatientId,
                    Name = d.Name,
                    Sex = d.Sex,
                    DateOfBirth = d.DateOfBirth,
                    PhoneNumber = d.PhoneNumber,
                    Address = d.Address
                }).ToList();
                UpdatePatientList();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
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
                var patient = new PatientCreateDto
                {
                    Name = TxtPatientName.Text,
                    Sex = CmbPatientGender.SelectedItem?.ToString() ?? "Female",
                    DateOfBirth = DateOnly.FromDateTime(DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30)),
                    PhoneNumber = TxtPatientContact.Text,
                    Address = TxtPatientAddress.Text,
                    BloodGroup = TxtBloodGroup.Text,
                    Allergies = TxtAllergies.Text
                };

                await _userService.CreatePatientAsync(patient, _settings.ApiBaseUrl, _authToken);
                MessageBox.Show("Patient registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearRegistrationForm();
                await LoadPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var visit = new VisitDto
                {
                    PatientId = (PatientListBox.SelectedItem as PatientViewModel)?.PatientId ?? 0,
                    DateOfVisit = DateTime.UtcNow,
                    Diagnosis = TxtDiagnosis.Text,
                    Notes = TxtNotes.Text
                };
                await _userService.SaveVisitAsync(visit, _settings.ApiBaseUrl, _authToken);
                MessageBox.Show("Visit saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearVisitForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /* ------------------  UI HELPERS  ------------------ */
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
            var search = TxtPatientSearch.Text?.ToLower() ?? "";
            var filtered = _allPatients.Where(p =>
                string.IsNullOrWhiteSpace(search) ||
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.PhoneNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) == true).ToList();
            PatientListBox.ItemsSource = filtered.OrderBy(p => p.Name);
        }

        private void UpdatePanel(Border panel, bool visible)
        {
            if (panel != null) panel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /* ------------------  PROPERTY CHANGED  ------------------ */
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}