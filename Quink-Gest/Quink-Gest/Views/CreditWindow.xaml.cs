using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Suivi des ventes à crédit non soldées et enregistrement des paiements.
    /// </summary>
    public partial class CreditWindow : Window
    {
        private readonly VenteController _controller = new VenteController();
        private Vente? _venteSelectionnee;

        public CreditWindow()
        {
            InitializeComponent();
            ChargerCredits();
        }

        private void ChargerCredits()
        {
            var credits = _controller.CreditsEnCours();
            GrilleCredits.ItemsSource = credits;
            TexteTotalCredit.Text = $"Total des crédits en cours : {credits.Sum(v => v.Solde):N0} F ({credits.Count} vente(s))";
        }

        private void GrilleCredits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _venteSelectionnee = GrilleCredits.SelectedItem as Vente;
            BoutonEnregistrerPaiement.IsEnabled = _venteSelectionnee != null;
        }

        private void BoutonEnregistrerPaiement_Click(object sender, RoutedEventArgs e)
        {
            if (_venteSelectionnee == null) return;

            var montant = double.TryParse(ChampMontantPaiement.Text, out var m) ? m : 0;
            var (succes, message) = _controller.EnregistrerPaiementCredit(
                _venteSelectionnee.Id, montant, _venteSelectionnee.Solde);

            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChampMontantPaiement.Clear();
                ChargerCredits();
                BoutonEnregistrerPaiement.IsEnabled = false;
            }
        }
    }
}
