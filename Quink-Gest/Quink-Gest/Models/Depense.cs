using System;

namespace QuinkGest.Models
{
    public class Depense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Montant { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public DateTime DateDepense { get; set; } = DateTime.Now;
    }
}
