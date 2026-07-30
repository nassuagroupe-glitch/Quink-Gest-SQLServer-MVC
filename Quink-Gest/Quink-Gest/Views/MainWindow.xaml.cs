using System.Windows;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue principale : tableau de bord donnant accès aux modules
    /// (vente, produits/stock, fournisseurs, historique, alertes).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Utilisateur _utilisateurConnecte;
        private readonly VenteController _venteController = new VenteController();

        public MainWindow(Utilisateur utilisateur)
        {
            InitializeComponent();
            _utilisateurConnecte = utilisateur;
            TexteBienvenue.Text = $"Bienvenue, {utilisateur.NomComplet} ({utilisateur.Role})";
            TexteVenteJour.Text = $"Total vendu aujourd'hui : {_venteController.TotalVenduAujourdhui():N0} F";
        }

        private void BoutonVente_Click(object sender, RoutedEventArgs e) =>
            new VenteWindow(_utilisateurConnecte).Show();

        private void BoutonProduits_Click(object sender, RoutedEventArgs e) => new ProduitsWindow().Show();
        private void BoutonFournisseurs_Click(object sender, RoutedEventArgs e) => new FournisseursWindow().Show();
        private void BoutonClients_Click(object sender, RoutedEventArgs e) => new ClientsWindow().Show();
        private void BoutonHistorique_Click(object sender, RoutedEventArgs e) => new HistoriqueVentesWindow().Show();
        private void BoutonDepenses_Click(object sender, RoutedEventArgs e) => new DepensesWindow().Show();
        private void BoutonAlertes_Click(object sender, RoutedEventArgs e) => new AlertesStockWindow().Show();
        private void BoutonParametres_Click(object sender, RoutedEventArgs e) => new ParametresWindow(_utilisateurConnecte).Show();
        private void BoutonEtablissement_Click(object sender, RoutedEventArgs e) => new EtablissementWindow().Show();

        private void BoutonRapportCaisse_Click(object sender, RoutedEventArgs e) => new RapportCaisseWindow().Show();

        private void BoutonCredit_Click(object sender, RoutedEventArgs e) => new CreditWindow().Show();

        private void BoutonRelances_Click(object sender, RoutedEventArgs e) => new RelancesWindow().Show();

        private void BoutonDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}
