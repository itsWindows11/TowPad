using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Rich_Text_Editor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BasePage : Page
    {
        public static BasePage Current;

        public bool _openDialog;

        public BasePage()
        {
            InitializeComponent();

            Current = this;

            if (Tabs.TabItems.Count == 0)
                CreateNewTab(false);

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            Window.Current.SetTitleBar(CustomDragRegion);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Keep tabs open if the root frame navigated elsewhere
            NavigationCacheMode = NavigationCacheMode.Enabled;

            (CompactOverlayBtn.Content as FontIcon).Glyph = ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay ? "\uEE49" : "\uEE47";

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            if (_openDialog)
            {
                e.Handled = true;
                return;
            }
            
            _openDialog = true;

            if (Tabs.TabItems.Count == 1)
            {
                if (!(((Tabs.SelectedItem as TabViewItem).Content as Frame).Content as MainPage).saved)
                {
                    e.Handled = true;
                    (((Tabs.SelectedItem as TabViewItem).Content as Frame).Content as MainPage).ShowUnsavedDialog();
                }
            } else if (Tabs.TabItems.Count > 1)
            {
                int unsavedItemsCount = 0;
                
                foreach (TabViewItem item in Tabs.TabItems)
                {
                    if (!((item.Content as Frame).Content as MainPage).saved)
                    {
                        unsavedItemsCount++;
                    }
                }

                if (unsavedItemsCount > 0)
                {
                    e.Handled = true;

                    ContentDialog confirmationDialog = new()
                    {
                        Title = "Save",
                        Content = $"There are unsaved changes to {(unsavedItemsCount > 1 ? "multiple documents" : "a document")}, want to save them?",
                        CloseButtonText = "Cancel and save changes",
                        CloseButtonStyle = Resources["AccentButtonStyle"] as Style,
                        PrimaryButtonText = "Close app",
                    };

                    // Allow the dialog to be opened again
                    confirmationDialog.CloseButtonClick += (s, e) => _openDialog = false; 

                    ContentDialogResult result = await confirmationDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                        await ApplicationView.GetForCurrentView().TryConsolidateAsync();
                }
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            CustomDragRegion.Height = ShellTitlebarInset.Height = sender.Height;
        }

        public void TabView_AddTabButtonClick(TabView sender, object args)
        {
            CreateNewTab();
        }

        public void CreateNewTab(bool selectTab = true, string tabName = null, object parameter = null)
        {
            TabViewItem newTab = new();

            Frame frame = new();

            newTab.Content = frame;
            newTab.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() 
            {
                Symbol = Symbol.Document
            };

            newTab.Header = tabName ?? $"Text document {Tabs.TabItems.Count + 1}";

            newTab.Style = Resources["TabViewItemStyle1"] as Style;

            frame.Navigate(typeof(MainPage), parameter);

            Tabs.TabItems.Add(newTab);

            if (selectTab)
                Tabs.SelectedItem = newTab;
        }

        public async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);

            if (sender.TabItems.Count < 1)
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
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

        public void OpenFilesWithArgs(FileActivatedEventArgs args)
        {
            foreach (var item in args.Files)
            {
                StorageFile file = item as StorageFile;

                CreateNewTab(tabName: file.Name, parameter: file);
            }
        }

        #region TabView keyboard accelerators
        private void NavigateToNumberedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            int tabToSelect = 0;

            switch (sender.Key)
            {
                case Windows.System.VirtualKey.Number1:
                case Windows.System.VirtualKey.NumberPad1:
                    tabToSelect = 0;
                    break;
                case Windows.System.VirtualKey.Number2:
                case Windows.System.VirtualKey.NumberPad2:
                    tabToSelect = 1;
                    break;
                case Windows.System.VirtualKey.Number3:
                case Windows.System.VirtualKey.NumberPad3:
                    tabToSelect = 2;
                    break;
                case Windows.System.VirtualKey.Number4:
                case Windows.System.VirtualKey.NumberPad4:
                    tabToSelect = 3;
                    break;
                case Windows.System.VirtualKey.Number5:
                case Windows.System.VirtualKey.NumberPad5:
                    tabToSelect = 4;
                    break;
                case Windows.System.VirtualKey.Number6:
                case Windows.System.VirtualKey.NumberPad6:
                    tabToSelect = 5;
                    break;
                case Windows.System.VirtualKey.Number7:
                case Windows.System.VirtualKey.NumberPad7:
                    tabToSelect = 6;
                    break;
                case Windows.System.VirtualKey.Number8:
                case Windows.System.VirtualKey.NumberPad8:
                    tabToSelect = 7;
                    break;
                case Windows.System.VirtualKey.Number9:
                case Windows.System.VirtualKey.NumberPad9:
                    // Select the last tab
                    tabToSelect = Tabs.TabItems.Count - 1;
                    break;
            }

            // Only select the tab if it is in the list
            if (tabToSelect < Tabs.TabItems.Count)
            {
                Tabs.SelectedIndex = tabToSelect;
            }
        }

        #endregion
    }
}
