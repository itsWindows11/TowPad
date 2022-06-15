using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Rich_Text_Editor.Dialogs
{
    public sealed partial class ImageOptionsDialog : ContentDialog
    {
        public double DefaultWidth { get; set; }
        public double DefaultHeight { get; set; }
        public string Tag { get; private set; }
        
        public ImageOptionsDialog()
        {
            InitializeComponent();

            Loaded += ImageOptionsDialog_Loaded;
        }

        private void ImageOptionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            WidthBox.Value = DefaultWidth;
            HeightBox.Value = DefaultHeight;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DefaultWidth = WidthBox.Value;
            DefaultHeight = HeightBox.Value;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
