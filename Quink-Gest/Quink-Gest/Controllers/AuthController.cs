using QuinkGest.Models;
using QuinkGest.Models.Repository;

namespace QuinkGest.Controllers
{
    public class AuthController
    {
        private readonly AuthRepository _repository = new AuthRepository();

        public (bool succes, string message, Utilisateur? utilisateur) Connecter(
            string nomUtilisateur, string motDePasse)
        {
            if (string.IsNullOrWhiteSpace(nomUtilisateur) || string.IsNullOrWhiteSpace(motDePasse))
                return (false, "Nom d'utilisateur et mot de passe requis", null);

            var utilisateur = _repository.Connexion(nomUtilisateur, motDePasse);
            if (utilisateur == null)
                return (false, "Identifiants incorrects", null);

            return (true, $"Bienvenue {utilisateur.NomComplet}", utilisateur);
        }
    }
}
