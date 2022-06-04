using System;
using System.ComponentModel;

namespace Rich_Text_Editor.ViewModels
{
    public class TabItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Type TargetPage { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new(nameof(Title)));
            }
        }

        private string _icon;
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                PropertyChanged?.Invoke(this, new(nameof(Icon)));
            }
        }

        private bool _saved;

        public bool Saved
        {
            get => _saved;
            set
            {
                _saved = value;
                PropertyChanged?.Invoke(this, new(nameof(Saved)));
            }
        }

        public object Content { get; set; }
    }
}
