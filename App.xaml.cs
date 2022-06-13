using Microsoft.Toolkit.Uwp.UI.Helpers;
using Rich_Text_Editor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Rich_Text_Editor
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>

        public static ResourceLoader Resources { get; private set; }
        public static SettingsViewModel SViewModel { get; private set; }
        public static List<string> Tips { get; private set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            Resources = ResourceLoader.GetForViewIndependentUse();
            SViewModel = new();
            Tips = new();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (Window.Current.Content is not Frame rootFrame)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                switch (SViewModel.Theme)
                {
                    case 0:
                        rootFrame.RequestedTheme = ElementTheme.Light;
                        break;
                    case 1:
                        rootFrame.RequestedTheme = ElementTheme.Dark;
                        break;
                    case 2:
                        rootFrame.RequestedTheme = ElementTheme.Default;
                        break;
                }

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!e.PrelaunchActivated)
            {
                CoreApplication.EnablePrelaunch(true);
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(BasePage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e?.SuspendingOperation?.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral?.Complete();
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            // TODO: Handle file activation
            // The number of files received is args.Files.Size
            // The name of the first file is args.Files[0].Name
            if (Window.Current.Content is not Frame)
            {
                Frame rootFrame = new();

                rootFrame.Navigate(typeof(BasePage));
                Window.Current.Content = rootFrame;
                Window.Current.Activate();

                BasePage.Current.Tabs.TabItems.RemoveAt(0);
            }

            if (BasePage.Current != null)
                BasePage.Current.OpenFilesWithArgs(args);
        }
    }
}
