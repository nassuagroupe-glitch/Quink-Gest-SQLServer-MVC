using System;
using System.Collections.Generic;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    /// <summary>
    /// Repository : point unique d'accès à la table "Depenses" (SQL Server).
    /// </summary>
    public class DepenseRepository
    {
        public int Ajouter(Depense depense)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO Depenses (Description, Montant, Categorie, DateDepense)
                OUTPUT INSERTED.Id
                VALUES (@description, @montant, @categorie, @dateDepense);";
            commande.Parameters.AddWithValue("@description", depense.Description);
            commande.Parameters.AddWithValue("@montant", depense.Montant);
            commande.Parameters.AddWithValue("@categorie", (object?)depense.Categorie ?? "");
            commande.Parameters.AddWithValue("@dateDepense", depense.DateDepense);
            return (int)commande.ExecuteScalar();
        }

        public void Supprimer(int depenseId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "DELETE FROM Depenses WHERE Id=@id";
            commande.Parameters.AddWithValue("@id", depenseId);
            commande.ExecuteNonQuery();
        }

        public List<Depense> ListerTout()
        {
            var liste = new List<Depense>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Depenses ORDER BY DateDepense DESC";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                liste.Add(new Depense
                {
                    Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                    Description = lecteur["Description"]?.ToString() ?? "",
                    Montant = lecteur.GetDouble(lecteur.GetOrdinal("Montant")),
                    Categorie = lecteur["Categorie"]?.ToString() ?? "",
                    DateDepense = lecteur.GetDateTime(lecteur.GetOrdinal("DateDepense"))
                });
            }
            return liste;
        }

        public double TotalDuMoisEnCours()
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                SELECT ISNULL(SUM(Montant), 0) FROM Depenses
                WHERE MONTH(DateDepense) = MONTH(GETDATE()) AND YEAR(DateDepense) = YEAR(GETDATE())";
            return (double)commande.ExecuteScalar();
        }
    }
}
