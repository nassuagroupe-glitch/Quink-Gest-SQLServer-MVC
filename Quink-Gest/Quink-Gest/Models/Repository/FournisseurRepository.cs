using System;
using System.Collections.Generic;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    public class FournisseurRepository
    {
        public int Ajouter(Fournisseur fournisseur)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO Fournisseurs (Nom, Telephone, Adresse, Contact)
                OUTPUT INSERTED.Id
                VALUES (@nom, @telephone, @adresse, @contact);";
            commande.Parameters.AddWithValue("@nom", fournisseur.Nom);
            commande.Parameters.AddWithValue("@telephone", (object?)fournisseur.Telephone ?? "");
            commande.Parameters.AddWithValue("@adresse", (object?)fournisseur.Adresse ?? "");
            commande.Parameters.AddWithValue("@contact", (object?)fournisseur.Contact ?? "");
            return (int)commande.ExecuteScalar();
        }

        public List<Fournisseur> ListerTout()
        {
            var liste = new List<Fournisseur>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Fournisseurs ORDER BY Nom";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                liste.Add(new Fournisseur
                {
                    Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                    Nom = lecteur["Nom"]?.ToString() ?? "",
                    Telephone = lecteur["Telephone"]?.ToString() ?? "",
                    Adresse = lecteur["Adresse"]?.ToString() ?? "",
                    Contact = lecteur["Contact"]?.ToString() ?? ""
                });
            }
            return liste;
        }
    }
}
