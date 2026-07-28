using System;
using System.Collections.Generic;
using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    public class ParametreController
    {
        private readonly ParametreRepository _repository = new ParametreRepository();
        private readonly AuthRepository _authRepository = new AuthRepository();

        public Parametre ChargerParametres() => _repository.Obtenir();

        public (bool succes, string message) EnregistrerParametres(Parametre parametre)
        {
            if (string.IsNullOrWhiteSpace(parametre.NomCommerce))
                return (false, "Le nom du commerce est obligatoire");

            try
            {
                _repository.Enregistrer(parametre);
                return (true, "Paramètres enregistrés");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de l'enregistrement : {ex.Message}");
            }
        }

        public List<Utilisateur> ChargerTousLesUtilisateurs() => _authRepository.ListerTout();

        public (bool succes, string message) CreerUtilisateur(
            string nomUtilisateur, string motDePasse, string nomComplet, string role)
        {
            if (string.IsNullOrWhiteSpace(nomUtilisateur) || string.IsNullOrWhiteSpace(motDePasse))
                return (false, "Nom d'utilisateur et mot de passe requis");

            if (_authRepository.NomUtilisateurExiste(nomUtilisateur))
                return (false, "Ce nom d'utilisateur existe déjà");

            try
            {
                _authRepository.CreerUtilisateur(nomUtilisateur, motDePasse, nomComplet, role);
                return (true, $"Utilisateur \"{nomUtilisateur}\" créé");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la création : {ex.Message}");
            }
        }

        public (bool succes, string message) SupprimerUtilisateur(int utilisateurId, int idUtilisateurConnecte)
        {
            if (utilisateurId == idUtilisateurConnecte)
                return (false, "Impossible de supprimer votre propre compte");

            try
            {
                _authRepository.Supprimer(utilisateurId);
                return (true, "Utilisateur supprimé");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur lors de la suppression : {ex.Message}");
            }
        }
    }
}
