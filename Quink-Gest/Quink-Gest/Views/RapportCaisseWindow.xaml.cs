using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QuinkGest.Controllers;

namespace QuinkGest.Views
{
    /// <summary>
    /// Rapport de caisse : ventes, dépenses et solde net pour une date donnée.
    /// </summary>
    public partial class RapportCaisseWindow : Window
    {
        private readonly VenteController _venteController = new VenteController();
        private readonly DepenseController _depenseController = new DepenseController();

        public RapportCaisseWindow()
        {
            InitializeComponent();
            SelecteurDate.SelectedDate = DateTime.Today;
            ChargerRapport();
        }

        private void SelecteurDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ChargerRapport();

        private void BoutonActualiser_Click(object sender, RoutedEventArgs e) => ChargerRapport();

        private void ChargerRapport()
        {
            var date = SelecteurDate.SelectedDate ?? DateTime.Today;
            var debut = date.Date;
            var fin = debut.AddDays(1).AddSeconds(-1);

            var ventes = _venteController.HistoriquePeriode(debut, fin);
            var depenses = _depenseController.HistoriquePeriode(debut, fin);

            GrilleVentes.ItemsSource = ventes;
            GrilleDepenses.ItemsSource = depenses;

            var totalVentes = ventes.Sum(v => v.MontantTotal);
            var totalDepenses = depenses.Sum(d => d.Montant);
            var soldeNet = totalVentes - totalDepenses;

            TexteTotalVentes.Text = $"Total des ventes : {totalVentes:N0} F ({ventes.Count} vente(s))";
            TexteTotalDepenses.Text = $"Total des dépenses : {totalDepenses:N0} F ({depenses.Count} dépense(s))";
            TexteSoldeNet.Text = $"Solde net de caisse : {soldeNet:N0} F";

            var parMode = ventes
                .GroupBy(v => v.ModePaiement)
                .Select(g => $"{g.Key} : {g.Sum(v => v.MontantTotal):N0} F")
                .ToList();
            TexteParMode.Text = parMode.Count > 0
                ? "Répartition par mode de paiement : " + string.Join("   |   ", parMode)
                : "Aucune vente pour cette date";
        }
    }
}
