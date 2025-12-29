using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Domain.Models;

namespace MedRecordsWPF
{
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private int _currentPatientId;
        private List<PatientViewModel> _allPatients = new();
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public MainWindow()
        {
            InitializeComponent();
            InitializeVisitHistory();
            SetupHttpClient();
            SetupAutoLogin();
        }

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

        // Fix for mouse scrolling
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
            e.Handled = true;
        }

        // Panel toggle methods
        private void ToggleLeftPanelButton_Click(object sender, RoutedEventArgs e)
        {
            LeftPanelBorder.Visibility = LeftPanelBorder.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (LeftPanelBorder.Visibility == Visibility.Visible)
            {
                MainContentScrollViewer.Margin = new Thickness(410, 0, 0, 0);
            }
            else
            {
                MainContentScrollViewer.Margin = new Thickness(0);
            }
        }

        private void ToggleRightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            RightPanelBorder.Visibility = RightPanelBorder.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (RightPanelBorder.Visibility == Visibility.Visible)
            {
                MainContentScrollViewer.Margin = new Thickness(0, 0, 410, 0);
            }
            else
            {
                MainContentScrollViewer.Margin = new Thickness(0);
            }
        }

        private void CloseLeftPanelButton_Click(object sender, RoutedEventArgs e)
        {
            LeftPanelBorder.Visibility = Visibility.Collapsed;
            MainContentScrollViewer.Margin = new Thickness(0);
        }

        private void CloseRightPanelButton_Click(object sender, RoutedEventArgs e)
        {
            RightPanelBorder.Visibility = Visibility.Collapsed;
            MainContentScrollViewer.Margin = new Thickness(0);
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

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformLoginAsync();
        }

        private async Task PerformLoginAsync()
        {
            if (!ValidateLoginInputs()) return;

            try
            {
                SetLoginButtonState(isLoggingIn: true);
                var loginRequest = new { Username = TxtUsername.Text, Password = TxtPassword.Password };
                var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    await ProcessSuccessfulLogin(response);
                }
                else
                {
                    await ProcessFailedLogin(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoginButtonState(isLoggingIn: false);
            }
        }

        private bool ValidateLoginInputs()
        {
            if (string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                MessageBox.Show("Please enter API URL and test connection first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SetLoginButtonState(bool isLoggingIn)
        {
            LoginButton.Content = isLoggingIn ? "Logging in..." : "Login";
            LoginButton.IsEnabled = !isLoggingIn;
        }

        private async Task ProcessSuccessfulLogin(HttpResponseMessage response)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            _authToken = loginResponse.Token;
            _currentUserId = loginResponse.UserId;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            LoginExpander.IsExpanded = false;
            MessageBox.Show($"Login successful! Welcome, {TxtUsername.Text}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadAllPatients();
        }

        private async Task ProcessFailedLogin(HttpResponseMessage response)
        {
            var error = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Login failed: {error}", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void InitializeVisitHistory()
        {
            HistoryTextBlock.Text = "No patient selected. Please login and select a patient to view visit history.";
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatientListBox.SelectedItem is PatientViewModel selectedPatient)
            {
                _currentPatientId = selectedPatient.PatientId;
                UpdateSelectedPatientInfo(selectedPatient);
                LoadVisitHistory(_currentPatientId);
                ClearVisitForm();

                // Update Visit Management header
                VisitManagementPatientRun.Text = $" ({selectedPatient.Name})";
            }
            else
            {
                VisitManagementPatientRun.Text = "";
            }
        }


        private async Task LoadAllPatients()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Patients");
                if (!response.IsSuccessStatusCode) return;

                var patients = await response.Content.ReadFromJsonAsync<List<Patient>>(_jsonOptions);
                _allPatients.Clear();

                _allPatients.AddRange(patients?.Select(p => new PatientViewModel
                {
                    PatientId = GetPropertyValue<int>(p, "PatientId"),
                    Name = GetPropertyValue<string>(p, "Name") ?? "",
                    DateOfBirth = GetPropertyValue<DateTime>(p, "DateOfBirth"),
                    Gender = GetPropertyValue<string>(p, "Sex") ?? "",
                    ContactNumber = GetPropertyValue<string>(p, "PhoneNumber") ?? "",
                    Address = GetPropertyValue<string>(p, "Address") ?? ""
                }) ?? Array.Empty<PatientViewModel>());

                UpdatePatientList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void UpdateSelectedPatientInfo(PatientViewModel patient)
        {
            SelectedPatientBorder.Visibility = Visibility.Visible;
            SelectedPatientInfo.Text = patient.Name;
            SelectedPatientDetails.Text =
                $"Age: {patient.Age} | Gender: {patient.Gender} | Contact: {patient.ContactNumber}";
        }

        private void ClearVisitForm()
        {
            TxtDiagnosis.Clear();
            TxtNotes.Clear();
        }

        private async void LoadVisitHistory(int patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Visits");
                if (!response.IsSuccessStatusCode) return;

                var allVisits = await response.Content.ReadFromJsonAsync<List<Visit>>(_jsonOptions);
                var patientVisits = allVisits?.Where(v => GetPropertyValue<int>(v, "PatientId") == patientId)
                                               .OrderByDescending(v => GetPropertyValue<DateTime>(v, "CreatedAt") != default ?
                                                   GetPropertyValue<DateTime>(v, "CreatedAt") :
                                                   GetPropertyValue<DateTime>(v, "VisitDate"))
                                               .ToList() ?? new List<Visit>();

                DisplayVisitHistory(patientVisits);
            }
            catch (Exception ex)
            {
                HistoryTextBlock.Text = $"Error loading visit history: {ex.Message}";
            }
        }

        private void DisplayVisitHistory(List<Visit> patientVisits)
        {
            var historyText = new StringBuilder("PATIENT VISIT HISTORY:\n\n");
            int visitCount = 0;

            foreach (var visit in patientVisits)
            {
                visitCount++;
                var date = GetPropertyValue<DateTime>(visit, "CreatedAt") != default ?
                    GetPropertyValue<DateTime>(visit, "CreatedAt") :
                    GetPropertyValue<DateTime>(visit, "VisitDate");
                var diagnosis = GetPropertyValue<string>(visit, "Diagnosis") ?? "";
                var notes = GetPropertyValue<string>(visit, "Notes") ?? "";

                historyText.AppendLine($"Visit #{visitCount} - {date:yyyy-MM-dd HH:mm}");
                historyText.AppendLine($"Diagnosis: {diagnosis}");
                if (!string.IsNullOrWhiteSpace(notes))
                    historyText.AppendLine($"Notes: {notes}");
                historyText.AppendLine(new string('-', 50));
                historyText.AppendLine();
            }

            HistoryTextBlock.Text = visitCount == 0 ? "No previous visits found for this patient." : historyText.ToString();

            // Auto-open right panel when patient is selected
            if (visitCount > 0 && RightPanelBorder.Visibility != Visibility.Visible)
            {
                ToggleRightPanelButton_Click(null, null);
            }
        }

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateVisitInputs()) return;

            try
            {
                SetSaveVisitButtonState(isSaving: true);

                var visitData = CreateVisitData();
                var content = new StringContent(JsonSerializer.Serialize(visitData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Visits", content);

                if (response.IsSuccessStatusCode)
                {
                    ProcessSuccessfulVisitSave();
                }
                else
                {
                    await ProcessFailedVisitSave(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visit: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetSaveVisitButtonState(isSaving: false);
            }
        }

        private bool ValidateVisitInputs()
        {
            if (_currentPatientId == 0)
            {
                MessageBox.Show("Please select a patient first.", "No Patient Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SetSaveVisitButtonState(bool isSaving)
        {
            SaveVisitButton.Content = isSaving ? "Saving..." : "Save Visit";
            SaveVisitButton.IsEnabled = !isSaving;
        }

        private Dictionary<string, object> CreateVisitData()
        {
            var visitData = new Dictionary<string, object>
            {
                ["PatientId"] = _currentPatientId,
                ["Diagnosis"] = TxtDiagnosis.Text,
                ["Notes"] = TxtNotes.Text,
                ["CreatedAt"] = DateTime.Now
            };

            if (!string.IsNullOrEmpty(_currentUserId))
                visitData["DoctorId"] = _currentUserId;

            return visitData;
        }

        private void ProcessSuccessfulVisitSave()
        {
            MessageBox.Show("Visit saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadVisitHistory(_currentPatientId);
            ClearVisitForm();
        }

        private async Task ProcessFailedVisitSave(HttpResponseMessage response)
        {
            var error = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Failed to save visit: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateRegistrationInputs()) return;

            try
            {
                SetRegisterButtonState(isRegistering: true);

                var patientData = CreatePatientRegistrationData();
                var content = new StringContent(JsonSerializer.Serialize(patientData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Patients", content);

                if (response.IsSuccessStatusCode)
                {
                    await ProcessSuccessfulRegistration();
                }
                else
                {
                    await ProcessFailedRegistration(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetRegisterButtonState(isRegistering: false);
            }
        }

        private bool ValidateRegistrationInputs()
        {
            if (string.IsNullOrWhiteSpace(TxtPatientName.Text))
            {
                MessageBox.Show("Please enter patient name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DatePatientBirth.SelectedDate == null)
            {
                MessageBox.Show("Please select date of birth.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SetRegisterButtonState(bool isRegistering)
        {
            RegisterPatientButton.Content = isRegistering ? "Registering..." : "Register Patient";
            RegisterPatientButton.IsEnabled = !isRegistering;
        }

        private Dictionary<string, object> CreatePatientRegistrationData()
        {
            return new Dictionary<string, object>
            {
                ["Name"] = TxtPatientName.Text,
                ["Sex"] = (CmbPatientGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Male",
                ["DateOfBirth"] = DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30),
                ["PhoneNumber"] = TxtPatientContact.Text,
                ["Address"] = TxtPatientAddress.Text,
                ["BloodGroup"] = TxtBloodGroup.Text,
                ["Allergies"] = TxtAllergies.Text
            };
        }

        private async Task ProcessSuccessfulRegistration()
        {
            MessageBox.Show("Patient registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearRegistrationForm();
            RegisterStatusText.Text = "Patient registered successfully!";
            await LoadAllPatients();
            CloseLeftPanelButton_Click(null, null);
        }

        private async Task ProcessFailedRegistration(HttpResponseMessage response)
        {
            var error = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Failed to register patient: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ClearRegistrationForm()
        {
            TxtPatientName.Clear();
            DatePatientBirth.SelectedDate = null;
            CmbPatientGender.SelectedIndex = 0;
            CmbPatientMaritalStatus.SelectedIndex = 0;
            TxtPatientContact.Clear();
            TxtPatientEmail.Clear();
            TxtPatientAddress.Clear();
            TxtBloodGroup.Clear();
            TxtAllergies.Clear();
        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleLeftPanelButton_Click(null, null);
        }

        private void CancelRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ClearRegistrationForm();
            RegisterStatusText.Text = "Registration canceled.";
            CloseLeftPanelButton_Click(null, null);
        }

        private void RefreshPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadAllPatients();
        }

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePatientList();
        }

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
    }

    //public class Patient { }
    //public class Visit { }
}