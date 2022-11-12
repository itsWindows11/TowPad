using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Rich_Text_Editor.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        public SettingsViewModel() { }

        public List<string> Fonts
            => CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();

        #region Appearance
        public int DocumentViewPadding
        {
            get => Get("Appearance", nameof(DocumentViewPadding), 11);
            set => Set("Appearance", nameof(DocumentViewPadding), value);
        }

        public string DefaultFont
        {
            get => Get("Appearance", nameof(DefaultFont), "Calibri");
            set => Set("Appearance", nameof(DefaultFont), value);
        }

        // Modes:
        // 0. No wrap
        // 1. Wrap
        // 2. Wrap whole words

        public int TextWrapping
        {
            get => Get("Appearance", nameof(TextWrapping), 0);
            set => Set("Appearance", nameof(TextWrapping), value);
        }

        // Modes:
        // 0. Light
        // 1. Dark
        // 2. Default

        public int Theme
        {
            get => Get("Appearance", nameof(Theme), 2);
            set
            {
                Set("Appearance", nameof(Theme), value);
                switch (value)
                {
                    case 0:
                        (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Light;
                        break;
                    case 1:
                        (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Dark;
                        break;
                    case 2:
                        (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }
        #endregion

        #region Setting/getting settings
        /// <summary>
        /// Gets an app setting.
        /// </summary>
        /// <param name="store">Setting store name.</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="defaultValue">Default setting value.</param>
        /// <returns>App setting value.</returns>
        /// <remarks>If the store parameter is "Local", a local setting will be returned.</remarks>
        protected T Get<T>(string store, string setting, T defaultValue)
        {
            // If store == "Local", get a local setting
            if (store == "Local")
            {
                // Get app settings
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                // Check if the setting exists
                if (localSettings.Values[setting] == null)
                {
                    localSettings.Values[setting] = defaultValue;
                }

                object val = localSettings.Values[setting];

                // Return the setting if type matches
                if (val is not T)
                {
                    throw new ArgumentException("Type mismatch for \"" + setting + "\" in local store. Got " + val.GetType());
                }
                return (T)val;
            }

            // Get desired composite value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)roamingSettings.Values[store];

            // If the store exists, check if the setting does as well
            composite ??= new ApplicationDataCompositeValue();

            if (composite[setting] == null)
            {
                composite[setting] = defaultValue;
                roamingSettings.Values[store] = composite;
            }

            object value = composite[setting];

            // Return the setting if type matches
            if (value is not T)
            {
                throw new ArgumentException("Type mismatch for \"" + setting + "\" in store \"" + store + "\". Current type is " + value.GetType());
            }
            return (T)value;
        }

        /// <summary>
        /// Sets an app setting.
        /// </summary>
        /// <param name="store">Setting store name.</param>
        /// <param name="setting">Setting name.</param>
        /// <param name="newValue">New setting value.</param>
        /// <remarks>If the store parameter is "Local", a local setting will be set.</remarks>
        protected void Set<T>(string store, string setting, T newValue)
        {
            // Try to get the setting, if types don't match, it'll throw an exception
            _ = Get(store, setting, newValue);

            // If store == "Local", set a local setting
            if (store == "Local")
            {
                // Get app settings
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values[setting] = newValue;
                return;
            }

            // Get desired composite value
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)roamingSettings.Values[store];

            // Store doesn't exist, create it
            composite ??= new ApplicationDataCompositeValue();

            // Set the setting to the desired value
            composite[setting] = newValue;
            roamingSettings.Values[store] = composite;

            OnPropertyChanged(setting);
        }
        #endregion
    }
}