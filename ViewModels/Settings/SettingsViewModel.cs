using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using System.Linq;

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
        #endregion
    }
}