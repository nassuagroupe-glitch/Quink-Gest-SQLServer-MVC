using System;
using System.Collections.Generic;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    public class FournisseurController
    {
        private readonly FournisseurRepository _repository = new FournisseurRepository();

        public List<Fournisseur> ChargerTousLesFournisseurs() => _repository.ListerTout();

        public (bool succes, string message) AjouterFournisseur(Fournisseur fournisseur)
        {
            if (string.IsNullOrWhiteSpace(fournisseur.Nom))
                return (false, "Le nom du fournisseur est obligatoire");

            try
            {
                _repository.Ajouter(fournisseur);
                return (true, $"Fournisseur \"{fournisseur.Nom}\" ajouté");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur : {ex.Message}");
            }
        }
    }
}
