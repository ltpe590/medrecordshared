using Core.Interfaces.Repositories;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.Helpers;
using WPF.Models;
using WPF.Models.ViewModels;
using WPF.Services;

namespace WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly MainViewModel _viewModel;
        private readonly IPatientRepository _patientRepository;
        private readonly ApiService _apiService;
        private readonly PatientService _patientService;

        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private List<PatientViewModel> _allPatients = new();

        public bool IsLeftPanelVisible { get; set; }
        public bool IsRightPanelVisible { get; set; }

        public MainWindow(MainViewModel viewModel, IPatientRepository patientRepository)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _patientRepository = patientRepository;

            _apiService = new ApiService(new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
            _patientService = new PatientService(_patientRepository, _apiService);

            DataContext = this;
            SetupDefaults();
            SetupEvents();
            AttemptAutoLogin();
        }

        private void SetupDefaults()
        {
            TxtApiUrl.Text = "https://localhost:7287";
            TxtUsername.Text = "doctor1";
            TxtPassword.Password = "Password123!";
            IsLeftPanelVisible = false;
            IsRightPanelVisible = false;
        }

        private void SetupEvents() => _viewModel.PropertyChanged += OnSelectedPatientChanged;

        private async void AttemptAutoLogin()
        {
            await Task.Delay(500);
            await TestConnectionAsync();
            await PerformLoginAsync();
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                await _apiService.GetAsync<string>($"{TxtApiUrl.Text}/swagger");
            }
            catch
            {
                StatusText.Text = "API not reachable";
            }
        }

        // === EVENT HANDLERS (Added all missing ones) ===

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _apiService.GetAsync<string>($"{TxtApiUrl.Text}/");
                MessageBox.Show("API connection successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusText.Text = "Connected to API";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Connection error";
            }
        }

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdatePatientList();

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e) => IsLeftPanelVisible = true;

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e) => await SaveVisitAsync();

        private void CloseLeftPanelButton_Click(object sender, RoutedEventArgs e) => IsLeftPanelVisible = false;

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e) => await RegisterPatientAsync();

        private void CancelRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            IsLeftPanelVisible = false;
            ClearRegistrationForm();
        }

        private void CloseRightPanelButton_Click(object sender, RoutedEventArgs e) => IsRightPanelVisible = false;

        private async void LoginButton_Click(object sender, RoutedEventArgs e) => await PerformLoginAsync();

        private async Task PerformLoginAsync()
        {
            if (!ValidateLogin()) return;

            try
            {
                SetLoginUI(true);
                _baseUrl = TxtApiUrl.Text.Trim();
                var response = await _apiService.PostAsync<object, LoginResponse>(
                    $"{_baseUrl}/api/Auth/login",
                    new { Username = TxtUsername.Text, Password = TxtPassword.Password });

                _authToken = response.Token;
                _currentUserId = response.UserId;
                _apiService.SetAuthToken(_authToken);

                LoginExpander.IsExpanded = false;
                MessageBox.Show($"Welcome, {TxtUsername.Text}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoginUI(false);
            }
        }

        private bool ValidateLogin() =>
            !string.IsNullOrWhiteSpace(TxtUsername.Text) &&
            !string.IsNullOrWhiteSpace(TxtPassword.Password) &&
            !string.IsNullOrWhiteSpace(TxtApiUrl.Text);

        private void SetLoginUI(bool isLoggingIn)
        {
            LoginButton.IsEnabled = !isLoggingIn;
            LoginButton.Content = isLoggingIn ? "Logging in..." : "Login";
        }

        private async void RefreshPatientsButton_Click(object sender, RoutedEventArgs e) => await LoadPatientsAsync();

        private async Task LoadPatientsAsync()
        {
            try
            {
                _allPatients = await _patientService.LoadPatientsAsync();
            }
            catch
            {
                _allPatients = await _patientService.LoadPatientsFromApiAsync(_baseUrl);
            }
            UpdatePatientList();
            StatusText.Text = $"Loaded {_allPatients.Count} patients";
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

        // === ADDITIONAL HELPER METHODS ===

        private async Task SaveVisitAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var visitData = new Dictionary<string, object>
                {
                    ["PatientId"] = _viewModel.SelectedPatient?.PatientId ?? 0,
                    ["Diagnosis"] = TxtDiagnosis.Text,
                    ["Notes"] = TxtNotes.Text,
                    ["CreatedAt"] = DateTime.Now
                };

                if (!string.IsNullOrEmpty(_currentUserId))
                    visitData["DoctorId"] = _currentUserId;

                await _apiService.PostAsync<object, object>($"{_baseUrl}/api/Visits", visitData);
                MessageBox.Show("Visit saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                TxtDiagnosis.Clear();
                TxtNotes.Clear();
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
                var patientData = new Dictionary<string, object>
                {
                    ["Name"] = TxtPatientName.Text,
                    ["Sex"] = CmbPatientGender.SelectedItem?.ToString() ?? "Male",
                    ["DateOfBirth"] = DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30),
                    ["PhoneNumber"] = TxtPatientContact.Text,
                    ["Address"] = TxtPatientAddress.Text,
                    ["BloodGroup"] = TxtBloodGroup.Text,
                    ["Allergies"] = TxtAllergies.Text
                };

                await _apiService.PostAsync<object, object>($"{_baseUrl}/api/Patients", patientData);
                MessageBox.Show("Patient registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                IsLeftPanelVisible = false;
                ClearRegistrationForm();
                await LoadPatientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        // === PANEL TOGGLE EVENTS ===
        private void ToggleLeftPanelButton_Click(object sender, RoutedEventArgs e) => IsLeftPanelVisible = !IsLeftPanelVisible;
        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e) => IsRightPanelVisible = !IsRightPanelVisible;
        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => _viewModel.SelectedPatient = PatientListBox.SelectedItem as Patient;

        // === PROPERTY CHANGED ===
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}