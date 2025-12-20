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
using SharedModels.Models;

namespace MedRecordsWPF
{
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private int _currentPatientId = 0;
        private List<PatientViewModel> _allPatients = new List<PatientViewModel>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeVisitHistory();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        private void InitializeVisitHistory()
        {
            HistoryTextBlock.Text = "No patient selected. Please login and select a patient to view visit history.";
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            _baseUrl = TxtApiUrl.Text.Trim();

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("API connection successful!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "Connected to API";
                }
                else
                {
                    MessageBox.Show($"API returned status: {response.StatusCode}", "Connection Failed",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatusText.Text = "Connection failed";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot connect to API: {ex.Message}", "Connection Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Connection error";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text;
            string password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                MessageBox.Show("Please enter API URL and test connection first.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LoginButton.Content = "Logging in...";
                LoginButton.IsEnabled = false;

                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    _authToken = loginResponse.Token;
                    _currentUserId = loginResponse.UserId;

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _authToken);

                    MessageBox.Show($"Login successful! Welcome, {username}!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Switch to main content
                    ShowMainContent();
                    await LoadAllPatients();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {error}", "Login Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoginButton.Content = "Login";
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowMainContent()
        {
            // Hide login/register sections
            LoginSection.Visibility = Visibility.Collapsed;
            RegisterSection.Visibility = Visibility.Collapsed;
            MiddleDivider.Visibility = Visibility.Collapsed;

            // Show main content
            MainContentSection.Visibility = Visibility.Visible;
        }

        private async Task LoadAllPatients()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Patients");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var patients = JsonSerializer.Deserialize<List<Patient>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    _allPatients = patients?.Select(p => new PatientViewModel
                    {
                        PatientId = GetPropertyValue<int>(p, "PatientId"),
                        FirstName = GetPropertyValue<string>(p, "FirstName") ?? GetPropertyValue<string>(p, "Name") ?? "",
                        LastName = GetPropertyValue<string>(p, "LastName") ?? GetPropertyValue<string>(p, "Surname") ?? "",
                        DateOfBirth = GetPropertyValue<DateTime>(p, "DateOfBirth"),
                        Gender = GetPropertyValue<string>(p, "Gender") ?? "",
                        ContactNumber = GetPropertyValue<string>(p, "ContactNumber") ?? "",
                        Email = GetPropertyValue<string>(p, "Email") ?? "",
                        Address = GetPropertyValue<string>(p, "Address") ?? ""
                    }).ToList() ?? new List<PatientViewModel>();

                    UpdatePatientList();
                }
                else
                {
                    MessageBox.Show($"Failed to load patients. Status: {response.StatusCode}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    var value = prop.GetValue(obj);
                    if (value is T typedValue)
                        return typedValue;
                }
            }
            catch { }
            return default(T);
        }

        private void UpdatePatientList()
        {
            var searchText = TxtPatientSearch?.Text?.ToLower() ?? "";

            var filteredPatients = _allPatients
                .Where(p => string.IsNullOrEmpty(searchText) ||
                           p.DisplayName.ToLower().Contains(searchText) ||
                           p.ContactNumber?.ToLower().Contains(searchText) == true)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            PatientListBox.ItemsSource = filteredPatients;
        }

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegisterPatientButton.Content = "Registering...";
                RegisterPatientButton.IsEnabled = false;

                var patientData = new Dictionary<string, object>
                {
                    ["FirstName"] = TxtPatientFirstName.Text,
                    ["LastName"] = TxtPatientLastName.Text,
                    ["DateOfBirth"] = DatePatientBirth.SelectedDate ?? DateTime.Now.AddYears(-30),
                    ["Gender"] = (CmbPatientGender.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Male",
                    ["MaritalStatus"] = (CmbPatientMaritalStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Single",
                    ["ContactNumber"] = TxtPatientContact.Text,
                    ["Email"] = TxtPatientEmail.Text,
                    ["Address"] = TxtPatientAddress.Text
                };

                var json = JsonSerializer.Serialize(patientData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Patients", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Patient registered successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear form
                    TxtPatientFirstName.Text = "";
                    TxtPatientLastName.Text = "";
                    DatePatientBirth.SelectedDate = null;
                    CmbPatientGender.SelectedIndex = 0;
                    CmbPatientMaritalStatus.SelectedIndex = 0;
                    TxtPatientContact.Text = "";
                    TxtPatientEmail.Text = "";
                    TxtPatientAddress.Text = "";

                    RegisterStatusText.Text = "Patient registered successfully!";

                    // Reload patients if logged in
                    if (MainContentSection.Visibility == Visibility.Visible)
                        await LoadAllPatients();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to register patient: {error}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error registering patient: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RegisterPatientButton.Content = "Register Patient";
                RegisterPatientButton.IsEnabled = true;
            }
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatientListBox.SelectedItem is PatientViewModel selectedPatient)
            {
                _currentPatientId = selectedPatient.PatientId;

                // Update selected patient info
                SelectedPatientInfo.Text = $"{selectedPatient.LastName}, {selectedPatient.FirstName}";
                SelectedPatientDetails.Text = $"Age: {CalculateAge(selectedPatient.DateOfBirth)} | Gender: {selectedPatient.Gender} | Contact: {selectedPatient.ContactNumber}";

                LoadVisitHistory(_currentPatientId);
                TxtDiagnosis.Text = string.Empty;
                TxtNotes.Text = string.Empty;
            }
        }

        private async Task LoadVisitHistory(int patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Visits");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var allVisits = JsonSerializer.Deserialize<List<Visit>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // Filter by patient
                    var patientVisits = allVisits?.Where(v =>
                    {
                        var patientIdProp = v.GetType().GetProperty("PatientId");
                        return patientIdProp != null && patientIdProp.GetValue(v) is int id && id == patientId;
                    }).ToList() ?? new List<Visit>();

                    string historyText = "PATIENT VISIT HISTORY:\n\n";
                    int visitCount = 0;

                    foreach (var visit in patientVisits.OrderByDescending(v =>
                    {
                        var dateProp = v.GetType().GetProperty("CreatedAt") ??
                                      v.GetType().GetProperty("VisitDate");
                        return dateProp != null && dateProp.GetValue(v) is DateTime date ? date : DateTime.MinValue;
                    }))
                    {
                        visitCount++;

                        var dateProp = visit.GetType().GetProperty("CreatedAt") ??
                                      visit.GetType().GetProperty("VisitDate");
                        var date = dateProp != null && dateProp.GetValue(visit) is DateTime visitDate
                            ? visitDate.ToString("yyyy-MM-dd HH:mm")
                            : "Unknown Date";

                        var diagnosisProp = visit.GetType().GetProperty("Diagnosis");
                        var diagnosis = diagnosisProp != null ? diagnosisProp.GetValue(visit)?.ToString() : "";

                        var notesProp = visit.GetType().GetProperty("Notes");
                        var notes = notesProp != null ? notesProp.GetValue(visit)?.ToString() : "";

                        historyText += $"Visit #{visitCount} - {date}\n";
                        historyText += $"Diagnosis: {diagnosis}\n";
                        if (!string.IsNullOrWhiteSpace(notes))
                            historyText += $"Notes: {notes}\n";
                        historyText += new string('-', 50) + "\n\n";
                    }

                    if (visitCount == 0)
                    {
                        historyText = "No previous visits found for this patient.";
                    }

                    HistoryTextBlock.Text = historyText;
                }
                else
                {
                    HistoryTextBlock.Text = "Error loading visit history.";
                }
            }
            catch (Exception ex)
            {
                HistoryTextBlock.Text = $"Error loading visit history: {ex.Message}";
            }
        }

        private async void SaveVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPatientId == 0)
            {
                MessageBox.Show("Please select a patient first.", "No Patient Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string diagnosis = TxtDiagnosis.Text;
            string notes = TxtNotes.Text;
            string maritalStatus = (CmbVisitMaritalStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Single";

            if (string.IsNullOrWhiteSpace(diagnosis))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveVisitButton.Content = "Saving...";
                SaveVisitButton.IsEnabled = false;

                var visitData = new Dictionary<string, object>
                {
                    ["PatientId"] = _currentPatientId,
                    ["Diagnosis"] = diagnosis,
                    ["Notes"] = notes,
                    ["MaritalStatus"] = maritalStatus
                };

                // Add additional fields if filled
                if (!string.IsNullOrWhiteSpace(TxtBloodPressure.Text))
                    visitData["BloodPressure"] = TxtBloodPressure.Text;

                if (!string.IsNullOrWhiteSpace(TxtTemperature.Text) && double.TryParse(TxtTemperature.Text, out double temp))
                    visitData["Temperature"] = temp;

                if (!string.IsNullOrWhiteSpace(TxtHeartRate.Text) && int.TryParse(TxtHeartRate.Text, out int hr))
                    visitData["HeartRate"] = hr;

                // Add date
                visitData["CreatedAt"] = DateTime.Now;

                // Add DoctorId
                if (!string.IsNullOrEmpty(_currentUserId))
                    visitData["DoctorId"] = _currentUserId;

                var json = JsonSerializer.Serialize(visitData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Visits", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Visit saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadVisitHistory(_currentPatientId);
                    TxtDiagnosis.Text = string.Empty;
                    TxtNotes.Text = string.Empty;
                    TxtBloodPressure.Text = string.Empty;
                    TxtTemperature.Text = string.Empty;
                    TxtHeartRate.Text = string.Empty;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save visit: {error}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving visit: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveVisitButton.Content = "Save Visit";
                SaveVisitButton.IsEnabled = true;
            }
        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch back to register section temporarily
            MainContentSection.Visibility = Visibility.Collapsed;
            LoginSection.Visibility = Visibility.Collapsed;
            RegisterSection.Visibility = Visibility.Visible;
            MiddleDivider.Visibility = Visibility.Collapsed;
        }

        private void RefreshPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAllPatients();
        }

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePatientList();
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        // ViewModel classes
        public class PatientViewModel
        {
            public int PatientId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Gender { get; set; }
            public string ContactNumber { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }

            public string DisplayName => $"{LastName}, {FirstName}";
            public int Age => CalculateAge(DateOfBirth);

            private int CalculateAge(DateTime dob)
            {
                DateTime today = DateTime.Today;
                int age = today.Year - dob.Year;
                if (dob.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public class LoginResponse
        {
            public string Token { get; set; }
            public string Message { get; set; }
            public string UserId { get; set; }
        }
    }
}