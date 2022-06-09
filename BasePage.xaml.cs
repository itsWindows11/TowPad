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
    }
}
