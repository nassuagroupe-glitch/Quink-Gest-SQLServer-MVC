using QuinkGest.Models.Database;

namespace QuinkGest.Models.Repository
{
    /// <summary>
    /// Repository : ligne unique (Id=1) de paramètres généraux du commerce.
    /// </summary>
    public class ParametreRepository
    {
        public Parametre Obtenir()
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "SELECT * FROM Parametres WHERE Id=1";
            using var lecteur = commande.ExecuteReader();
            if (!lecteur.Read()) return new Parametre();

            return new Parametre
            {
                NomCommerce = lecteur["NomCommerce"]?.ToString() ?? "",
                Adresse = lecteur["Adresse"]?.ToString() ?? "",
                Telephone = lecteur["Telephone"]?.ToString() ?? "",
                Devise = lecteur["Devise"]?.ToString() ?? "F",
                LogoChemin = lecteur["LogoChemin"]?.ToString() ?? ""
            };
        }

        public void Enregistrer(Parametre parametre)
        {
            using var connexion = DatabaseHelper.ObtenirConnexion();
            using var commande = connexion.CreateCommand();
            commande.CommandText = @"
                UPDATE Parametres SET NomCommerce=@nomCommerce, Adresse=@adresse,
                    Telephone=@telephone, Devise=@devise, LogoChemin=@logoChemin
                WHERE Id=1";
            commande.Parameters.AddWithValue("@nomCommerce", parametre.NomCommerce);
            commande.Parameters.AddWithValue("@adresse", (object?)parametre.Adresse ?? "");
            commande.Parameters.AddWithValue("@telephone", (object?)parametre.Telephone ?? "");
            commande.Parameters.AddWithValue("@devise", parametre.Devise);
            commande.Parameters.AddWithValue("@logoChemin", (object?)parametre.LogoChemin ?? "");
            commande.ExecuteNonQuery();
        }
    }
}
