﻿/// Clippy - File: "SettingsWindow.xaml.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using Clippy.Resources;
using Microsoft.Win32;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Clippy.UiElements
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Brush s_invalidSettingBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x7F, 0x7F));
        private const string s_settingsInvalidMessage = "One or more of the settings are invalid.";
        private GlobalHotkey _currentHotkey;
        private bool _isRestoring = false;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreSettings();
        }

        private void ButtonBrowseTextFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Title + " - Select clipboard text file...";

            //openFileDialog.Filter = $"Clippy list file (*.{ext})|*.{ext}";

            if (openFileDialog.ShowDialog() == true && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                TextBoxClipboardTextFile.Text = openFileDialog.FileName;
                Encoding encoding = StaticHelper.GetFileEncoding(openFileDialog.FileName);
                ComboBoxEncoding.SelectedIndex = Array.FindIndex(Encoding.GetEncodings(), enc => enc.CodePage == encoding.CodePage);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSettings())
            {
                ApplySettings();
            }
            else
            {
                MessageBox.Show(s_settingsInvalidMessage, Title, MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSettings())
            {
                ApplySettings();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(s_settingsInvalidMessage, Title, MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void FileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateFileTextBox(sender as TextBox);
        }

        private void TextBoxClipboardTextFile_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxClipboardTextFile.Text = TextBoxClipboardTextFile.Text.Trim();
            if (!string.IsNullOrEmpty(TextBoxClipboardTextFile.Text))
            {
                ValidateFileTextBox(TextBoxClipboardTextFile);
            }
        }

        private bool ValidateFileTextBox(TextBox fileTextBox)
        {
            bool result = true;
            if (!StaticHelper.ValidateFileNameAndPath(fileTextBox.Text))
            {
                fileTextBox.Background = s_invalidSettingBrush;
                result = false;
            }
            else if (fileTextBox.Background == s_invalidSettingBrush)
            {
                fileTextBox.ClearValue(BackgroundProperty);
                result = true;
            }

            return result;
        }

        private bool ValidateSettings()
        {
            bool result = true;
            if (CheckBoxUseClipboardFiles.IsChecked.HasValue && CheckBoxUseClipboardFiles.IsChecked.Value)
            {
                result = ValidateFileTextBox(TextBoxClipboardTextFile);
            }

            return result;
        }

        private void RestoreSettings()
        {
            _isRestoring = true;

            // General settings
            CheckBoxAlwaysOnTop.IsChecked = ClippySettings.Instance.MainWindowAlwaysOnTop;
            CheckBoxAutosaveWindowLayout.IsChecked = ClippySettings.Instance.SaveWindowLayoutState;
            CheckBoxAutosaveItems.IsChecked = ClippySettings.Instance.AutoSaveState;
            CheckBoxTextItemNameFromContent.IsChecked = ClippySettings.Instance.TextItemNameFromContent;
            CheckBoxShowTrayIcon.IsChecked = ClippySettings.Instance.ShowIconInSystemTray;
            _currentHotkey = ClippySettings.Instance.GlobalHotkey;
            TextboxGlobalHotkey.Text = _currentHotkey.ToString();
            CheckBoxGlobalHotkey.IsChecked = _currentHotkey.IsActive;

            // Clipboard file settings
            CheckBoxUseClipboardFiles.IsChecked = ClippySettings.Instance.UseClipboardFiles;
            CheckBoxAllowEmptyClipboardFile.IsChecked = ClippySettings.Instance.AllowEmptyClipboardFiles;
            TextBoxClipboardTextFile.Text = ClippySettings.Instance.ClipboardTextFileName;
            if (ClippySettings.Instance.UseClipboardFiles)
            {
                ValidateFileTextBox(TextBoxClipboardTextFile);
            }

            int codepage = ClippySettings.Instance.ClipboardTextFileEncoding;
            ComboBoxEncoding.SelectedIndex = Array.FindIndex(Encoding.GetEncodings(), enc => enc.CodePage == codepage);

            _isRestoring = false;
        }

        private void ApplySettings()
        {
            // General settings
            ClippySettings.Instance.MainWindowAlwaysOnTop = CheckBoxAlwaysOnTop.IsChecked.Value;
            ClippySettings.Instance.SaveWindowLayoutState = CheckBoxAutosaveWindowLayout.IsChecked.Value;
            ClippySettings.Instance.AutoSaveState = CheckBoxAutosaveItems.IsChecked.Value;
            ClippySettings.Instance.TextItemNameFromContent = CheckBoxTextItemNameFromContent.IsChecked.Value;
            ClippySettings.Instance.ShowIconInSystemTray = CheckBoxShowTrayIcon.IsChecked.Value;
            ClippySettings.Instance.GlobalHotkey = _currentHotkey;


            // Clipboard file settings
            ClippySettings.Instance.UseClipboardFiles = CheckBoxUseClipboardFiles.IsChecked.Value;
            ClippySettings.Instance.ClipboardTextFileName = TextBoxClipboardTextFile.Text;
            ClippySettings.Instance.AllowEmptyClipboardFiles = CheckBoxAllowEmptyClipboardFile.IsChecked.Value;

            if (ComboBoxEncoding.SelectedIndex == -1)
            {
                int codepage = Encoding.UTF8.CodePage;
                ClippySettings.Instance.ClipboardTextFileEncoding = codepage;
                ComboBoxEncoding.SelectedIndex = Array.FindIndex(Encoding.GetEncodings(), enc => enc.CodePage == codepage);
            }
            else
            {
                ClippySettings.Instance.ClipboardTextFileEncoding = (ComboBoxEncoding.SelectedItem as EncodingInfo).CodePage;
            }

            ClippySettings.Instance.SaveAllSettings();
        }

        private void TextboxGlobalHotkey_KeyDown(object sender, KeyEventArgs e)
        {
            TextboxGlobalHotkey.Clear();
            _currentHotkey.Key = e.Key;
            _currentHotkey.Modifiers = StaticHelper.GetCurrentKeyModifiers();
            TextboxGlobalHotkey.Text = _currentHotkey.ToString();
        }

        private void CheckBoxGlobalHotkey_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ActivateHotkeyField();
        }

        private void CheckBoxGlobalHotkey_Checked(object sender, RoutedEventArgs e)
        {
            ActivateHotkeyField();
        }

        private void CheckBoxGlobalHotkey_Unchecked(object sender, RoutedEventArgs e)
        {
            _currentHotkey.IsActive = CheckBoxGlobalHotkey.IsChecked.Value;
        }

        private void ActivateHotkeyField()
        {
            _currentHotkey.IsActive = CheckBoxGlobalHotkey.IsChecked.Value;
            if (_currentHotkey.IsActive && _currentHotkey.Key == Key.None)
            {
                TextboxGlobalHotkey.Text = "Press hotkey";
            }

            if (!_isRestoring) TextboxGlobalHotkey.Focus(); ;
        }
    }
}
