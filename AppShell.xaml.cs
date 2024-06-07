using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Chat
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            BindingContext = this; // Thiết lập BindingContext cho Shell
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public void UpdateUserName(string username)
        {
            FullName = username;
        }
    }
    public class AppShellViewModel : INotifyPropertyChanged
    {
        private string _fullName;

        public string FullName
        {
            get => _fullName;
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    OnPropertyChanged();
                }
            }
        }

        public AppShellViewModel()
        {
            // Load the user information from Preferences or API
            FullName = Preferences.Get("FullName", "Guest");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
