using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;
using QuinkGest.Models;

namespace QuinkGest.Views
{
    /// <summary>Projection d'affichage : une vente à crédit avec le téléphone du client retrouvé par nom.</summary>
    public class LigneRelance
    {
        public Vente Vente { get; set; } = null!;
        public string Telephone { get; set; } = string.Empty;

        public string DerniereRelanceAffichee => Vente.DateDerniereRelance.HasValue
            ? Vente.DateDerniereRelance.Value.ToString("dd/MM/yyyy")
            : "Jamais";
    }

    /// <summary>
    /// Liste les clients ayant un crédit en cours, triés par ancienneté de dette,
    /// pour permettre le suivi des relances de paiement.
    /// </summary>
    public partial class RelancesWindow : Window
    {
        private readonly VenteController _venteController = new VenteController();
        private readonly ClientController _clientController = new ClientController();
        private LigneRelance? _ligneSelectionnee;

        public RelancesWindow()
        {
            InitializeComponent();
            ChargerRelances();
        }

        private void ChargerRelances()
        {
            var credits = _venteController.CreditsEnCours();
            var clients = _clientController.ChargerTousLesClients();
            var telephonesParId = clients.ToDictionary(c => c.Id, c => c.Telephone);
            var telephonesParNom = clients
                .GroupBy(c => c.Nom.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First().Telephone);

            var lignes = credits
                .Select(v => new LigneRelance
                {
                    Vente = v,
                    Telephone = v.ClientId.HasValue && telephonesParId.TryGetValue(v.ClientId.Value, out var telParId)
                        ? telParId
                        : telephonesParNom.TryGetValue(v.ClientNom.Trim().ToLowerInvariant(), out var telParNom) ? telParNom : ""
                })
                .OrderByDescending(l => l.Vente.JoursDepuisVente)
                .ToList();

            GrilleRelances.ItemsSource = lignes;
            TexteResume.Text = $"{lignes.Count} client(s) à relancer — total dû : {lignes.Sum(l => l.Vente.Solde):N0} F";
        }

        private void GrilleRelances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ligneSelectionnee = GrilleRelances.SelectedItem as LigneRelance;
            BoutonMarquerRelance.IsEnabled = _ligneSelectionnee != null;
        }

        private void BoutonMarquerRelance_Click(object sender, RoutedEventArgs e)
        {
            if (_ligneSelectionnee == null) return;

            var (succes, message) = _venteController.MarquerRelance(_ligneSelectionnee.Vente.Id);
            MessageBox.Show(message, succes ? "Succès" : "Erreur",
                MessageBoxButton.OK, succes ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (succes)
            {
                ChargerRelances();
                BoutonMarquerRelance.IsEnabled = false;
            }
        }
    }
}
