using System.Windows;
using QuinkGest.Controllers;

namespace QuinkGest.Views
{
    public partial class AlertesStockWindow : Window
    {
        private readonly ProduitController _controller = new ProduitController();

        public AlertesStockWindow()
        {
            InitializeComponent();
            GrilleAlertes.ItemsSource = _controller.ChargerProduitsEnAlerte();
        }
    }
}
