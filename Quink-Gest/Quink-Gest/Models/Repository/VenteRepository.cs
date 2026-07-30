using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    public class VenteRepository
    {
        /// <summary>Enregistre la vente ET ses lignes dans une transaction SQL Server unique.</summary>
        public int Enregistrer(Vente vente)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var transaction = connexion.BeginTransaction();
            try
            {
                using var commandeVente = connexion.CreateCommand();
                commandeVente.Transaction = transaction;
                commandeVente.CommandText = @"
                    INSERT INTO Ventes (Numero, ClientId, ClientNom, MontantTotal, MontantPaye, ModePaiement, VendeurNom, DateVente)
                    OUTPUT INSERTED.Id
                    VALUES (@numero, @clientId, @clientNom, @montantTotal, @montantPaye, @modePaiement, @vendeurNom, @dateVente);";
                commandeVente.Parameters.AddWithValue("@numero", vente.Numero);
                commandeVente.Parameters.AddWithValue("@clientId", (object?)vente.ClientId ?? DBNull.Value);
                commandeVente.Parameters.AddWithValue("@clientNom", vente.ClientNom);
                commandeVente.Parameters.AddWithValue("@montantTotal", vente.MontantTotal);
                commandeVente.Parameters.AddWithValue("@montantPaye", vente.MontantPaye);
                commandeVente.Parameters.AddWithValue("@modePaiement", vente.ModePaiement);
                commandeVente.Parameters.AddWithValue("@vendeurNom", vente.VendeurNom);
                commandeVente.Parameters.AddWithValue("@dateVente", vente.DateVente);
                var venteId = (int)commandeVente.ExecuteScalar();

                foreach (var ligne in vente.Lignes)
                {
                    using var commandeLigne = connexion.CreateCommand();
                    commandeLigne.Transaction = transaction;
                    commandeLigne.CommandText = @"
                        INSERT INTO LignesVente (VenteId, ProduitId, NomProduit, Unite, Quantite, PrixUnitaire)
                        VALUES (@venteId, @produitId, @nomProduit, @unite, @quantite, @prixUnitaire)";
                    commandeLigne.Parameters.AddWithValue("@venteId", venteId);
                    commandeLigne.Parameters.AddWithValue("@produitId", ligne.ProduitId);
                    commandeLigne.Parameters.AddWithValue("@nomProduit", ligne.NomProduit);
                    commandeLigne.Parameters.AddWithValue("@unite", ligne.Unite);
                    commandeLigne.Parameters.AddWithValue("@quantite", ligne.Quantite);
                    commandeLigne.Parameters.AddWithValue("@prixUnitaire", ligne.PrixUnitaire);
                    commandeLigne.ExecuteNonQuery();
                }

                transaction.Commit();
                return venteId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Vente> ListerParPeriode(DateTime debut, DateTime fin)
        {
            var liste = new List<Vente>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"SELECT * FROM Ventes
                WHERE DateVente >= @debut AND DateVente <= @fin ORDER BY DateVente DESC";
            commande.Parameters.AddWithValue("@debut", debut);
            commande.Parameters.AddWithValue("@fin", fin);
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
                liste.Add(LireVente(lecteur));
            return liste;
        }

        public double TotalVenduAujourdhui()
        {
            var debut = DateTime.Today;
            var fin = debut.AddDays(1).AddSeconds(-1);
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"SELECT ISNULL(SUM(MontantTotal), 0) FROM Ventes
                WHERE DateVente >= @debut AND DateVente <= @fin";
            commande.Parameters.AddWithValue("@debut", debut);
            commande.Parameters.AddWithValue("@fin", fin);
            return Convert.ToDouble(commande.ExecuteScalar());
        }

        public List<Vente> ListerCreditsEnCours()
        {
            var liste = new List<Vente>();
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"SELECT * FROM Ventes
                WHERE ModePaiement = 'Crédit' AND MontantPaye < MontantTotal
                ORDER BY DateVente ASC";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
                liste.Add(LireVente(lecteur));
            return liste;
        }

        /// <summary>Ajoute un paiement partiel ou total à une vente à crédit.</summary>
        public void EnregistrerPaiement(int venteId, double montant)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "UPDATE Ventes SET MontantPaye = MontantPaye + @montant WHERE Id=@id";
            commande.Parameters.AddWithValue("@montant", montant);
            commande.Parameters.AddWithValue("@id", venteId);
            commande.ExecuteNonQuery();
        }

        /// <summary>Note la date du jour comme dernière relance effectuée pour ce crédit.</summary>
        public void MarquerRelance(int venteId)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "UPDATE Ventes SET DateDerniereRelance = @date WHERE Id=@id";
            commande.Parameters.AddWithValue("@date", DateTime.Now);
            commande.Parameters.AddWithValue("@id", venteId);
            commande.ExecuteNonQuery();
        }

        private static Vente LireVente(SqlDataReader lecteur) => new()
        {
            Id = lecteur.GetInt32(lecteur.GetOrdinal("Id")),
            Numero = lecteur["Numero"]?.ToString() ?? "",
            ClientId = lecteur["ClientId"] is DBNull ? null : lecteur.GetInt32(lecteur.GetOrdinal("ClientId")),
            ClientNom = lecteur["ClientNom"]?.ToString() ?? "",
            MontantTotal = lecteur.GetDouble(lecteur.GetOrdinal("MontantTotal")),
            MontantPaye = lecteur.GetDouble(lecteur.GetOrdinal("MontantPaye")),
            ModePaiement = lecteur["ModePaiement"]?.ToString() ?? "",
            VendeurNom = lecteur["VendeurNom"]?.ToString() ?? "",
            DateVente = lecteur.GetDateTime(lecteur.GetOrdinal("DateVente")),
            DateDerniereRelance = lecteur["DateDerniereRelance"] is DBNull
                ? null
                : lecteur.GetDateTime(lecteur.GetOrdinal("DateDerniereRelance"))
        };
    }
}
