using System.Windows;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    public partial class FournisseursWindow : Window
    {
        private readonly FournisseurController _controller = new FournisseurController();

        public FournisseursWindow()
        {
            InitializeComponent();
            ChargerFournisseurs();
        }

        private void ChargerFournisseurs()
        {
            GrilleFournisseurs.ItemsSource = _controller.ChargerTousLesFournisseurs();
        }

        private void BoutonAjouter_Click(object sender, RoutedEventArgs e)
        {
            var fournisseur = new Fournisseur
            {
                Nom = ChampNom.Text,
                Telephone = ChampTelephone.Text,
                Adresse = ChampAdresse.Text,
                Contact = ChampContact.Text
            };

            var (succes, message) = _controller.AjouterFournisseur(fournisseur);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerFournisseurs();
                ChampNom.Clear();
                ChampTelephone.Clear();
                ChampAdresse.Clear();
                ChampContact.Clear();
            }
        }
    }
}
