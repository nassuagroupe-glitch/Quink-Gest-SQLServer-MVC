using System.Windows;
using QuinkGest.Controllers;

namespace QuinkGest.Views
{
    public partial class HistoriqueVentesWindow : Window
    {
        private readonly VenteController _controller = new VenteController();

        public HistoriqueVentesWindow()
        {
            InitializeComponent();
            var ventes = _controller.HistoriqueDuJour();
            GrilleVentes.ItemsSource = ventes;
            TexteTotal.Text = $"Total du jour : {_controller.TotalVenduAujourdhui():N0} F ({ventes.Count} vente(s))";
        }
    }
}
