using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue de vente au comptoir : composition du panier puis validation.
    /// Ne fait aucun accès SQLite direct, tout passe par VenteController.
    /// </summary>
    public partial class VenteWindow : Window
    {
        private readonly VenteController _controller = new VenteController();
        private readonly ProduitController _produitController = new ProduitController();
        private readonly ClientController _clientController = new ClientController();
        private readonly Utilisateur _utilisateurConnecte;

        public VenteWindow(Utilisateur utilisateur)
        {
            InitializeComponent();
            _utilisateurConnecte = utilisateur;
            ChampProduit.ItemsSource = _produitController.ChargerTousLesProduits();
            ChampClient.ItemsSource = _clientController.ChargerTousLesClients();
            RafraichirTotal();
        }

        private void RafraichirTotal()
        {
            GrillePanier.ItemsSource = null;
            GrillePanier.ItemsSource = _controller.Panier;
            TexteTotal.Text = $"Total : {_controller.Total:N0} F";
        }

        private void BoutonAjouterPanier_Click(object sender, RoutedEventArgs e)
        {
            if (ChampProduit.SelectedItem is not Produit produit)
            {
                MessageBox.Show("Veuillez sélectionner un produit", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var quantite = double.TryParse(ChampQuantite.Text, out var q) ? q : 0;
            var (succes, message) = _controller.AjouterAuPanier(produit, quantite);

            if (!succes)
                MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);

            RafraichirTotal();
            ChampQuantite.Clear();
        }

        private void BoutonValider_Click(object sender, RoutedEventArgs e)
        {
            var modePaiement = (ChampModePaiement.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Espèces";

            var clientSelectionne = ChampClient.SelectedItem as Client;
            var clientNom = clientSelectionne?.Nom ?? ChampClient.Text;

            var (succes, message, venteId) = _controller.ValiderVente(
                clientNom, clientSelectionne?.Id, modePaiement, _utilisateurConnecte.NomComplet);

            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                RafraichirTotal();
                Close();
            }
        }
    }
}
