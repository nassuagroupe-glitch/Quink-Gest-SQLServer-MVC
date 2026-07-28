using System;
using System.Collections.Generic;
using System.Linq;

namespace QuinkGest.Models
{
    public class Vente
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string ClientNom { get; set; } = "Client comptoir";
        public double MontantTotal { get; set; }
        public string ModePaiement { get; set; } = "Espèces";
        public string VendeurNom { get; set; } = string.Empty;
        public DateTime DateVente { get; set; } = DateTime.Now;

        public List<LigneVente> Lignes { get; set; } = new();

        public double CalculerTotal() => Lignes.Sum(l => l.SousTotal());
    }
}
