using System;

namespace QuinkGest.Models
{
    public class MouvementStock
    {
        public const string TYPE_ENTREE = "ENTREE";
        public const string TYPE_SORTIE = "SORTIE";

        public int Id { get; set; }
        public int ProduitId { get; set; }
        public string Type { get; set; } = TYPE_ENTREE;
        public double Quantite { get; set; }
        public string Motif { get; set; } = string.Empty;
        public int? FournisseurId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
