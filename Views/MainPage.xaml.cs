using System;
using Microsoft.Toolkit.Uwp;
using Microsoft.UI.Xaml.Controls;
using Rich_Text_Editor.Dialogs;
using Rich_Text_Editor.Helpers;
using Rich_Text_Editor.Views;
using Rich_Text_Editor.Views.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Rich_Text_Editor
{
    public sealed partial class MainPage : Page
    {
        public bool saved = true;
        public bool _wasOpen = false;
        private bool updateFontFormat = true;
        string appTitleStr = "AppName".GetLocalized();
        string fileNameWithPath = "";
        string originalDocText = "";

        public MainPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
        }

        public async void SaveFile(bool isCopy)
        {
            string fileName = (BasePage.Current.Tabs.TabItems[BasePage.Current.Tabs.SelectedIndex] as TabViewItem).Header as string;
            if (isCopy || fileName == "Untitled")
            {
                FileSavePicker savePicker = new()
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we
                    // finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        editor.Document.SaveToStream(file.FileType.Contains("txt") ? TextGetOptions.None : TextGetOptions.FormatRtf, randAccStream);
                    }

                    // Let Windows know that we're finished changing the file so the
                    // other app can update the remote version of the file.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete)
                    {
                        ContentDialog dialog = new()
                        {
                            Title = "An error occurred",
                            Content = "File " + file.Name + " couldn't be saved."
                        };

                        await dialog.ShowAsync();
                    }

                    saved = true;
                    fileNameWithPath = file.Path;
                    (BasePage.Current.Tabs.TabItems[BasePage.Current.Tabs.SelectedIndex] as TabViewItem).Header = file.Name;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Untitled")
            {
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);

                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            editor.Document.SaveToStream(file.FileType.Contains("txt") ? TextGetOptions.None : TextGetOptions.FormatRtf, randAccStream);
                        }

                        // Let Windows know that we're finished changing the file so the
                        // other app can update the remote version of the file.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != FileUpdateStatus.Complete)
                        {
                            Windows.UI.Popups.MessageDialog errorBox = new("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                        saved = true;
                        (BasePage.Current.Tabs.TabItems[BasePage.Current.Tabs.SelectedIndex] as TabViewItem).Header = file.Name;
                        Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove("CurrentlyOpenFile");
                    }
                } 
                catch (Exception)
                {
                    SaveFile(true);
                }
            }
        }

        private async void Print_Click(object sender, RoutedEventArgs e)
        {
            if (PrintManager.IsSupported())
            {
                try
                {
                    // Show print UI
                    await PrintManager.ShowPrintUIAsync();
                }
                catch
                {
                    // Printing cannot proceed at this time
                    ContentDialog noPrintingDialog = new()
                    {
                        Title = "Printing error",
                        Content = "Sorry, printing can't proceed at this time.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                }
            }
            else
            {
                // Printing is not supported on this device
                ContentDialog noPrintingDialog = new()
                {
                    Title = "Printing not supported",
                    Content = "Sorry, printing is not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await noPrintingDialog.ShowAsync();
            }
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Bold);
        }

        private async void NewDoc_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView currentAV = ApplicationView.GetForCurrentView();
            CoreApplicationView newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var newWindow = Window.Current;
                var newAppView = ApplicationView.GetForCurrentView();
                newAppView.Title = $"Untitled - {"AppName".GetLocalized()}";

                var frame = new Frame();
                frame.Navigate(typeof(MainPage));
                newWindow.Content = frame;
                newWindow.Activate();

                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newAppView.Id,
                    ViewSizePreference.UseMinimum, currentAV.Id, ViewSizePreference.UseMinimum);
            });
        }

        private void StrikethroughButton_Click(object sender, RoutedEventArgs e)
            => editor.FormatSelected(RichEditHelpers.FormattingMode.Strikethrough);

        private void SubscriptButton_Click(object sender, RoutedEventArgs e)
            => editor.FormatSelected(RichEditHelpers.FormattingMode.Subscript);

        private void SuperScriptButton_Click(object sender, RoutedEventArgs e)
            => editor.FormatSelected(RichEditHelpers.FormattingMode.Superscript);

        private void AlignRightButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Right);
            editor_SelectionChanged(sender, e);
        }

        private void AlignCenterButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Center);
            editor_SelectionChanged(sender, e);
        }

        private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Left);
            editor_SelectionChanged(sender, e);
        }

        private void FindBoxHighlightMatches()
        {
            FindBoxRemoveHighlights();

            Color highlightBackgroundColor = (Color)Application.Current.Resources["SystemColorHighlightColor"];
            Color highlightForegroundColor = (Color)Application.Current.Resources["SystemColorHighlightTextColor"];

            string textToFind = findBox.Text;
            if (textToFind != null)
            {
                ITextRange searchRange = editor.Document.GetRange(0, 0);
                while (searchRange.FindText(textToFind, TextConstants.MaxUnitCount, FindOptions.None) > 0)
                {
                    searchRange.CharacterFormat.BackgroundColor = highlightBackgroundColor;
                    searchRange.CharacterFormat.ForegroundColor = highlightForegroundColor;
                }
            }
        }

        private void FindBoxRemoveHighlights()
        {
            ITextRange documentRange = editor.Document.GetRange(0, TextConstants.MaxUnitCount);
            SolidColorBrush defaultBackground = editor.Background as SolidColorBrush;
            SolidColorBrush defaultForeground = editor.Foreground as SolidColorBrush;

            documentRange.CharacterFormat.BackgroundColor = defaultBackground.Color;
            documentRange.CharacterFormat.ForegroundColor = defaultForeground.Color;
        }


        private void ItalicButton_Click(object sender, RoutedEventArgs e)
            => editor.FormatSelected(RichEditHelpers.FormattingMode.Italic);

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
            => editor.FormatSelected(RichEditHelpers.FormattingMode.Underline);

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            FileOpenPicker open = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            open.FileTypeFilter.Add(".rtf");
            open.FileTypeFilter.Add(".txt");

            StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    var reader = DataReader.FromBuffer(buffer);
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    string text = reader.ReadString(buffer.Length);
                    // Load the file into the Document property of the RichEditBox.
                    editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                    editor.Document.GetText(TextGetOptions.UseObjectText, out originalDocText);
                    //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                    (BasePage.Current.Tabs.TabItems[BasePage.Current.Tabs.SelectedIndex] as TabViewItem).Header = file.Name;
                    fileNameWithPath = file.Path;
                }
                saved = true;
                _wasOpen = true;
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
            }
        }

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Open an image file.
            FileOpenPicker open = new()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            
            open.FileTypeFilter.Add(".png");
            open.FileTypeFilter.Add(".jpg");
            open.FileTypeFilter.Add(".jpeg");

            StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                using IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.Read);
                var properties = await file.Properties.GetImagePropertiesAsync();
                int width = (int)properties.Width;
                int height = (int)properties.Height;

                ImageOptionsDialog dialog = new()
                {
                    DefaultWidth = width,
                    DefaultHeight = height
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    editor.Document.Selection.InsertImage((int)dialog.DefaultWidth, (int)dialog.DefaultHeight, 0, VerticalCharacterAlignment.Baseline, string.IsNullOrWhiteSpace(dialog.Tag) ? "Image" : dialog.Tag, randAccStream);
                    return;
                }

                // Insert an image
                editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "Image", randAccStream);
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract the color of the button that was clicked.
            Button clickedColor = (Button)sender;
            var rectangle = (Windows.UI.Xaml.Shapes.Rectangle)clickedColor.Content;
            var color = (rectangle.Fill as SolidColorBrush).Color;

            editor.Document.Selection.CharacterFormat.ForegroundColor = color;

            fontColorButton.Flyout.Hide();
            editor.Focus(FocusState.Keyboard);
        }

        private void AddLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(hyperlinkText.Text))
            {
                if (editor.Document.Selection.Length == 0)
                {
                    editor.Document.Selection.Text = hyperlinkText.Text;
                }
                editor.Document.Selection.Link = $"\"{hyperlinkText.Text}\"";
                editor.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), "#6194c7");
            }
            AddLinkButton.Flyout.Hide();
            editor.Focus(FocusState.Programmatic);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
            => editor.Document.Selection.Copy();

        private void CutButton_Click(object sender, RoutedEventArgs e)
            => editor.Document.Selection.Cut();

        private void PasteButton_Click(object sender, RoutedEventArgs e)
            => editor.Document.Selection.Paste(0);

        private void UndoButton_Click(object sender, RoutedEventArgs e)
            => editor.Document.Undo();

        private void RedoButton_Click(object sender, RoutedEventArgs e)
            => editor.Document.Redo();

        private Task DisplayAboutDialog()
            => new AboutDialog().ShowAsync().AsTask();

        public async Task ShowUnsavedDialog()
        {
            string fileName = (BasePage.Current.Tabs.TabItems[BasePage.Current.Tabs.SelectedIndex] as TabViewItem).Header as string;
            ContentDialog aboutDialog = new()
            {
                Title = "Do you want to save changes to " + fileName + "?",
                Content = "There are unsaved changes, want to save them?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save changes",
                PrimaryButtonStyle = Resources["AccentButtonStyle"] as Style,
                SecondaryButtonText = "No (close app)",
            };

            aboutDialog.CloseButtonClick += (s, e) => BasePage.Current._openDialog = false;

            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                SaveFile(true);
            else if (result == ContentDialogResult.Secondary)
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
            => await DisplayAboutDialog();

        private void FontsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editor.Document.Selection == null || !updateFontFormat)
                return;

            editor.Document.Selection.CharacterFormat.Name = FontsCombo.SelectedValue.ToString();
            editor.Focus(FocusState.Programmatic);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
            => FindBoxHighlightMatches();

        private void editor_TextChanged(object sender, RoutedEventArgs e)
        {
            editor.Document.GetText(TextGetOptions.UseObjectText, out string textStart);

            saved = string.IsNullOrWhiteSpace(textStart) || (_wasOpen && textStart == originalDocText);

            if (!saved) UnsavedTextBlock.Visibility = Visibility.Visible;
            else UnsavedTextBlock.Visibility = Visibility.Collapsed;
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (saved)
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            else
                await ShowUnsavedDialog();
        }

        private void ConfirmColor_Click(object sender, RoutedEventArgs e)
        {
            // Confirm color picker choice and apply color to text
            Color color = myColorPicker.Color;
            editor.Document.Selection.CharacterFormat.ForegroundColor = color;

            // Hide flyout
            colorPickerButton.Flyout.Hide();
        }

        private void CancelColor_Click(object sender, RoutedEventArgs e)
            // Cancel flyout
            => colorPickerButton.Flyout.Hide();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is StorageFile file)
            {
                using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    var reader = DataReader.FromBuffer(buffer);
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    string text = reader.ReadString(buffer.Length);
                    // Load the file into the Document property of the RichEditBox.
                    editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                    editor.Document.GetText(TextGetOptions.UseObjectText, out originalDocText);
                    //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                    fileNameWithPath = file.Path;
                }
                saved = true;
                fileNameWithPath = file.Path;
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
                _wasOpen = true;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
            => (Window.Current.Content as Frame).Navigate(typeof(SettingsPage));

        private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e)
            => FindBoxRemoveHighlights();

        private void ReplaceSelected_Click(object sender, RoutedEventArgs e)
            => editor.Replace(false, replaceBox.Text);

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
            => editor.Replace(true, find: findBox.Text, replace: replaceBox.Text);

        private void FontSizeBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (editor != null && editor.Document.Selection != null)
                editor.ChangeFontSize((float)sender.Value);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Content is not Frame rootFrame)
                return;

            rootFrame.Navigate(typeof(HomePage));
        }

        private void OnKeyboardAcceleratorInvoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case Windows.System.VirtualKey.B:
                    editor.FormatSelected(RichEditHelpers.FormattingMode.Bold);
                    BoldButton.IsChecked = editor.Document.Selection.CharacterFormat.Bold == FormatEffect.On;
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.I:
                    editor.FormatSelected(RichEditHelpers.FormattingMode.Italic);
                    ItalicButton.IsChecked = editor.Document.Selection.CharacterFormat.Italic == FormatEffect.On;
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.U:
                    editor.FormatSelected(RichEditHelpers.FormattingMode.Underline);
                    UnderlineButton.IsChecked = editor.Document.Selection.CharacterFormat.Underline == UnderlineType.Single;
                    args.Handled = true;
                    break;
                case Windows.System.VirtualKey.S:
                    SaveFile(false);
                    break;
            }
        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            BoldButton.IsChecked = editor.Document.Selection.CharacterFormat.Bold == FormatEffect.On;

            ItalicButton.IsChecked = editor.Document.Selection.CharacterFormat.Italic == FormatEffect.On;

            UnderlineButton.IsChecked = editor.Document.Selection.CharacterFormat.Underline != UnderlineType.None &&
                                        editor.Document.Selection.CharacterFormat.Underline != UnderlineType.Undefined;

            StrikethroughButton.IsChecked = editor.Document.Selection.CharacterFormat.Strikethrough == FormatEffect.On;

            SubscriptButton.IsChecked = editor.Document.Selection.CharacterFormat.Subscript == FormatEffect.On;

            SuperscriptButton.IsChecked = editor.Document.Selection.CharacterFormat.Superscript == FormatEffect.On;

            AlignLeftButton.IsChecked = editor.Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Left;

            AlignCenterButton.IsChecked = editor.Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Center;

            AlignRightButton.IsChecked = editor.Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Right;
            
            if (editor.Document.Selection.CharacterFormat.Size > 0)
                // Font size is negative when selection contains multiple font sizes
                FontSizeBox.Value = editor.Document.Selection.CharacterFormat.Size;

            // Prevent accidental font changes when selection contains multiple styles
            updateFontFormat = false;
            FontsCombo.SelectedItem = editor.Document.Selection.CharacterFormat.Name;
            updateFontFormat = true;
        }
    }
}
