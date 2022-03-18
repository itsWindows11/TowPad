using Rich_Text_Editor.Helpers;
using Rich_Text_Editor.Views;
using Rich_Text_Editor.Views.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
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
        private bool saved = true;
        private bool _wasOpen = false;
        private string appTitleStr = Strings.Resources.AppName;
        private string fileNameWithPath = "";

        public MainPage()
        {
            InitializeComponent();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            Window.Current.Activated += Current_Activated;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;

            NavigationCacheMode = NavigationCacheMode.Required;

            (CompactOverlayBtn.Content as FontIcon).Glyph = ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay ? "\uEE49" : "\uEE47";
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        // Update the TitleBar based on the inactive/active state of the app
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            SolidColorBrush defaultForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            SolidColorBrush inactiveForegroundBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorDisabledBrush"];

            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                AppTitle.Foreground = inactiveForegroundBrush;
            }
            else
            {
                AppTitle.Foreground = defaultForegroundBrush;
            }
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (!saved) { e.Handled = true; ShowUnsavedDialog(); }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(false);
        }

        private async void SaveFile(bool isCopy)
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            if (isCopy || fileName == "Untitled")
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

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
                    // write to file
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        if (file.Name.EndsWith(".txt"))
                        {
                            editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.None, randAccStream);
                        }
                        else
                        {
                            editor.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
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
                    fileNameWithPath = file.Path;
                    AppTitle.Text = file.Name + " - " + appTitleStr;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
            else if (!isCopy || fileName != "Untitled")
            {
                string path = fileNameWithPath.Replace("\\" + fileName, "");
                try
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync("CurrentlyOpenFile");
                    if (file != null)
                    {
                        // Prevent updates to the remote version of the file until we
                        // finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(file);
                        // write to file
                        using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            if (file.Name.EndsWith(".txt"))
                            {
                                editor.Document.SaveToStream(TextGetOptions.None, randAccStream);
                            }
                            else
                            {
                                editor.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
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
                        AppTitle.Text = file.Name + " - " + appTitleStr;
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
                        newAppView.Title = $"Untitled - {Strings.Resources.AppName}";

                        var frame = new Frame();
                        frame.Navigate(typeof(MainPage));
                        newWindow.Content = frame;
                        newWindow.Activate();

                        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newAppView.Id, 
                            ViewSizePreference.UseMinimum, currentAV.Id, ViewSizePreference.UseMinimum);
                  });
        }

        private void StrikethoughButton_Click(object sender, RoutedEventArgs e)
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Strikethrough);
        }

        private void SubscriptButton_Click(object sender, RoutedEventArgs e)
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Subscript);
        }

        private void SuperScriptButton_Click(object sender, RoutedEventArgs e)
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Superscript);
        }

        private void AlignRightButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Right);
        }

        private void AlignCenterButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Center);
        }

        private void AlignLeftButton_Click(object sender, RoutedEventArgs e)
        {
            editor.AlignSelectedTo(RichEditHelpers.AlignMode.Left);
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
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Italic);
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            editor.FormatSelected(RichEditHelpers.FormattingMode.Underline);
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a text file.
            FileOpenPicker open = new();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
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
                    //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                    AppTitle.Text = file.Name + " - " + appTitleStr;
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
            FileOpenPicker open = new();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
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

                // Load the file into the Document property of the RichEditBox.
                editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "img", randAccStream);
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
            editor.Document.Selection.Link = $"\"{hyperlinkText.Text}\"";
            editor.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), "#6194c7");
            AddLinkButton.Flyout.Hide();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Copy();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Cut();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Selection.Paste(0);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Redo();
        }

        private async Task DisplayAboutDialog()
        {
            ContentDialog aboutDialog = new()
            {
                Title = appTitleStr,
                Content = $"Version {typeof(App).GetTypeInfo().Assembly.GetName().Version}\n\n© 2022",
                CloseButtonText = "OK"
            };

            await aboutDialog.ShowAsync();
        }

        private async Task ShowUnsavedDialog()
        {
            string fileName = AppTitle.Text.Replace(" - " + appTitleStr, "");
            ContentDialog aboutDialog = new()
            {
                Title = "Do you want to save changes to " + fileName + "?",
                Content = "There are unsaved changes, want to save them?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save changes",
                SecondaryButtonText = "No (close app)",
            };

            ContentDialogResult result = await aboutDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveFile(true);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            await DisplayAboutDialog();
        }

        private void FontsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editor.Document.Selection != null)
            {
                editor.Document.Selection.CharacterFormat.Name = FontsCombo.SelectedValue.ToString();
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindBoxHighlightMatches();
        }

        private void editor_TextChanged(object sender, RoutedEventArgs e)
        {
            editor.Document.GetText(TextGetOptions.UseObjectText, out string textStart);

            if (textStart == "" || string.IsNullOrWhiteSpace(textStart) || _wasOpen)
            {
                saved = true;
            }
            else
            {
                saved = false;
            }

            if (!saved) UnsavedTextBlock.Visibility = Visibility.Visible;
            else UnsavedTextBlock.Visibility = Visibility.Collapsed;
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (saved)
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
            else await ShowUnsavedDialog();
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
        {
            // Cancel flyout
            colorPickerButton.Flyout.Hide();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.File)
                {
                    var fileArgs = args as FileActivatedEventArgs;
                    StorageFile file = (StorageFile)fileArgs.Files[0];
                    using (IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        IBuffer buffer = await FileIO.ReadBufferAsync(file);
                        var reader = DataReader.FromBuffer(buffer);
                        reader.UnicodeEncoding =UnicodeEncoding.Utf8;
                        string text = reader.ReadString(buffer.Length);
                        // Load the file into the Document property of the RichEditBox.
                        editor.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                        //editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                        AppTitle.Text = file.Name + " - " + appTitleStr;
                        fileNameWithPath = file.Path;
                    }
                    saved = true;
                    fileNameWithPath = file.Path;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("CurrentlyOpenFile", file);
                    _wasOpen = true;
                }
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            /*SettingsDialog dlg = new(editor, FontsCombo, this);
            await dlg.ShowAsync();*/

            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(SettingsPage));
            }
        }

        private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e)
        {
            FindBoxRemoveHighlights();
        }

        private void ReplaceSelected_Click(object sender, RoutedEventArgs e)
        {
            editor.Replace(false, replaceBox.Text);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            editor.Replace(true, find: findBox.Text, replace: replaceBox.Text);
        }

        private void FontSizeBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (editor != null && editor.Document.Selection != null)
            {
                editor.ChangeFontSize((float)sender.Value);
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(HomePage));
            }
        }

        private async void CompactOverlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                {
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                    (button.Content as FontIcon).Glyph = "\uEE49";
                    button.Margin = new(10, 5, 195, 10);
                }
                else
                {
                    ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                    preferences.CustomSize = new Windows.Foundation.Size(400, 400);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);
                    (button.Content as FontIcon).Glyph = "\uEE47";
                    button.Margin = new(10, 5, 70, 10);
                }
            }
        }
    }
}
