using Rich_Text_Editor.Pages;
using Rich_Text_Editor.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Rich_Text_Editor
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public ObservableCollection<TabItem> Tabs;

        public TabItem SelectedTab => (TabItem)TabView.SelectedItem;

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

            Window.Current.SetTitleBar(CustomDragRegion);

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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is IActivatedEventArgs args)
            {
                if (args.Kind == ActivationKind.File)
                {
                    foreach (var file in (args as FileActivatedEventArgs).Files)
                    {
                        var editorPage = new EditorPage(file as StorageFile);
                        TabItem item = new()
                        {
                            Title = file.Name,
                            Icon = "\uE130",
                            TargetPage = typeof(EditorPage),
                            Saved = true,
                            Content = editorPage
                        };

                        Tabs.Add(item);

                        TabView.SelectedItem = item;
                    }
                }
            }
        }

        private void TabView_AddTabButtonClick(Microsoft.UI.Xaml.Controls.TabView sender, object args)
        {
            TabItem item = new()
            {
                Title = $"Text document {Tabs.Count + 1}",
                Icon = "\uE130",
                TargetPage = typeof(EditorPage),
                Saved = true,
                Content = new EditorPage()
            };

            Tabs.Add(item);

            sender.SelectedItem = item;
        }

        private async void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            var item = args.Tab.DataContext as TabItem;

            ContentDialogResult result = ContentDialogResult.None;

            if (!item.Saved)
            {
                result = await ShowUnsavedDialog();
            }

            Tabs.Remove(item);

            if (Tabs.Count == 0 && (result != ContentDialogResult.Secondary))
            {
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            }
        }

        private async Task<ContentDialogResult> ShowUnsavedDialog()
        {
            string fileName = SelectedTab.Title.Replace(" - " + SelectedTab.Title, "");
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
                (SelectedTab.Content as EditorPage).SaveFile(SelectedTab.Saved);
            }
            else if (result == ContentDialogResult.Secondary)
            {
                if (Tabs.Count == 0)
                {
                    await ApplicationView.GetForCurrentView().TryConsolidateAsync();
                }
            }

            return result;
        }
    }
}
