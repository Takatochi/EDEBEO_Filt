using System.Windows;

namespace EdeboCsvProcessor.Wpf
{
    public partial class PaymentWindow : Window
    {
        public PaymentWindow()
        {
            InitializeComponent();
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дякую! 1,000,000 ₴ успішно списано з вашої картки (жарт 😉). Гарного дня!", 
                            "Оплата успішна", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
