using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    /// <summary>
    /// Authentification 100% locale : mot de passe hashé SHA-256, jamais en clair.
    /// </summary>
    public class AuthRepository
    {
        private static string Hacher(string motDePasse)
        {
            using var sha256 = SHA256.Create();
            var octets = sha256.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
            return Convert.ToBase64String(octets);
        }

        public Utilisateur? Connexion(string nomUtilisateur, string motDePasse)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"SELECT * FROM Utilisateurs
                WHERE NomUtilisateur=@nom AND MotDePasseHash=@hash";
            commande.Parameters.AddWithValue("@nom", nomUtilisateur);
            commande.Parameters.AddWithValue("@hash", Hacher(motDePasse));
            using var lecteur = commande.ExecuteReader();
            if (!lecteur.Read()) return null;

            return new Utilisateur
            {
                Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                NomUtilisateur = lecteur["NomUtilisateur"]?.ToString() ?? "",
                NomComplet = lecteur["NomComplet"]?.ToString() ?? "",
                Role = lecteur["Role"]?.ToString() ?? "vendeur"
            };
        }

        public int CreerUtilisateur(string nomUtilisateur, string motDePasse, string nomComplet, string role)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO Utilisateurs (NomUtilisateur, MotDePasseHash, NomComplet, Role)
                OUTPUT INSERTED.Id
                VALUES (@nom, @hash, @nomComplet, @role);";
            commande.Parameters.AddWithValue("@nom", nomUtilisateur);
            commande.Parameters.AddWithValue("@hash", Hacher(motDePasse));
            commande.Parameters.AddWithValue("@nomComplet", nomComplet);
            commande.Parameters.AddWithValue("@role", role);
            return (int)commande.ExecuteScalar();
        }

        /// <summary>Liste tous les utilisateurs (sans le hash du mot de passe).</summary>
        public List<Utilisateur> ListerTout()
        {
            var liste = new List<Utilisateur>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Utilisateurs ORDER BY NomUtilisateur";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                liste.Add(new Utilisateur
                {
                    Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                    NomUtilisateur = lecteur["NomUtilisateur"]?.ToString() ?? "",
                    NomComplet = lecteur["NomComplet"]?.ToString() ?? "",
                    Role = lecteur["Role"]?.ToString() ?? "vendeur"
                });
            }
            return liste;
        }

        public void Supprimer(int utilisateurId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "DELETE FROM Utilisateurs WHERE Id=@id";
            commande.Parameters.AddWithValue("@id", utilisateurId);
            commande.ExecuteNonQuery();
        }

        public bool NomUtilisateurExiste(string nomUtilisateur)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT COUNT(*) FROM Utilisateurs WHERE NomUtilisateur=@nom";
            commande.Parameters.AddWithValue("@nom", nomUtilisateur);
            return (int)commande.ExecuteScalar() > 0;
        }
    }
}
