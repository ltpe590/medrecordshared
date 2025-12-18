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
using SharedModels.Models; // Add reference to SharedModels

namespace MedRecordsWPF
{
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private int _currentPatientId = 0;

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
                // Test connection by calling a simple endpoint
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/health");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("API connection successful!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = "Connected to API";
                }
                else
                {
                    // Try the root URL if /api/health doesn't exist
                    response = await _httpClient.GetAsync($"{_baseUrl}/");
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

                // Call login endpoint - matches your AuthController
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

                    // Set authorization header for future requests
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _authToken);

                    MessageBox.Show("Login successful!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    EnableDataEntryControls(true);
                    await LoadPatients();
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

        private async Task LoadPatients()
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

                    Dispatcher.Invoke(() =>
                    {
                        // Create display items with formatted names
                        var patientDisplayItems = patients?.Select(p => new PatientDisplayItem(p)).ToList()
                            ?? new List<PatientDisplayItem>();

                        PatientComboBox.ItemsSource = patientDisplayItems;
                        PatientComboBox.DisplayMemberPath = "DisplayName";
                        PatientComboBox.SelectedValuePath = "PatientId";

                        if (patientDisplayItems.Any())
                        {
                            PatientComboBox.SelectedIndex = 0;
                            StatusText.Text = $"{patientDisplayItems.Count} patients loaded";
                        }
                        else
                        {
                            StatusText.Text = "No patients found";
                        }
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Failed to load patients. Status: {response.StatusCode}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error loading patients: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async void PatientComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatientComboBox.SelectedItem is PatientDisplayItem selectedPatient)
            {
                _currentPatientId = selectedPatient.PatientId;
                await LoadVisitHistory(_currentPatientId);

                TxtDiagnosis.Text = string.Empty;
                TxtNotes.Text = string.Empty;
            }
        }

        private async Task LoadVisitHistory(int patientId)
        {
            try
            {
                // Your current API doesn't have GetVisitsByPatient endpoint
                // So we need to get all visits and filter client-side
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Visits");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var allVisits = JsonSerializer.Deserialize<List<Visit>>(
                        responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // Filter visits for this patient
                    var patientVisits = allVisits?.Where(v => v.PatientId == patientId).ToList()
                        ?? new List<Visit>();

                    string historyText = "PATIENT VISIT HISTORY:\n\n";
                    int visitCount = 0;

                    foreach (var visit in patientVisits.OrderByDescending(v => v.VisitDate))
                    {
                        visitCount++;
                        var visitDate = visit.VisitDate.ToString("yyyy-MM-dd HH:mm");
                        var doctorName = GetDoctorNameFromVisit(visit); // You'll need to implement this

                        historyText += $"Visit #{visitCount} - {visitDate}\n";
                        historyText += $"Doctor: {doctorName}\n";
                        historyText += $"Diagnosis: {visit.Diagnosis}\n";
                        if (!string.IsNullOrWhiteSpace(visit.Notes))
                            historyText += $"Notes: {visit.Notes}\n";
                        historyText += new string('-', 50) + "\n\n";
                    }

                    if (visitCount == 0)
                    {
                        historyText = "No previous visits found for this patient.";
                    }

                    Dispatcher.Invoke(() =>
                    {
                        HistoryTextBlock.Text = historyText;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        HistoryTextBlock.Text = "Error loading visit history.";
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    HistoryTextBlock.Text = $"Error loading visit history: {ex.Message}";
                });
            }
        }

        private string GetDoctorNameFromVisit(Visit visit)
        {
            // Since your Visit model doesn't include Doctor navigation property in the GET endpoint
            // You have a few options:

            // Option 1: If DoctorId is stored in the Visit model
            if (!string.IsNullOrEmpty(visit.DoctorId))
            {
                return $"Dr. {visit.DoctorId}"; // Or fetch doctor details from API
            }

            // Option 2: If you have a DoctorName property
            if (!string.IsNullOrEmpty(visit.DoctorName))
            {
                return visit.DoctorName;
            }

            return "Unknown Doctor";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPatientId == 0)
            {
                MessageBox.Show("Please select a patient first.", "No Patient Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string diagnosis = TxtDiagnosis.Text;
            string notes = TxtNotes.Text;

            if (string.IsNullOrWhiteSpace(diagnosis))
            {
                MessageBox.Show("Please enter a diagnosis.", "Missing Information",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveButton.Content = "Saving...";
                SaveButton.IsEnabled = false;

                // Create a new Visit object matching your model
                var visit = new Visit
                {
                    PatientId = _currentPatientId,
                    Diagnosis = diagnosis,
                    Notes = notes,
                    VisitDate = DateTime.Now,
                    // You'll need to set DoctorId from the logged-in user
                    DoctorId = _currentUserId
                };

                var json = JsonSerializer.Serialize(visit);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/Visits", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Visit saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadVisitHistory(_currentPatientId);
                    TxtDiagnosis.Text = string.Empty;
                    TxtNotes.Text = string.Empty;
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
                SaveButton.Content = "Save Visit";
                SaveButton.IsEnabled = true;
            }
        }

        private void EnableDataEntryControls(bool enable)
        {
            PatientComboBox.IsEnabled = enable;
            TxtDiagnosis.IsEnabled = enable;
            TxtNotes.IsEnabled = enable;
            SaveButton.IsEnabled = enable;
        }

        // Helper class for displaying patients
        public class PatientDisplayItem
        {
            private readonly Patient _patient;

            public PatientDisplayItem(Patient patient)
            {
                _patient = patient;
            }

            public int PatientId => _patient.PatientId;
            public string DisplayName => $"{_patient.LastName}, {_patient.FirstName} (Age: {CalculateAge(_patient.DateOfBirth)})";

            private int CalculateAge(DateTime dateOfBirth)
            {
                DateTime today = DateTime.Today;
                int age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        // DTO for login response
        public class LoginResponse
        {
            public string Token { get; set; }
            public string Message { get; set; }
            public string UserId { get; set; }
        }
    }
}