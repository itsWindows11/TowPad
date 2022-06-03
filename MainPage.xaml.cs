using Rich_Text_Editor.Pages;
using Rich_Text_Editor.ViewModels;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Rich_Text_Editor
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public ObservableCollection<TabItem> Tabs;

        public TabItem SelectedTab;

        public MainPage()
        {
            InitializeComponent();

            Current = this;

            Tabs = new();

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;

            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);

            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

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

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;

            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
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
                    preferences.CustomSize = new Windows.Foundation.Size(500, 500);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);
                    (button.Content as FontIcon).Glyph = "\uEE47";
                    button.Margin = new(10, 5, 70, 10);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.File)
                {
                    foreach (var file in (args as FileActivatedEventArgs).Files)
                    {
                        TabItem item = new()
                        {
                            Title = file.Name,
                            Icon = "&#xE130;",
                            TargetPage = typeof(EditorPage),
                            Saved = true
                        };

                        Tabs.Add(item);

                        CurrentTabFrame.Navigate(item.TargetPage, item);
                    }
                }
            }
        }
    }
}
