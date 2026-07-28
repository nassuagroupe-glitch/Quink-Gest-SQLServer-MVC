# Quink-Gest (C# / WPF / SQL Server Express)

Logiciel Windows de vente et de gestion de stock pour une **quincaillerie**,
déployé localement — variante de QuinkGest utilisant **SQL Server Express**
au lieu de SQLite comme moteur de base de données locale.

## Ce qui change par rapport à la version SQLite

Le point clé du pattern **MVC** se vérifie ici concrètement : changer de
serveur de base de données n'a touché **que la couche Model**
(`Models/Database/DatabaseHelper.cs` + `Models/Repository/*.cs`).
Les **Controllers et les Views sont strictement identiques** — ils ne
connaissent jamais le type de base de données utilisé, seulement les
méthodes exposées par les Repositories.

| Aspect                  | QuinkGest (SQLite)                  | Quink-Gest (SQL Server Express)         |
|---------------------------|-----------------------------------------|-----------------------------------------|
| Package NuGet               | `Microsoft.Data.Sqlite`                 | `Microsoft.Data.SqlClient`               |
| Stockage                      | Un fichier `.db` dans `%AppData%`       | Une base sur une instance SQL Server locale |
| Paramètres SQL                  | `$parametre`                           | `@parametre`                             |
| Auto-incrément                    | `AUTOINCREMENT` + `last_insert_rowid()` | `IDENTITY(1,1)` + `OUTPUT INSERTED.Id`   |
| Création de la base                 | Automatique (simple fichier)           | Nécessite une instance SQL Server Express active + `CREATE DATABASE` |
| Controllers / Views                   | **Identiques**                          | **Identiques**                           |

## Structure du projet

```
Quink-Gest/
├── Models/
│   ├── Produit.cs, Fournisseur.cs, Vente.cs, LigneVente.cs,
│   │   MouvementStock.cs, Utilisateur.cs      (inchangés)
│   ├── Database/
│   │   └── DatabaseHelper.cs                  (SQL Server au lieu de SQLite)
│   └── Repository/
│       ├── ProduitRepository.cs               (requêtes adaptées T-SQL)
│       ├── FournisseurRepository.cs
│       ├── VenteRepository.cs
│       ├── MouvementStockRepository.cs
│       └── AuthRepository.cs
│
├── Controllers/                                → INCHANGÉS (copie conforme)
│   ├── ProduitController.cs
│   ├── FournisseurController.cs
│   ├── VenteController.cs
│   └── AuthController.cs
│
├── Views/                                       → INCHANGÉES (copie conforme)
│   ├── LoginWindow.xaml(.cs)
│   ├── MainWindow.xaml(.cs)
│   ├── ProduitsWindow.xaml(.cs)
│   ├── VenteWindow.xaml(.cs)
│   ├── FournisseursWindow.xaml(.cs)
│   ├── HistoriqueVentesWindow.xaml(.cs)
│   └── AlertesStockWindow.xaml(.cs)
│
├── App.xaml / App.xaml.cs
└── Quink-Gest.csproj
```

## Prérequis : SQL Server Express installé localement

Contrairement à SQLite, il faut qu'une **instance SQL Server Express** soit
installée et démarrée sur le poste avant de lancer l'application :

1. Télécharger et installer **SQL Server Express**
   (gratuit, depuis le site officiel Microsoft)
2. Pendant l'installation, noter le nom de l'instance (par défaut
   `SQLEXPRESS`)
3. Vérifier que le service **SQL Server (SQLEXPRESS)** est démarré
   (via `services.msc`)

Par défaut, `DatabaseHelper.cs` se connecte à `.\SQLEXPRESS` avec
l'authentification Windows (`Trusted_Connection=True`). Si ton instance
porte un autre nom, modifie la constante `ServeurSql` en haut du fichier.

## Comment ça circule (identique à la version SQLite)

1. **View** (`VenteWindow.xaml.cs`) : appelle `_controller.AjouterAuPanier(...)`.
2. **Controller** (`VenteController`) : vérifie le stock, applique la logique
   métier.
3. **Model** (`VenteRepository`) : exécute une transaction SQL Server
   (`BeginTransaction`/`Commit`) pour enregistrer la vente et ses lignes de
   façon atomique.

Le Controller ne sait pas s'il parle à SQLite ou SQL Server — c'est
exactement ce découplage que le pattern MVC apporte.

## Base de données SQL Server

Au premier lancement, `DatabaseHelper.Initialiser()` :
1. Se connecte à la base `master` et crée la base **QuinkGest** si absente
   (`CREATE DATABASE`)
2. Crée les tables si absentes (`Produits`, `Fournisseurs`, `Ventes`,
   `LignesVente`, `MouvementsStock`, `Utilisateurs`) — mêmes tables que la
   version SQLite, adaptées en syntaxe T-SQL (`IDENTITY`, `DATETIME2`...)

## Installation / lancement

```bash
cd Quink-Gest
dotnet restore
dotnet run
```

Pour un `.exe` autonome :

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Premier compte utilisateur

Identique à la version SQLite — ajoute temporairement dans `App.xaml.cs` :

```csharp
new Models.Repository.AuthRepository()
    .CreerUtilisateur("admin", "motdepasse", "Gérant Quincaillerie", "gerant");
```

## Ce qui n'est pas encore implémenté

Mêmes pistes d'évolution que la version SQLite (ticket de caisse
imprimable, modification/suppression de produit dans l'UI, rapports par
catégorie, gestion des rôles, recherche/filtre du catalogue) — voir le
README du projet QuinkGest original pour le détail.
