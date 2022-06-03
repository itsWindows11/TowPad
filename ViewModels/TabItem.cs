using System;

namespace Rich_Text_Editor.ViewModels
{
    public class TabItem
    {
        public Type TargetPage { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public bool Saved { get; set; }
        public object Content { get; set; }
    }
}
