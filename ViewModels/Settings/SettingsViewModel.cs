using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Rich_Text_Editor.ViewModels
{
    public class SettingsViewModel : SettingsManager
    {
        public SettingsViewModel() { }

        public List<string> Fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
            }
        }

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
    }
}