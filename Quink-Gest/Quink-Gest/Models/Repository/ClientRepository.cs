using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    /// <summary>
    /// Repository : point unique d'accès à la table "Clients" (SQL Server).
    /// </summary>
    public class ClientRepository
    {
        public int Ajouter(Client client)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO Clients (Nom, Telephone, Adresse, Email)
                OUTPUT INSERTED.Id
                VALUES (@nom, @telephone, @adresse, @email);";
            RemplirParametres(commande, client);
            return (int)commande.ExecuteScalar();
        }

        public void Modifier(Client client)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                UPDATE Clients SET Nom=@nom, Telephone=@telephone, Adresse=@adresse, Email=@email
                WHERE Id=@id";
            RemplirParametres(commande, client);
            commande.Parameters.AddWithValue("@id", client.Id);
            commande.ExecuteNonQuery();
        }

        public void Supprimer(int clientId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "DELETE FROM Clients WHERE Id=@id";
            commande.Parameters.AddWithValue("@id", clientId);
            commande.ExecuteNonQuery();
        }

        public List<Client> ListerTout()
        {
            var liste = new List<Client>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Clients ORDER BY Nom";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                liste.Add(new Client
                {
                    Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                    Nom = lecteur["Nom"]?.ToString() ?? "",
                    Telephone = lecteur["Telephone"]?.ToString() ?? "",
                    Adresse = lecteur["Adresse"]?.ToString() ?? "",
                    Email = lecteur["Email"]?.ToString() ?? ""
                });
            }
            return liste;
        }

        private static void RemplirParametres(SqlCommand commande, Client client)
        {
            commande.Parameters.AddWithValue("@nom", client.Nom);
            commande.Parameters.AddWithValue("@telephone", (object?)client.Telephone ?? "");
            commande.Parameters.AddWithValue("@adresse", (object?)client.Adresse ?? "");
            commande.Parameters.AddWithValue("@email", (object?)client.Email ?? "");
        }
    }
}
