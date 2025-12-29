using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.ViewModels;

namespace MedRecordsWPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly MainViewModel _viewModel;
        private readonly IPatientRepository _patientRepository;
        private HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private int _currentPatientId;
        private List<PatientViewModel> _allPatients = new();
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private bool _isLeftPanelVisible;
        public bool IsLeftPanelVisible
        {
            get => _isLeftPanelVisible;
            set
            {
                _isLeftPanelVisible = value;
                OnPropertyChanged();
                UpdatePanelVisibility();
            }
        }

        private bool _isRightPanelVisible;
        public bool IsRightPanelVisible
        {
            get => _isRightPanelVisible;
            set
            {
                _isRightPanelVisible = value;
                OnPropertyChanged();
                UpdatePanelVisibility();
            }
        }
        #endregion

        public MainWindow(MainViewModel viewModel, IPatientRepository patientRepository)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _patientRepository = patientRepository;

            DataContext = this;
            DataContext = _viewModel; // Set both as data contexts

            InitializeVisitHistory();
            SetupHttpClient();
            SetupAutoLogin();
            SetupPanelEvents();

            // Set initial panel state
            IsLeftPanelVisible = false;
            IsRightPanelVisible = false;
        }

        #region Initialization
        private void SetupHttpClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private void SetupAutoLogin()
        {
            TxtApiUrl.Text = "https://localhost:7287";

            Loaded += async (s, e) =>
            {
                await Task.Delay(500);
                await TestConnectionOnStartup();

                // Set development credentials
                TxtUsername.Text = "doctor1";
                TxtPassword.Password = "Password123!";

                // Root URL for API calls
                _baseUrl = TxtApiUrl.Text.Trim();

                // Auto-login if connection is successful
                await AttemptAutoLogin();
            };
        }

        private void SetupPanelEvents()
        {
            // Auto-open right panel when patient selected
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedPatient))
                {
                    if (_viewModel.SelectedPatient != null)
                    {
                        IsRightPanelVisible = true;
                        UpdateSelectedPatientInfo(_viewModel.SelectedPatient);
                    }
                }
            };
        }

        private void InitializeVisitHistory()
        {
            HistoryTextBlock.Text = "No patient selected. Please login and select a patient to view visit history.";
        }
        #endregion

        #region Panel Visibility Management
        private void UpdatePanelVisibility()
        {
            LeftPanelBorder.Visibility = IsLeftPanelVisible ? Visibility.Visible : Visibility.Collapsed;
            RightPanelBorder.Visibility = IsRightPanelVisible ? Visibility.Visible : Visibility.Collapsed;

            // Update button text
            ToggleLeftPanelButton.Content = IsLeftPanelVisible ? "< Hide Registration" : "> Register New Patient";
            ToggleRightPanelButton.Content = IsRightPanelVisible ? "< Hide History" : "< Previous Visits";
        }

        private void ToggleLeftPanelButton_Click(object sender, RoutedEventArgs e)
        {
            IsLeftPanelVisible = !IsLeftPanelVisible;
        }

        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            IsRightPanelVisible = !IsRightPanelVisible;
        }

        private void CloseLeftPanelButton_Click(object sender, RoutedEventArgs e)
        {
            IsLeftPanelVisible = false;
        }

        private void CloseRightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            IsRightPanelVisible = false;
        }
        #endregion

        #region Event Handlers (Connected to your existing methods)
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
            e.Handled = true;
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await TestConnectionAsync();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformLoginAsync();
        }

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveVisitAsync();
        }

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e)
        {
            await RegisterPatientAsync();
        }

        private async void RefreshPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAllPatientsAsync();
        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e)
        {
            IsLeftPanelVisible = true;
        }

        private void CancelRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ClearRegistrationForm();
            IsLeftPanelVisible = false;
        }

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This will trigger the ViewModel's SearchText property
            _viewModel.SearchText = TxtPatientSearch.Text;
            PatientListBox.ItemsSource = _viewModel.FilteredPatients;
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedPatient = PatientListBox.SelectedItem as Patient;
        }
        #endregion

        #region Clean Architecture Integration
        private void UpdateSelectedPatientInfo(Patient patient)
        {
            if (patient != null)
            {
                SelectedPatientBorder.Visibility = Visibility.Visible;
                SelectedPatientInfo.Text = patient.Name;
                SelectedPatientDetails.Text =
                    $"Age: {CalculateAge(patient.DateOfBirth)} | Gender: {patient.Sex} | Contact: {patient.PhoneNumber}";

                // Auto-open right panel
                IsRightPanelVisible = true;
            }
            else
            {
                SelectedPatientBorder.Visibility = Visibility.Collapsed;
                VisitManagementPatientRun.Text = "";
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
        #endregion

        #region HTTP Methods (Single implementations)
        private async Task TestConnectionAsync()
        {
            _baseUrl = TxtApiUrl.Text.Trim();
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("API connection successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "Connected to API";
                }
                else
                {
                    MessageBox.Show($"API returned status: {response.StatusCode}", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText.Text = "Connection failed";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Connection error";
            }
        }

        private async Task TestConnectionOnStartup()
        {
            var testUrl = $"{TxtApiUrl.Text}/swagger";
            try
            {
                var test = await _httpClient.GetAsync(testUrl);
                if (!test.IsSuccessStatusCode)
                {
                    StatusText.Text = "API not reachable";
                }
            }
            catch
            {
                StatusText.Text = "API not reachable";
            }
        }

        private async Task AttemptAutoLogin()
        {
            try
            {
                await PerformLoginAsync();
            }
            catch
            {
                // Auto-login failed, user will need to login manually
            }
        }

        private async Task PerformLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                MessageBox.Show("Please enter API URL and test connection first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                LoginButton.Content = "Logging in...";

                var loginRequest = new { Username = TxtUsername.Text, Password = TxtPassword.Password };
                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
                    _authToken = loginResponse.Token;
                    _currentUserId = loginResponse.UserId;

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

                    LoginExpander.IsExpanded = false;
                    MessageBox.Show($"Login successful! Welcome, {TxtUsername.Text}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadAllPatientsAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {error}", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Login";
            }
        }

        private async Task LoadAllPatientsAsync()
        {
            try
            {
                // First try clean architecture repository
                if (_patientRepository != null)
                {
                    var patients = await _patientRepository.GetAllAsync();
                    if (patients != null && patients.Any())
                    {
                        _allPatients.Clear();
                        _allPatients.AddRange(patients.Select(p => new PatientViewModel
                        {
                            PatientId = p.PatientId,
                            Name = p.Name ?? "",
                            DateOfBirth = p.DateOfBirth,
                            Gender = p.Sex ?? "",
                            ContactNumber = p.PhoneNumber ?? "",
                            Address = p.Address ?? ""
                        }));

                        UpdatePatientList();
                        StatusText.Text = $"Loaded {_allPatients.Count} patients (via repository)";
                        return;
                    }
                }

                // Fallback to HTTP API
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Patients");
                if (!response.IsSuccessStatusCode) return;

                var patientsFromApi = await response.Content.ReadFromJsonAsync<List<Patient>>(_jsonOptions);
                _allPatients.Clear();

                _allPatients.AddRange(patientsFromApi?.Select(p => new PatientViewModel
                {
                    PatientId = GetPropertyValue<int>(p, "PatientId"),
                    Name = GetPropertyValue<string>(p, "Name") ?? "",
                    DateOfBirth = GetPropertyValue<DateTime>(p, "DateOfBirth"),
                    Gender = GetPropertyValue<string>(p, "Sex") ?? "",
                    ContactNumber = GetPropertyValue<string>(p, "PhoneNumber") ?? "",
                    Address = GetPropertyValue<string>(p, "Address") ?? ""
                }) ?? Array.Empty<PatientViewModel>());

                UpdatePatientList();
                StatusText.Text = $"Loaded {_allPatients.Count} patients (via API)";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error loading patients: {ex.Message}";
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePatientList()
        {
            var searchText = TxtPatientSearch?.Text?.ToLower() ?? "";
            var filteredPatients = _allPatients
                .Where(p => string.IsNullOrEmpty(searchText) ||
                           p.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           p.ContactNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(p => p.Name)
                .ToList();

            PatientListBox.ItemsSource = filteredPatients;
        }

        private static T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName);
                return prop?.GetValue(obj) is T typedValue ? typedValue : default;
            }
            catch { return default; }
        }

        private async Task SaveVisitAsync()
        {
            if (_currentPatientId == 0)
            {
                MessageBox.Show("Please select a patient first.", "No Patient Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveVisitButton.IsEnabled = false;
                SaveVisitButton.Content = "Saving...";

                var visitData = new Dictionary<string, object>
                {
                    ["PatientId"] = _currentPatientId,
                    ["Diagnosis"] = TxtDiagnosis.Text,
                    ["Notes"] = TxtNotes.Text,
                    ["CreatedAt"] = DateTime.Now
                };

                if (!string.IsNullOrEmpty(_currentUserId))
                    visitData["DoctorId"] = _currentUserId;

                var content = new StringContent(JsonSerializer.Serialize(visitData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Visits", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Visit saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadVisitHistory(_currentPatientId);
                    ClearVisitForm();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save visit: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveVisitButton.IsEnabled = true;
                SaveVisitButton.Content = "Save Visit";
            }
        }

        private async Task RegisterPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(TxtPatientName.Text))
            {
                MessageBox.Show("Please enter patient name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DatePatientBirth.SelectedDate == null)
            {
                MessageBox.Show("Please select date of birth.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                RegisterPatientButton.IsEnabled = false;
                RegisterPatientButton.Content = "Registering...";

                var patientData = new Dictionary<string, object>
                {
                    ["Name"] = TxtPatientName.Text,
                    ["Sex"] = (CmbPatientGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Male",
                    ["DateOfBirth"] = DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30),
                    ["PhoneNumber"] = TxtPatientContact.Text,
                    ["Address"] = TxtPatientAddress.Text,
                    ["BloodGroup"] = TxtBloodGroup.Text,
                    ["Allergies"] = TxtAllergies.Text
                };

                var content = new StringContent(JsonSerializer.Serialize(patientData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Patients", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Patient registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearRegistrationForm();
                    IsLeftPanelVisible = false;
                    await LoadAllPatientsAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to register patient: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RegisterPatientButton.IsEnabled = true;
                RegisterPatientButton.Content = "Register Patient";
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

        private void ClearVisitForm()
        {
            TxtDiagnosis.Clear();
            TxtNotes.Clear();
        }

        private void LoadVisitHistory(int patientId)
        {
            // Implementation for loading visit history
            HistoryTextBlock.Text = $"Visit history for patient {patientId} will appear here.";
        }
        #endregion

        #region Helper Classes
        public class PatientViewModel
        {
            public int PatientId { get; set; }
            public string Name { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string ContactNumber { get; set; }
            public string Address { get; set; }

            public string DisplayName => Name;
            public int Age => AgeConverter.DateOfBirthToAge(DateOfBirth);
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public string Message { get; set; }
            public string UserId { get; set; }
        }

        public static class AgeConverter
        {
            public static int DateOfBirthToAge(DateTime dateOfBirth)
            {
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        #endregion
    }
}