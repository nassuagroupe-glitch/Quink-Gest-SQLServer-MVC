using System;
using System.Collections.Generic;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    /// <summary>
    /// Controller : reçoit les actions de la Vue, applique la logique
    /// métier (validation, alertes stock), délègue au Repository (Model).
    /// </summary>
    public class ProduitController
    {
        private readonly ProduitRepository _repository = new ProduitRepository();

        public List<Produit> ChargerTousLesProduits() => _repository.ListerTout();

        public List<Produit> ChargerProduitsEnAlerte() => _repository.ListerEnAlerte();

        public (bool succes, string message) AjouterProduit(Produit produit)
        {
            if (string.IsNullOrWhiteSpace(produit.Nom))
                return (false, "Le nom du produit est obligatoire");

            if (produit.PrixVente <= 0)
                return (false, "Le prix de vente doit être supérieur à zéro");

            try
            {
                _repository.Ajouter(produit);
                return (true, $"Produit \"{produit.Nom}\" ajouté avec succès");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'ajout : {ex.Message}");
            }
        }

        public (bool succes, string message) ModifierProduit(Produit produit)
        {
            if (string.IsNullOrWhiteSpace(produit.Nom))
                return (false, "Le nom du produit est obligatoire");

            try
            {
                _repository.Modifier(produit);
                return (true, "Produit mis à jour");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la modification : {ex.Message}");
            }
        }

        public (bool succes, string message) SupprimerProduit(int produitId)
        {
            try
            {
                _repository.Supprimer(produitId);
                return (true, "Produit supprimé");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la suppression : {ex.Message}");
            }
        }

        /// <summary>Réapprovisionnement : augmente le stock et journalise le mouvement.</summary>
        public (bool succes, string message) Reapprovisionner(int produitId, double quantite, int fournisseurId)
        {
            if (quantite <= 0)
                return (false, "La quantité doit être supérieure à zéro");

            var produit = _repository.ParId(produitId);
            if (produit == null)
                return (false, "Produit introuvable");

            _repository.AjusterStock(produitId, produit.QuantiteStock + quantite);

            new MouvementStockRepository().Enregistrer(new MouvementStock
            {
                ProduitId = produitId,
                Type = MouvementStock.TYPE_ENTREE,
                Quantite = quantite,
                Motif = "Réassort fournisseur",
                FournisseurId = fournisseurId
            });

            return (true, $"Stock de {produit.Nom} augmenté de {quantite} {produit.Unite}");
        }
    }
}
