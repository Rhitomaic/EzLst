using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace EzLst;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        BrowseButton.Click += async (_, _) => await OnBrowseClicked();
        ConvertButton.Click += async (_, _) => await OnConvertClicked();
    }

    private async Task OnBrowseClicked()
    {
        var selected = (ModeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (selected == "File")
        {
            var files = await StorageProvider.OpenFilePickerAsync(new()
            {
                Title = "Select a .lst file",
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                PathTextBox.Text = files[0].Path.LocalPath;
            }
        }
        else if (selected == "Directory")
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new()
            {
                Title = "Select a folder"
            });

            if (folders.Count > 0)
            {
                PathTextBox.Text = folders[0].Path.LocalPath;
            }
        }
    }

    private async Task OnConvertClicked()
    {
        try
        {
            var inputPath = PathTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(inputPath))
            {
                StatusLabel.Text = "Please select a file or directory first.";
                return;
            }

            string? customOutput = null;
            if (CustomOutputCheckBox.IsChecked == true)
            {
                customOutput = OutputPathTextBox.Text?.Trim();
            }

            StatusLabel.Text = "Starting conversion...";
            ConvertButton.IsEnabled = false;

            await Task.Run(() =>
                LstConverter.Convert(inputPath, customOutput, msg =>
                    Dispatcher.UIThread.Post(() => StatusLabel.Text = msg)
                )
            );

            Dispatcher.UIThread.Post(() =>
            {
                StatusLabel.Text = "Conversion finished!";
                ConvertButton.IsEnabled = true;
            });
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error: " + ex.Message;
            ConvertButton.IsEnabled = true;
        }
    }
}