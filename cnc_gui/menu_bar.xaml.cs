using System.Windows;

namespace cnc_gui
{
    /// <summary>
    /// menu_bar.xaml 的互動邏輯
    /// </summary>
    public partial class menu_bar : Window
    {
        public menu_bar()
        {
            InitializeComponent();
            Main_menu_bar.Content = new home();

        }

        private void Button_Click_home(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new home();
        }

        private void Button_Click_cam(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new cam();
        }

        private void Button_Click_energy(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new energy();
        }

        private void Button_Click_setting(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new setting();
        }

        private void Button_Click_clear(object sender, RoutedEventArgs e)
        {
            Main_menu_bar.Content = new clear();
        }

    }
}
