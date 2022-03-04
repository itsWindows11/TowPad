using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;

namespace Rich_Text_Editor.Helpers
{
    public static class RichEditHelpers
    {
        public static void ChangeFontSize(this RichEditBox editor, float size)
        {
            ITextSelection selectedText = editor.Document.Selection;
            ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
            charFormatting.Size = size;
            selectedText.CharacterFormat = charFormatting;
        }

        public static void Replace(this RichEditBox editor, bool replaceAll, string replace, string find = "")
        {
            if (replaceAll)
            {
                editor.Document.GetText(TextGetOptions.FormatRtf, out string value);
                if (!(string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(find) && string.IsNullOrWhiteSpace(replace)))
                {
                    editor.Document.SetText(TextSetOptions.FormatRtf, value.Replace(find, replace));
                }
            } else
            {
                editor.Document.Selection.SetText(TextSetOptions.None, replace);
            }
        }

        public static void AlignSelectedTo(this RichEditBox editor, AlignMode mode)
        {
            ITextSelection selectedText = editor.Document.Selection;

            if (selectedText != null)
            {
                switch (mode)
                {
                    case AlignMode.Left:
                        selectedText.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                        break;
                    case AlignMode.Center:
                        selectedText.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                        break;
                    case AlignMode.Right:
                        selectedText.ParagraphFormat.Alignment = ParagraphAlignment.Right;
                        break;
                }
            }
        }

        public static void FormatSelected(this RichEditBox editor, FormattingMode mode)
        {
            ITextSelection selectedText = editor.Document.Selection;

            if (selectedText != null)
            {
                ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                switch (mode)
                {
                    case FormattingMode.Bold:
                        charFormatting.Bold = FormatEffect.Toggle;
                        break;
                    case FormattingMode.Italic:
                        charFormatting.Italic = FormatEffect.Toggle;
                        break;
                    case FormattingMode.Underline:
                        if (charFormatting.Underline == UnderlineType.None)
                        {
                            charFormatting.Underline = UnderlineType.Single;
                        }
                        else
                        {
                            charFormatting.Underline = UnderlineType.None;
                        }
                        break;
                    case FormattingMode.Strikethrough:
                        charFormatting.Strikethrough = FormatEffect.Toggle;
                        break;
                    case FormattingMode.Subscript:
                        charFormatting.Subscript = FormatEffect.Toggle;
                        break;
                    case FormattingMode.Superscript:
                        charFormatting.Superscript = FormatEffect.Toggle;
                        break;
                }
                selectedText.CharacterFormat = charFormatting;
            }
        }

        public enum AlignMode
        {
            Left,
            Right,
            Center
        }

        public enum FormattingMode
        {
            Bold,
            Italic,
            Strikethrough,
            Underline,
            Subscript,
            Superscript
        }
    }
}
