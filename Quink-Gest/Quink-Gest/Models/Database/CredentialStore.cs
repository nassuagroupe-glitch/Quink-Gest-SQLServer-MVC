using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuinkGest.Models.Database
{
    /// <summary>
    /// Persistance locale et chiffrée (DPAPI, lié au compte Windows) des identifiants
    /// de connexion pour l'option "Se souvenir du mot de passe".
    /// </summary>
    public static class CredentialStore
    {
        private static readonly string DossierDonnees =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuinkGest");

        private static readonly string CheminFichier = Path.Combine(DossierDonnees, "identifiants.dat");

        public static void Enregistrer(string nomUtilisateur, string motDePasse)
        {
            Directory.CreateDirectory(DossierDonnees);
            var texte = $"{nomUtilisateur}\n{motDePasse}";
            var octetsProteges = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(texte), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(CheminFichier, octetsProteges);
        }

        public static (string nomUtilisateur, string motDePasse)? Charger()
        {
            if (!File.Exists(CheminFichier)) return null;

            try
            {
                var octetsProteges = File.ReadAllBytes(CheminFichier);
                var octets = ProtectedData.Unprotect(octetsProteges, null, DataProtectionScope.CurrentUser);
                var parties = Encoding.UTF8.GetString(octets).Split('\n', 2);
                return parties.Length == 2 ? (parties[0], parties[1]) : null;
            }
            catch
            {
                return null;
            }
        }

        public static void Effacer()
        {
            if (File.Exists(CheminFichier)) File.Delete(CheminFichier);
        }
    }
}
