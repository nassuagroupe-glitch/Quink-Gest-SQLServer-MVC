using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue de gestion du catalogue / stock : ajout de produits
    /// et réapprovisionnement. Ne fait aucun accès SQLite direct.
    /// </summary>
    public partial class ProduitsWindow : Window
    {
        private readonly ProduitController _controller = new ProduitController();
        private readonly FournisseurController _fournisseurController = new FournisseurController();
        private Produit? _produitSelectionne;

        public ProduitsWindow()
        {
            InitializeComponent();
            ChargerProduits();
            ChampFournisseurReappro.ItemsSource = _fournisseurController.ChargerTousLesFournisseurs();
        }

        private void ChargerProduits()
        {
            var produits = _controller.ChargerTousLesProduits();
            GrilleProduits.ItemsSource = produits;
            ChampProduitReappro.ItemsSource = produits;
        }

        private void BoutonAjouter_Click(object sender, RoutedEventArgs e)
        {
            var uniteItem = ChampUnite.SelectedItem as ComboBoxItem;

            var produit = new Produit
            {
                Nom = ChampNom.Text,
                Reference = ChampReference.Text,
                Categorie = ChampCategorie.Text,
                Unite = uniteItem?.Content?.ToString() ?? "piece",
                PrixAchat = double.TryParse(ChampPrixAchat.Text, out var pa) ? pa : 0,
                PrixVente = double.TryParse(ChampPrixVente.Text, out var pv) ? pv : 0,
                QuantiteStock = double.TryParse(ChampStockInitial.Text, out var stock) ? stock : 0,
                SeuilAlerte = double.TryParse(ChampSeuilAlerte.Text, out var seuil) ? seuil : 5
            };

            var (succes, message) = _controller.AjouterProduit(produit);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerProduits();
                ViderFormulaire();
            }
        }

        private void GrilleProduits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GrilleProduits.SelectedItem is not Produit produit)
            {
                _produitSelectionne = null;
                return;
            }

            _produitSelectionne = produit;
            GroupeFormulaire.Header = $"Modifier « {produit.Nom} »";
            ChampNom.Text = produit.Nom;
            ChampReference.Text = produit.Reference;
            ChampCategorie.Text = produit.Categorie;
            ChampPrixAchat.Text = produit.PrixAchat.ToString();
            ChampPrixVente.Text = produit.PrixVente.ToString();
            ChampStockInitial.Text = produit.QuantiteStock.ToString();
            ChampSeuilAlerte.Text = produit.SeuilAlerte.ToString();

            foreach (ComboBoxItem item in ChampUnite.Items)
            {
                if (item.Content?.ToString() == produit.Unite)
                {
                    ChampUnite.SelectedItem = item;
                    break;
                }
            }

            BoutonAjouter.IsEnabled = false;
            BoutonModifier.IsEnabled = true;
            BoutonSupprimer.IsEnabled = true;
            BoutonNouveau.IsEnabled = true;
            ChampStockInitial.IsEnabled = false;
        }

        private void BoutonModifier_Click(object sender, RoutedEventArgs e)
        {
            if (_produitSelectionne == null) return;

            var uniteItem = ChampUnite.SelectedItem as ComboBoxItem;

            var produit = new Produit
            {
                Id = _produitSelectionne.Id,
                Nom = ChampNom.Text,
                Reference = ChampReference.Text,
                Categorie = ChampCategorie.Text,
                Unite = uniteItem?.Content?.ToString() ?? "piece",
                PrixAchat = double.TryParse(ChampPrixAchat.Text, out var pa) ? pa : 0,
                PrixVente = double.TryParse(ChampPrixVente.Text, out var pv) ? pv : 0,
                SeuilAlerte = double.TryParse(ChampSeuilAlerte.Text, out var seuil) ? seuil : 5,
                FournisseurId = _produitSelectionne.FournisseurId
            };

            var (succes, message) = _controller.ModifierProduit(produit);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerProduits();
                ViderFormulaire();
            }
        }

        private void BoutonSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_produitSelectionne == null) return;

            var confirmation = MessageBox.Show(
                $"Supprimer définitivement « {_produitSelectionne.Nom} » ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            var (succes, message) = _controller.SupprimerProduit(_produitSelectionne.Id);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerProduits();
                ViderFormulaire();
            }
        }

        private void BoutonNouveau_Click(object sender, RoutedEventArgs e) => ViderFormulaire();

        private void ViderFormulaire()
        {
            _produitSelectionne = null;
            GrilleProduits.SelectedItem = null;
            GroupeFormulaire.Header = "Nouveau produit";

            ChampNom.Clear();
            ChampReference.Clear();
            ChampCategorie.Clear();
            ChampPrixAchat.Clear();
            ChampPrixVente.Clear();
            ChampStockInitial.Clear();
            ChampSeuilAlerte.Clear();
            ChampUnite.SelectedIndex = 0;
            ChampStockInitial.IsEnabled = true;

            BoutonAjouter.IsEnabled = true;
            BoutonModifier.IsEnabled = false;
            BoutonSupprimer.IsEnabled = false;
            BoutonNouveau.IsEnabled = false;
        }

        private void BoutonReapprovisionner_Click(object sender, RoutedEventArgs e)
        {
            if (ChampProduitReappro.SelectedItem is not Produit produit)
            {
                MessageBox.Show("Veuillez sélectionner un produit", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fournisseur = ChampFournisseurReappro.SelectedItem as Fournisseur;
            var quantite = double.TryParse(ChampQuantiteReappro.Text, out var q) ? q : 0;

            var (succes, message) = _controller.Reapprovisionner(produit.Id, quantite, fournisseur?.Id ?? 0);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerProduits();
                ChampQuantiteReappro.Clear();
            }
        }
    }
}
