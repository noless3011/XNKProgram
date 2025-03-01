using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DescriptionGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _apiBaseUrl = "http://localhost:5000";
    private readonly HttpClient _httpClient;
    private string _uploadedFileId;
    private string _originalFileName;
    private string _outputFolder;
    private List<string> _sheetList = new List<string>();
    private Dictionary<string, string> _sheetTypes = new Dictionary<string, string>();
    private bool _isFileUploaded = false;
    private bool _isProcessing = false;

    public event PropertyChangedEventHandler PropertyChanged;

    public List<string> SheetList
    {
        get => _sheetList;
        set
        {
            _sheetList = value;
            OnPropertyChanged(nameof(SheetList));
        }
    }

    public Dictionary<string, string> SheetTypes
    {
        get => _sheetTypes;
        set
        {
            _sheetTypes = value;
            OnPropertyChanged(nameof(SheetTypes));
        }
    }

    public bool IsFileUploaded
    {
        get => _isFileUploaded;
        set
        {
            _isFileUploaded = value;
            OnPropertyChanged(nameof(IsFileUploaded));
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            OnPropertyChanged(nameof(IsProcessing));
        }
    }

    public string OutputFolder
    {
        get => _outputFolder;
        set
        {
            _outputFolder = value;
            OnPropertyChanged(nameof(OutputFolder));
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _httpClient = new HttpClient();
        OutputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ExcelProcessor");
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void BtnUploadExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Select an Excel File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsProcessing = true;
                StatusText.Text = "Uploading file...";

                var fileUploadResult = await UploadExcelFile(openFileDialog.FileName);
                if (fileUploadResult != null)
                {
                    _uploadedFileId = fileUploadResult.FileId;
                    _originalFileName = fileUploadResult.OriginalFilename;
                    FileName.Text = _originalFileName;

                    // Fetch sheet names
                    await GetSheetNames();
                    IsFileUploaded = true;
                    StatusText.Text = "File uploaded successfully";
                }
                else
                {
                    StatusText.Text = "Failed to upload file";
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Error uploading file";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async void BtnProcess_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_uploadedFileId))
        {
            MessageBox.Show("Please upload an Excel file first.", "No File", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_sheetTypes.Count == 0)
        {
            MessageBox.Show("Please select sheet types for processing.", "No Sheets Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsProcessing = true;
            StatusText.Text = "Processing sheets...";

            var result = await ProcessExcelSheets();
            if (result != null)
            {
                StringBuilder resultMessage = new StringBuilder("Processing completed:\n\n");

                foreach (var sheet in result)
                {
                    if (sheet.Value.Status == "success")
                    {
                        resultMessage.AppendLine($"✓ {sheet.Key}: {sheet.Value.Type} - {sheet.Value.OutputPath}");
                    }
                    else
                    {
                        resultMessage.AppendLine($"✗ {sheet.Key}: Error - {sheet.Value.Message}");
                    }
                }

                MessageBox.Show(resultMessage.ToString(), "Processing Results", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusText.Text = "Processing completed";
            }
            else
            {
                StatusText.Text = "Processing failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Error processing file";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void BtnSelectOutputFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
        {
            Title = "Select Output Folder",
            IsFolderPicker = true,
            InitialDirectory = OutputFolder
        };

        if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
        {
            OutputFolder = dialog.FileName;
            OutputFolderText.Text = OutputFolder;
        }
    }

    private void SheetTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.Tag is string sheetName)
        {
            var selectedType = comboBox.SelectedItem as string;
            if (selectedType != null)
            {
                if (_sheetTypes.ContainsKey(sheetName))
                {
                    _sheetTypes[sheetName] = selectedType.ToLower();
                }
                else
                {
                    _sheetTypes.Add(sheetName, selectedType.ToLower());
                }
            }
        }
    }

    private async Task<UploadResult> UploadExcelFile(string filePath)
    {
        using (var content = new MultipartFormDataContent())
        {
            var fileStream = new FileStream(filePath, FileMode.Open);
            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/upload-excel", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UploadResult>(jsonResult);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error: {errorContent}");
            }
        }
    }

    private async Task GetSheetNames()
    {
        var response = await _httpClient.GetAsync($"{_apiBaseUrl}/get-sheets/{_uploadedFileId}");

        if (response.IsSuccessStatusCode)
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var sheetsResult = JsonConvert.DeserializeObject<SheetsResult>(jsonResult);

            SheetList = new List<string>(sheetsResult.Sheets);
            _sheetTypes.Clear();

            // Create the UI for sheets
            SheetsPanel.Children.Clear();

            foreach (var sheet in SheetList)
            {
                var sheetPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var sheetLabel = new TextBlock
                {
                    Text = sheet,
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var typeComboBox = new ComboBox
                {
                    Width = 100,
                    Tag = sheet,
                    ItemsSource = new List<string> { "Table", "UI" }
                };
                typeComboBox.SelectionChanged += SheetTypeChanged;

                sheetPanel.Children.Add(sheetLabel);
                sheetPanel.Children.Add(typeComboBox);
                SheetsPanel.Children.Add(sheetPanel);
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API error: {errorContent}");
        }
    }

    private async Task<Dictionary<string, SheetResult>> ProcessExcelSheets()
    {
        var requestBody = new ProcessRequest
        {
            FileId = _uploadedFileId,  // Use the file_id directly
            OutputFolder = OutputFolder,
            Sheets = _sheetTypes
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/process-excel", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, SheetResult>>(jsonResult);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API error: {errorContent}");
        }
    }

    // Models for API responses
    public class UploadResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("original_filename")]
        public string OriginalFilename { get; set; }

        [JsonProperty("file_path")]
        public string FilePath { get; set; }
    }

    public class SheetsResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("sheets")]
        public List<string> Sheets { get; set; }
    }

    public class SheetResult
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("output_path")]
        public string OutputPath { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class ProcessRequest
    {
        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("output_folder")]
        public string OutputFolder { get; set; }

        [JsonProperty("sheets")]
        public Dictionary<string, string> Sheets { get; set; }
    }
}