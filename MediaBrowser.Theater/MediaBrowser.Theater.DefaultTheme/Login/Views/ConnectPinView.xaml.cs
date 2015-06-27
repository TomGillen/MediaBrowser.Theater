using System.Windows;
using System.Windows.Controls;

namespace MediaBrowser.Theater.DefaultTheme.Login.Views
{
    /// <summary>
    ///     Interaction logic for ConnectPinView.xaml
    /// </summary>
    public partial class ConnectPinView : UserControl
    {
        public ConnectPinView()
        {
            InitializeComponent();

            Loaded += ConnectPinView_Loaded;
        }

        private void ConnectPinView_Loaded(object sender, RoutedEventArgs e)
        {
            SelectServerButton.Focus();
        }
    }
}