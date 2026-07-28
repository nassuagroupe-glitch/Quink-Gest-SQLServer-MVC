using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue de gestion des dépenses : ajout, suppression, total du mois en cours.
    /// Ne fait aucun accès SQL direct.
    /// </summary>
    public partial class DepensesWindow : Window
    {
        private readonly DepenseController _controller = new DepenseController();
        private Depense? _depenseSelectionnee;

        public DepensesWindow()
        {
            InitializeComponent();
            ChargerDepenses();
        }

        private void ChargerDepenses()
        {
            GrilleDepenses.ItemsSource = _controller.ChargerToutesLesDepenses();
            TexteTotalMois.Text = $"Total des dépenses ce mois-ci : {_controller.TotalDuMoisEnCours():N0} F";
        }

        private void BoutonAjouter_Click(object sender, RoutedEventArgs e)
        {
            var depense = new Depense
            {
                Description = ChampDescription.Text,
                Categorie = ChampCategorie.Text,
                Montant = double.TryParse(ChampMontant.Text, out var montant) ? montant : 0
            };

            var (succes, message) = _controller.AjouterDepense(depense);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerDepenses();
                ViderFormulaire();
            }
        }

        private void GrilleDepenses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _depenseSelectionnee = GrilleDepenses.SelectedItem as Depense;
            BoutonSupprimer.IsEnabled = _depenseSelectionnee != null;
        }

        private void BoutonSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_depenseSelectionnee == null) return;

            var confirmation = MessageBox.Show(
                $"Supprimer définitivement la dépense « {_depenseSelectionnee.Description} » ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            var (succes, message) = _controller.SupprimerDepense(_depenseSelectionnee.Id);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerDepenses();
                ViderFormulaire();
            }
        }

        private void ViderFormulaire()
        {
            _depenseSelectionnee = null;
            GrilleDepenses.SelectedItem = null;
            ChampDescription.Clear();
            ChampCategorie.Clear();
            ChampMontant.Clear();
            BoutonSupprimer.IsEnabled = false;
        }
    }
}
