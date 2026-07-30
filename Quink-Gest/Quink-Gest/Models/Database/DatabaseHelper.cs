using System;
using Microsoft.Data.SqlClient;

namespace QuinkGest.Models.Database
{
    /// <summary>
    /// Point d'accès unique à la base SQL Server Express locale.
    /// Fait partie du Model : encapsule toute la persistance.
    ///
    /// Contrairement à SQLite (simple fichier), SQL Server nécessite une
    /// instance de service active sur la machine (SQL Server Express) et
    /// que la base "QuinkGest" existe : Initialiser() la crée si absente.
    /// </summary>
    public static class DatabaseHelper
    {
        // Adapter le nom d'instance si besoin (ex: ".\SQLEXPRESS", "localhost\SQLEXPRESS").
        private const string ServeurSql = @"(localdb)\MSSQLLocalDB";
        private const string NomBase = "QuinkGest";

        private static string ChaineConnexionMaster =>
            $"Server={ServeurSql};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        public static string ChaineConnexion =>
            $"Server={ServeurSql};Database={NomBase};Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenirConnexion()
        {
            var connexion = new SqlConnection(ChaineConnexion);
            connexion.Open();
            return connexion;
        }

        /// <summary>Crée la base QuinkGest (si absente) puis les tables (si absentes).</summary>
        public static void Initialiser()
        {
            CreerBaseSiAbsente();

            using var connexion = ObtenirConnexion();

            ExecuterScript(connexion, @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fournisseurs')
                CREATE TABLE Fournisseurs (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Nom NVARCHAR(200) NOT NULL,
                    Telephone NVARCHAR(50),
                    Adresse NVARCHAR(300),
                    Contact NVARCHAR(200)
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Produits')
                CREATE TABLE Produits (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Nom NVARCHAR(200) NOT NULL,
                    Reference NVARCHAR(100),
                    Categorie NVARCHAR(100),
                    Unite NVARCHAR(20) DEFAULT 'piece',
                    PrixAchat FLOAT DEFAULT 0,
                    PrixVente FLOAT NOT NULL,
                    QuantiteStock FLOAT DEFAULT 0,
                    SeuilAlerte FLOAT DEFAULT 5,
                    FournisseurId INT NULL,
                    DateCreation DATETIME2,
                    DateModification DATETIME2,
                    CONSTRAINT FK_Produits_Fournisseurs FOREIGN KEY (FournisseurId) REFERENCES Fournisseurs(Id)
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ventes')
                CREATE TABLE Ventes (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Numero NVARCHAR(50) UNIQUE,
                    ClientNom NVARCHAR(200),
                    MontantTotal FLOAT DEFAULT 0,
                    ModePaiement NVARCHAR(50),
                    VendeurNom NVARCHAR(200),
                    DateVente DATETIME2
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LignesVente')
                CREATE TABLE LignesVente (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    VenteId INT NOT NULL,
                    ProduitId INT NOT NULL,
                    NomProduit NVARCHAR(200),
                    Unite NVARCHAR(20),
                    Quantite FLOAT,
                    PrixUnitaire FLOAT,
                    CONSTRAINT FK_Lignes_Ventes FOREIGN KEY (VenteId) REFERENCES Ventes(Id),
                    CONSTRAINT FK_Lignes_Produits FOREIGN KEY (ProduitId) REFERENCES Produits(Id)
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MouvementsStock')
                CREATE TABLE MouvementsStock (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ProduitId INT NOT NULL,
                    Type NVARCHAR(20),
                    Quantite FLOAT,
                    Motif NVARCHAR(300),
                    FournisseurId INT NULL,
                    Date DATETIME2,
                    CONSTRAINT FK_Mouvements_Produits FOREIGN KEY (ProduitId) REFERENCES Produits(Id)
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Utilisateurs')
                CREATE TABLE Utilisateurs (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    NomUtilisateur NVARCHAR(100) UNIQUE NOT NULL,
                    MotDePasseHash NVARCHAR(200) NOT NULL,
                    NomComplet NVARCHAR(200),
                    Role NVARCHAR(50) DEFAULT 'vendeur'
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clients')
                CREATE TABLE Clients (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Nom NVARCHAR(200) NOT NULL,
                    Telephone NVARCHAR(50),
                    Adresse NVARCHAR(300),
                    Email NVARCHAR(200)
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Depenses')
                CREATE TABLE Depenses (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Description NVARCHAR(300) NOT NULL,
                    Montant FLOAT NOT NULL,
                    Categorie NVARCHAR(100),
                    DateDepense DATETIME2
                );

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Parametres')
                CREATE TABLE Parametres (
                    Id INT PRIMARY KEY,
                    NomCommerce NVARCHAR(200),
                    Adresse NVARCHAR(300),
                    Telephone NVARCHAR(50),
                    Devise NVARCHAR(20)
                );

                IF NOT EXISTS (SELECT * FROM Parametres WHERE Id = 1)
                INSERT INTO Parametres (Id, NomCommerce, Adresse, Telephone, Devise)
                VALUES (1, 'QuinkGest', '', '', 'F');

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Parametres') AND name = 'LogoChemin')
                ALTER TABLE Parametres ADD LogoChemin NVARCHAR(500) NULL;
            ");

            // Chaque ALTER TABLE ADD COLUMN doit être dans un batch séparé de toute
            // instruction qui utilise la nouvelle colonne (sinon "Invalid column name").
            ExecuterScript(connexion, @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventes') AND name = 'MontantPaye')
                ALTER TABLE Ventes ADD MontantPaye FLOAT NOT NULL DEFAULT 0;
            ");
            ExecuterScript(connexion, @"
                UPDATE Ventes SET MontantPaye = MontantTotal
                WHERE ModePaiement <> 'Crédit' AND MontantPaye <> MontantTotal;
            ");

            ExecuterScript(connexion, @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventes') AND name = 'DateDerniereRelance')
                ALTER TABLE Ventes ADD DateDerniereRelance DATETIME2 NULL;
            ");

            ExecuterScript(connexion, @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Ventes') AND name = 'ClientId')
                ALTER TABLE Ventes ADD ClientId INT NULL;
            ");
            ExecuterScript(connexion, @"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Ventes_Clients')
                ALTER TABLE Ventes ADD CONSTRAINT FK_Ventes_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id);
            ");
        }

        private static void ExecuterScript(SqlConnection connexion, string script)
        {
            using var commande = connexion.CreateCommand();
            commande.CommandText = script;
            commande.ExecuteNonQuery();
        }

        private static void CreerBaseSiAbsente()
        {
            using var connexion = new SqlConnection(ChaineConnexionMaster);
            connexion.Open();
            using var commande = connexion.CreateCommand();
            commande.CommandText = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{NomBase}')
                CREATE DATABASE {NomBase};";
            commande.ExecuteNonQuery();
        }
    }
}
