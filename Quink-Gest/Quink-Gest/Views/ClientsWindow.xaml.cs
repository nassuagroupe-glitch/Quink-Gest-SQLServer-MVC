using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue de gestion des clients : ajout, modification, suppression.
    /// Ne fait aucun accès SQL direct.
    /// </summary>
    public partial class ClientsWindow : Window
    {
        private readonly ClientController _controller = new ClientController();
        private Client? _clientSelectionne;

        public ClientsWindow()
        {
            InitializeComponent();
            ChargerClients();
        }

        private void ChargerClients()
        {
            GrilleClients.ItemsSource = _controller.ChargerTousLesClients();
        }

        private void BoutonAjouter_Click(object sender, RoutedEventArgs e)
        {
            var client = new Client
            {
                Nom = ChampNom.Text,
                Telephone = ChampTelephone.Text,
                Adresse = ChampAdresse.Text,
                Email = ChampEmail.Text
            };

            var (succes, message) = _controller.AjouterClient(client);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerClients();
                ViderFormulaire();
            }
        }

        private void GrilleClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GrilleClients.SelectedItem is not Client client)
            {
                _clientSelectionne = null;
                return;
            }

            _clientSelectionne = client;
            GroupeFormulaire.Header = $"Modifier « {client.Nom} »";
            ChampNom.Text = client.Nom;
            ChampTelephone.Text = client.Telephone;
            ChampAdresse.Text = client.Adresse;
            ChampEmail.Text = client.Email;

            BoutonAjouter.IsEnabled = false;
            BoutonModifier.IsEnabled = true;
            BoutonSupprimer.IsEnabled = true;
            BoutonNouveau.IsEnabled = true;
        }

        private void BoutonModifier_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;

            var client = new Client
            {
                Id = _clientSelectionne.Id,
                Nom = ChampNom.Text,
                Telephone = ChampTelephone.Text,
                Adresse = ChampAdresse.Text,
                Email = ChampEmail.Text
            };

            var (succes, message) = _controller.ModifierClient(client);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerClients();
                ViderFormulaire();
            }
        }

        private void BoutonSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null) return;

            var confirmation = MessageBox.Show(
                $"Supprimer définitivement « {_clientSelectionne.Nom} » ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.Yes) return;

            var (succes, message) = _controller.SupprimerClient(_clientSelectionne.Id);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerClients();
                ViderFormulaire();
            }
        }

        private void BoutonNouveau_Click(object sender, RoutedEventArgs e) => ViderFormulaire();

        private void ViderFormulaire()
        {
            _clientSelectionne = null;
            GrilleClients.SelectedItem = null;
            GroupeFormulaire.Header = "Nouveau client";

            ChampNom.Clear();
            ChampTelephone.Clear();
            ChampAdresse.Clear();
            ChampEmail.Clear();

            BoutonAjouter.IsEnabled = true;
            BoutonModifier.IsEnabled = false;
            BoutonSupprimer.IsEnabled = false;
            BoutonNouveau.IsEnabled = false;
        }
    }
}
