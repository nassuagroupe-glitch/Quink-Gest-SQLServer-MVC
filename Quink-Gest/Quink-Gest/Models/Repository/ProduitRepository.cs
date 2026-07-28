using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    /// <summary>
    /// Repository : point unique d'accès à la table "Produits" (SQL Server).
    /// </summary>
    public class ProduitRepository
    {
        public int Ajouter(Produit produit)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO Produits (Nom, Reference, Categorie, Unite, PrixAchat, PrixVente,
                                       QuantiteStock, SeuilAlerte, FournisseurId, DateCreation, DateModification)
                OUTPUT INSERTED.Id
                VALUES (@nom, @reference, @categorie, @unite, @prixAchat, @prixVente,
                        @quantiteStock, @seuilAlerte, @fournisseurId, @dateCreation, @dateModification);";
            RemplirParametres(commande, produit);
            commande.Parameters.AddWithValue("@dateCreation", produit.DateCreation);
            return (int)commande.ExecuteScalar();
        }

        public void Modifier(Produit produit)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                UPDATE Produits SET Nom=@nom, Reference=@reference, Categorie=@categorie, Unite=@unite,
                    PrixAchat=@prixAchat, PrixVente=@prixVente, SeuilAlerte=@seuilAlerte,
                    FournisseurId=@fournisseurId, DateModification=@dateModification
                WHERE Id=@id";
            RemplirParametres(commande, produit);
            commande.Parameters.AddWithValue("@id", produit.Id);
            commande.ExecuteNonQuery();
        }

        public void Supprimer(int produitId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "DELETE FROM Produits WHERE Id=@id";
            commande.Parameters.AddWithValue("@id", produitId);
            commande.ExecuteNonQuery();
        }

        public List<Produit> ListerTout()
        {
            var liste = new List<Produit>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Produits ORDER BY Nom";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read()) liste.Add(LireProduit(lecteur));
            return liste;
        }

        public List<Produit> ListerEnAlerte()
        {
            var liste = new List<Produit>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Produits WHERE QuantiteStock <= SeuilAlerte ORDER BY Nom";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read()) liste.Add(LireProduit(lecteur));
            return liste;
        }

        public Produit? ParId(int produitId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Produits WHERE Id=@id";
            commande.Parameters.AddWithValue("@id", produitId);
            using var lecteur = commande.ExecuteReader();
            return lecteur.Read() ? LireProduit(lecteur) : null;
        }

        public void AjusterStock(int produitId, double nouvelleQuantite)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "UPDATE Produits SET QuantiteStock=@quantite, DateModification=@date WHERE Id=@id";
            commande.Parameters.AddWithValue("@quantite", nouvelleQuantite);
            commande.Parameters.AddWithValue("@date", DateTime.Now);
            commande.Parameters.AddWithValue("@id", produitId);
            commande.ExecuteNonQuery();
        }

        private static void RemplirParametres(SqlCommand commande, Produit produit)
        {
            commande.Parameters.AddWithValue("@nom", produit.Nom);
            commande.Parameters.AddWithValue("@reference", (object?)produit.Reference ?? "");
            commande.Parameters.AddWithValue("@categorie", (object?)produit.Categorie ?? "");
            commande.Parameters.AddWithValue("@unite", produit.Unite);
            commande.Parameters.AddWithValue("@prixAchat", produit.PrixAchat);
            commande.Parameters.AddWithValue("@prixVente", produit.PrixVente);
            commande.Parameters.AddWithValue("@quantiteStock", produit.QuantiteStock);
            commande.Parameters.AddWithValue("@seuilAlerte", produit.SeuilAlerte);
            commande.Parameters.AddWithValue("@fournisseurId",
                produit.FournisseurId == 0 ? DBNull.Value : produit.FournisseurId);
            commande.Parameters.AddWithValue("@dateModification", DateTime.Now);
        }

        private static Produit LireProduit(SqlDataReader lecteur) => new Produit
        {
            Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
            Nom = lecteur["Nom"]?.ToString() ?? "",
            Reference = lecteur["Reference"]?.ToString() ?? "",
            Categorie = lecteur["Categorie"]?.ToString() ?? "",
            Unite = lecteur["Unite"]?.ToString() ?? "piece",
            PrixAchat = lecteur.GetDouble(lecteur.GetOrdinal("PrixAchat")),
            PrixVente = lecteur.GetDouble(lecteur.GetOrdinal("PrixVente")),
            QuantiteStock = lecteur.GetDouble(lecteur.GetOrdinal("QuantiteStock")),
            SeuilAlerte = lecteur.GetDouble(lecteur.GetOrdinal("SeuilAlerte")),
            FournisseurId = lecteur["FournisseurId"] is DBNull ? 0 : lecteur.GetInt32(lecteur.GetOrdinal("FournisseurId"))
        };
    }
}
