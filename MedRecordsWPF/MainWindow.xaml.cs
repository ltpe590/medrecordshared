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
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private string _currentUserId;
        private int _currentPatientId;
        private readonly List<PatientViewModel> _allPatients = new();
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public MainWindow()
{
    InitializeComponent();
    InitializeVisitHistory();

    _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    TxtApiUrl.Text = "https://localhost:7287";
    var testUrl = $"{TxtApiUrl.Text}/swagger";

    Loaded += async (s, e) =>
    {
        await Task.Delay(500);

        // Test connection (swagger only)
        var test = await _httpClient.GetAsync(testUrl);
        if (!test.IsSuccessStatusCode)
        {
            MessageBox.Show("API not reachable");
            return;
        }

        // DEV credentials
        TxtUsername.Text = "doctor1";
        TxtPassword.Password = "Password123!";

        // Root URL for API calls
        _baseUrl = TxtApiUrl.Text.Trim();

        LoginButton_Click(null, null);
    };
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
                LoginButton.Content = "Logging in...";
                LoginButton.IsEnabled = false;

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
                    ShowMainContent();
                    await LoadAllPatients();
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
                LoginButton.Content = "Login";
                LoginButton.IsEnabled = true;
            }
        }

        // In MainWindow.xaml.cs, add or replace the existing event handlers:

        // We need a reference to the Grid container to modify column definitions in C#
        // Add 'x:Name="MainGrid"' to your <Grid> tag in XAML first if you haven't already.
        // <Grid x:Name="MainGrid"> ... </Grid> 

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            UpdateGridColumns();
            UpdateMainContentColumnSpan();
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            UpdateGridColumns();
            UpdateMainContentColumnSpan();
        }

        private void UpdateGridColumns()
        {
            // Access the actual ColumnDefinitions by their index (0=Left, 1=Main, 2=Right)

            // Left Column (Index 0)
            if (RegisterExpander.IsExpanded)
            {
                // When expanded, set width back to Auto with MinWidth constraint (defined in XAML)
                MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                // When collapsed, set width explicitly to 0 pixels
                MainGrid.ColumnDefinitions[0].Width = new GridLength(0);
            }

            // Right Column (Index 2)
            if (HistoryExpander.IsExpanded)
            {
                // When expanded, set width back to Auto with MinWidth constraint (defined in XAML)
                MainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                // When collapsed, set width explicitly to 0 pixels
                MainGrid.ColumnDefinitions[2].Width = new GridLength(0);
            }
        }

        // Keep this method as it was from the previous instructions:
        private void UpdateMainContentColumnSpan()
        {
            int startColumn = RegisterExpander.IsExpanded ? 1 : 0;
            int endColumn = HistoryExpander.IsExpanded ? 1 : 2;
            int columnSpan = (endColumn - startColumn) + 1;

            Grid.SetColumn(MainContentScrollViewer, startColumn);
            Grid.SetColumnSpan(MainContentScrollViewer, columnSpan);
        }



        private void ShowMainContent()
        {
            MainContentSection.Visibility = Visibility.Visible;
        }

        private async Task LoadAllPatients()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Patients");
                if (!response.IsSuccessStatusCode) return;

                var patients = await response.Content.ReadFromJsonAsync<List<Patient>>(_jsonOptions);
                _allPatients.Clear();
                
                // IDE0301 Fixed: Use AddRange instead of individual Add calls
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

        private async void RegisterPatientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegisterPatientButton.Content = "Registering...";
                RegisterPatientButton.IsEnabled = false;

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

                    // IDE0028 Fixed: Use Clear() instead of = ""
                    TxtPatientName.Clear();
                    DatePatientBirth.SelectedDate = null;
                    CmbPatientGender.SelectedIndex = 0;
                    TxtPatientContact.Clear();
                    TxtPatientAddress.Clear();
                    TxtBloodGroup.Clear();
                    TxtAllergies.Clear();

                    RegisterStatusText.Text = "Patient registered successfully!";
                    await LoadAllPatients();
                    ShowMainContent();
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
                RegisterPatientButton.Content = "Register Patient";
                RegisterPatientButton.IsEnabled = true;
            }
        }

        private void PatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatientListBox.SelectedItem is PatientViewModel selectedPatient)
            {
                _currentPatientId = selectedPatient.PatientId;
                SelectedPatientInfo.Text = selectedPatient.Name;
                SelectedPatientDetails.Text = $"Age: {selectedPatient.Age} | Gender: {selectedPatient.Gender} | Contact: {selectedPatient.ContactNumber}";
                LoadVisitHistory(_currentPatientId);
                TxtDiagnosis.Clear();
                TxtNotes.Clear();
            }
        }

        private async Task LoadVisitHistory(int patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/Visits");
                if (!response.IsSuccessStatusCode) return;

                var allVisits = await response.Content.ReadFromJsonAsync<List<Visit>>(_jsonOptions);
                var patientVisits = allVisits?.Where(v => GetPropertyValue<int>(v, "PatientId") == patientId)
                                               .OrderByDescending(v => GetPropertyValue<DateTime>(v, "CreatedAt") != default ? GetPropertyValue<DateTime>(v, "CreatedAt") : GetPropertyValue<DateTime>(v, "VisitDate"))
                                               .ToList() ?? new List<Visit>();

                // IDE0037 Fixed: Use var instead of explicit type
                var historyText = new StringBuilder("PATIENT VISIT HISTORY:\n\n");
                int visitCount = 0;

                foreach (var visit in patientVisits)
                {
                    visitCount++;
                    var date = GetPropertyValue<DateTime>(visit, "CreatedAt") != default ? GetPropertyValue<DateTime>(visit, "CreatedAt") : GetPropertyValue<DateTime>(visit, "VisitDate");
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
                SaveVisitButton.Content = "Saving...";
                SaveVisitButton.IsEnabled = false;

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
                    await LoadVisitHistory(_currentPatientId);
                    TxtDiagnosis.Clear();
                    TxtNotes.Clear();
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
                SaveVisitButton.Content = "Save Visit";
                SaveVisitButton.IsEnabled = true;
            }
        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentSection.Visibility = Visibility.Collapsed;
        }

        private void CancelRegisterButton_Click(object sender, RoutedEventArgs e)
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
            RegisterStatusText.Text = "Registration canceled.";
        }

        private void RefreshPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAllPatients();
        }

        private void TxtPatientSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePatientList();
        }

        // ViewModel classes
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
    }
}