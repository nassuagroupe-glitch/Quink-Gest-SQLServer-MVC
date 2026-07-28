using System;

namespace QuinkGest.Models
{
    /// <summary>
    /// Article du catalogue et du stock de la quincaillerie
    /// (outillage, plomberie, électricité, visserie, peinture...).
    /// </summary>
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public string Unite { get; set; } = "piece";
        public double PrixAchat { get; set; }
        public double PrixVente { get; set; }
        public double QuantiteStock { get; set; }
        public double SeuilAlerte { get; set; } = 5;
        public int FournisseurId { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime DateModification { get; set; } = DateTime.Now;

        public bool EstEnAlerte() => QuantiteStock <= SeuilAlerte;
    }
}
