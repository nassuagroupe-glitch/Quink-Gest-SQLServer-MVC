using System;
using System.Collections.Generic;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    public class MouvementStockRepository
    {
        public void Enregistrer(MouvementStock mouvement)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                INSERT INTO MouvementsStock (ProduitId, Type, Quantite, Motif, FournisseurId, Date)
                VALUES (@produitId, @type, @quantite, @motif, @fournisseurId, @date)";
            commande.Parameters.AddWithValue("@produitId", mouvement.ProduitId);
            commande.Parameters.AddWithValue("@type", mouvement.Type);
            commande.Parameters.AddWithValue("@quantite", mouvement.Quantite);
            commande.Parameters.AddWithValue("@motif", (object?)mouvement.Motif ?? "");
            commande.Parameters.AddWithValue("@fournisseurId",
                mouvement.FournisseurId is null or 0 ? DBNull.Value : mouvement.FournisseurId);
            commande.Parameters.AddWithValue("@date", mouvement.Date);
            commande.ExecuteNonQuery();
        }

        public List<MouvementStock> HistoriqueProduit(int produitId)
        {
            var liste = new List<MouvementStock>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM MouvementsStock WHERE ProduitId=@id ORDER BY Date DESC";
            commande.Parameters.AddWithValue("@id", produitId);
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                liste.Add(new MouvementStock
                {
                    Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
                    ProduitId = lecteur.GetInt32(lecteur.GetOrdinal("ProduitId")),
                    Type = lecteur["Type"]?.ToString() ?? "",
                    Quantite = lecteur.GetDouble(lecteur.GetOrdinal("Quantite")),
                    Motif = lecteur["Motif"]?.ToString() ?? "",
                    Date = lecteur.GetDateTime(lecteur.GetOrdinal("Date"))
                });
            }
            return liste;
        }
    }
}
