using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>
    /// Vue des informations de l'établissement : coordonnées et logo.
    /// Ne fait aucun accès SQL direct.
    /// </summary>
    public partial class EtablissementWindow : Window
    {
        private readonly ParametreController _controller = new ParametreController();
        private string _deviseActuelle = "F";
        private string _cheminLogoEnregistre = string.Empty;
        private string? _cheminLogoNouveau;

        public EtablissementWindow()
        {
            InitializeComponent();
            ChargerParametres();
        }

        private void ChargerParametres()
        {
            var parametres = _controller.ChargerParametres();
            ChampNomCommerce.Text = parametres.NomCommerce;
            ChampAdresse.Text = parametres.Adresse;
            ChampTelephone.Text = parametres.Telephone;
            _deviseActuelle = parametres.Devise;
            _cheminLogoEnregistre = parametres.LogoChemin;
            AfficherLogo(_cheminLogoEnregistre);
        }

        private void AfficherLogo(string? chemin)
        {
            if (!string.IsNullOrWhiteSpace(chemin) && File.Exists(chemin))
                ImageLogo.Source = new BitmapImage(new Uri(chemin, UriKind.Absolute));
            else
                ImageLogo.Source = null;
        }

        private void BoutonParcourirLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialogue = new OpenFileDialog
            {
                Title = "Choisir le logo de l'établissement",
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialogue.ShowDialog() == true)
            {
                _cheminLogoNouveau = dialogue.FileName;
                AfficherLogo(_cheminLogoNouveau);
            }
        }

        private void BoutonEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var cheminLogoFinal = _cheminLogoEnregistre;
            if (_cheminLogoNouveau != null)
            {
                try
                {
                    cheminLogoFinal = _controller.CopierLogo(_cheminLogoNouveau);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la copie du logo : {ex.Message}", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var parametres = new Parametre
            {
                NomCommerce = ChampNomCommerce.Text,
                Adresse = ChampAdresse.Text,
                Telephone = ChampTelephone.Text,
                Devise = _deviseActuelle,
                LogoChemin = cheminLogoFinal
            };

            var (succes, message) = _controller.EnregistrerParametres(parametres);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                _cheminLogoEnregistre = cheminLogoFinal;
                _cheminLogoNouveau = null;
            }
        }
    }
}
