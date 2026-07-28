using System.Windows;
using QuinkGest.Controllers;
using QuinkGest.Models.Database;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue de connexion : ne contient aucune logique métier,
    /// délègue tout au Controller.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthController _controller = new AuthController();

        public LoginWindow()
        {
            InitializeComponent();

            var identifiants = CredentialStore.Charger();
            if (identifiants != null)
            {
                ChampNomUtilisateur.Text = identifiants.Value.nomUtilisateur;
                ChampMotDePasse.Password = identifiants.Value.motDePasse;
                CaseSouvenirMotDePasse.IsChecked = true;
            }
        }

        private void BoutonConnexion_Click(object sender, RoutedEventArgs e)
        {
            var (succes, message, utilisateur) = _controller.Connecter(
                ChampNomUtilisateur.Text, ChampMotDePasse.Password);

            if (!succes || utilisateur == null)
            {
                TexteMessage.Text = message;
                return;
            }

            if (CaseSouvenirMotDePasse.IsChecked == true)
                CredentialStore.Enregistrer(ChampNomUtilisateur.Text, ChampMotDePasse.Password);
            else
                CredentialStore.Effacer();

            new MainWindow(utilisateur).Show();
            Close();
        }
    }
}
