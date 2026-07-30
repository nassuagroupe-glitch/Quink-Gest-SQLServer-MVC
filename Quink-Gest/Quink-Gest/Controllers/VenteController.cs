using System;
using System.Collections.Generic;
using System.Linq;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    /// <summary>
    /// Controller gérant le processus de vente : vérification du stock,
    /// enregistrement de la vente et décrémentation automatique du stock.
    /// </summary>
    public class VenteController
    {
        private readonly VenteRepository _venteRepository = new VenteRepository();
        private readonly ProduitRepository _produitRepository = new ProduitRepository();
        private readonly MouvementStockRepository _mouvementRepository = new MouvementStockRepository();

        private readonly List<LigneVente> _panier = new();

        public IReadOnlyList<LigneVente> Panier => _panier;

        public double Total => _panier.Count == 0 ? 0 : _panier.Sum(l => l.SousTotal());

        public (bool succes, string message) AjouterAuPanier(Produit produit, double quantite)
        {
            if (quantite <= 0)
                return (false, "La quantité doit être supérieure à zéro");

            if (quantite > produit.QuantiteStock)
                return (false, $"Stock insuffisant pour {produit.Nom} (disponible : {produit.QuantiteStock} {produit.Unite})");

            _panier.Add(new LigneVente
            {
                ProduitId = produit.Id,
                NomProduit = produit.Nom,
                Unite = produit.Unite,
                Quantite = quantite,
                PrixUnitaire = produit.PrixVente
            });

            return (true, $"{produit.Nom} ajouté au panier");
        }

        public void ViderPanier() => _panier.Clear();

        public (bool succes, string message, int? venteId) ValiderVente(
            string clientNom, int? clientId, string modePaiement, string vendeurNom)
        {
            if (_panier.Count == 0)
                return (false, "Le panier est vide", null);

            var vente = new Vente
            {
                Numero = "QG-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ClientId = clientId,
                ClientNom = string.IsNullOrWhiteSpace(clientNom) ? "Client comptoir" : clientNom,
                ModePaiement = modePaiement,
                VendeurNom = vendeurNom,
                Lignes = new List<LigneVente>(_panier)
            };
            vente.MontantTotal = vente.CalculerTotal();
            vente.MontantPaye = modePaiement == "Crédit" ? 0 : vente.MontantTotal;

            try
            {
                var venteId = _venteRepository.Enregistrer(vente);
                DecrementerStockApresVente(vente);
                _panier.Clear();
                return (true, $"Vente {vente.Numero} enregistrée avec succès", venteId);
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'enregistrement : {ex.Message}", null);
            }
        }

        /// <summary>Met à jour le stock de chaque produit vendu et journalise le mouvement.</summary>
        private void DecrementerStockApresVente(Vente vente)
        {
            foreach (var ligne in vente.Lignes)
            {
                var produit = _produitRepository.ParId(ligne.ProduitId);
                if (produit == null) continue;

                _produitRepository.AjusterStock(produit.Id, produit.QuantiteStock - ligne.Quantite);
                _mouvementRepository.Enregistrer(new MouvementStock
                {
                    ProduitId = produit.Id,
                    Type = MouvementStock.TYPE_SORTIE,
                    Quantite = ligne.Quantite,
                    Motif = $"Vente {vente.Numero}"
                });
            }
        }

        public List<Vente> HistoriqueDuJour()
        {
            var debut = DateTime.Today;
            var fin = debut.AddDays(1).AddSeconds(-1);
            return _venteRepository.ListerParPeriode(debut, fin);
        }

        public double TotalVenduAujourdhui() => _venteRepository.TotalVenduAujourdhui();

        public List<Vente> HistoriquePeriode(DateTime debut, DateTime fin) => _venteRepository.ListerParPeriode(debut, fin);

        public List<Vente> CreditsEnCours() => _venteRepository.ListerCreditsEnCours();

        public (bool succes, string message) MarquerRelance(int venteId)
        {
            try
            {
                _venteRepository.MarquerRelance(venteId);
                return (true, "Relance enregistrée");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'enregistrement de la relance : {ex.Message}");
            }
        }

        public (bool succes, string message) EnregistrerPaiementCredit(int venteId, double montant, double soldeActuel)
        {
            if (montant <= 0)
                return (false, "Le montant du paiement doit être supérieur à zéro");

            if (montant > soldeActuel + 0.01)
                return (false, $"Le montant dépasse le solde restant ({soldeActuel:N0} F)");

            try
            {
                _venteRepository.EnregistrerPaiement(venteId, montant);
                return (true, "Paiement enregistré");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'enregistrement du paiement : {ex.Message}");
            }
        }
    }
}
