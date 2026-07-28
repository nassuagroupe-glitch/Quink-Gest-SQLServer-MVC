using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue des paramètres : informations du commerce et gestion des utilisateurs.
    /// Ne fait aucun accès SQL direct.
    /// </summary>
    public partial class ParametresWindow : Window
    {
        private readonly ParametreController _controller = new ParametreController();
        private readonly Utilisateur _utilisateurConnecte;
        private Utilisateur? _utilisateurSelectionne;

        public ParametresWindow(Utilisateur utilisateurConnecte)
        {
            InitializeComponent();
            _utilisateurConnecte = utilisateurConnecte;
            ChargerParametres();
            ChargerUtilisateurs();
        }

        private void ChargerParametres()
        {
            var parametres = _controller.ChargerParametres();
            ChampNomCommerce.Text = parametres.NomCommerce;
            ChampAdresse.Text = parametres.Adresse;
            ChampTelephone.Text = parametres.Telephone;
            ChampDevise.Text = parametres.Devise;
        }

        private void ChargerUtilisateurs()
        {
            GrilleUtilisateurs.ItemsSource = _controller.ChargerTousLesUtilisateurs();
        }

        private void BoutonEnregistrerParametres_Click(object sender, RoutedEventArgs e)
        {
            var parametres = new Parametre
            {
                NomCommerce = ChampNomCommerce.Text,
                Adresse = ChampAdresse.Text,
                Telephone = ChampTelephone.Text,
                Devise = ChampDevise.Text
            };

            var (succes, message) = _controller.EnregistrerParametres(parametres);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void BoutonCreerUtilisateur_Click(object sender, RoutedEventArgs e)
        {
            var roleItem = ChampRole.SelectedItem as ComboBoxItem;

            var (succes, message) = _controller.CreerUtilisateur(
                ChampNomUtilisateur.Text, ChampMotDePasse.Text, ChampNomComplet.Text,
                roleItem?.Content?.ToString() ?? "vendeur");

            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerUtilisateurs();
                ChampNomUtilisateur.Clear();
                ChampMotDePasse.Clear();
                ChampNomComplet.Clear();
                ChampRole.SelectedIndex = 0;
            }
        }

        private void GrilleUtilisateurs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _utilisateurSelectionne = GrilleUtilisateurs.SelectedItem as Utilisateur;
            BoutonSupprimerUtilisateur.IsEnabled = _utilisateurSelectionne != null;
        }

        private void BoutonSupprimerUtilisateur_Click(object sender, RoutedEventArgs e)
        {
            if (_utilisateurSelectionne == null) return;

            var confirmation = MessageBox.Show(
                $"Supprimer définitivement l'utilisateur « {_utilisateurSelectionne.NomUtilisateur} » ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            var (succes, message) = _controller.SupprimerUtilisateur(
                _utilisateurSelectionne.Id, _utilisateurConnecte.Id);

            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerUtilisateurs();
                BoutonSupprimerUtilisateur.IsEnabled = false;
            }
        }
    }
}
