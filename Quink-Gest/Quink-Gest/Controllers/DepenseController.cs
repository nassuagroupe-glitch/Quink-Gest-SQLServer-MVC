using System;
using System.Collections.Generic;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    public class DepenseController
    {
        private readonly DepenseRepository _repository = new DepenseRepository();

        public List<Depense> ChargerToutesLesDepenses() => _repository.ListerTout();

        public double TotalDuMoisEnCours() => _repository.TotalDuMoisEnCours();

        public (bool succes, string message) AjouterDepense(Depense depense)
        {
            if (string.IsNullOrWhiteSpace(depense.Description))
                return (false, "La description de la dépense est obligatoire");

            if (depense.Montant <= 0)
                return (false, "Le montant doit être supérieur à zéro");

            try
            {
                _repository.Ajouter(depense);
                return (true, $"Dépense \"{depense.Description}\" enregistrée");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'ajout : {ex.Message}");
            }
        }

        public (bool succes, string message) SupprimerDepense(int depenseId)
        {
            try
            {
                _repository.Supprimer(depenseId);
                return (true, "Dépense supprimée");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la suppression : {ex.Message}");
            }
        }
    }
}
