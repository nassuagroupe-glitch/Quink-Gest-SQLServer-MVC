namespace QuinkGest.Models
{
    public class LigneVente
    {
        public int Id { get; set; }
        public int VenteId { get; set; }
        public int ProduitId { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public string Unite { get; set; } = "piece";
        public double Quantite { get; set; }
        public double PrixUnitaire { get; set; }

        public double SousTotal() => Quantite * PrixUnitaire;
    }
}
