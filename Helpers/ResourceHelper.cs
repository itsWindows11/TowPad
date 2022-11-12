using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;

namespace Rich_Text_Editor.Helpers
{
    /// <summary>
    /// A markup extension that gets a string from a resource.
    /// Can also be used outside of XAML through the static
    /// <see cref="GetString(string)"/> method.
    /// </summary>
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public sealed class ResourceHelper : MarkupExtension
    {
        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

        /// <summary>
        /// Resource identifier to get the string from.
        /// </summary>
        public string Name { get; set; }

        protected override object ProvideValue()
            => loader.GetString(Name);

        /// <summary>
        /// Gets the string from the resource with the provided
        /// identifier.
        /// </summary>
        public static string GetString(string resource)
            => loader.GetString(resource);
    }
}
