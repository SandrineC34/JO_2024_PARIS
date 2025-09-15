namespace JO2024API.Models.DTO
{
    public class OffreDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Nom { get; set; }
        public string SousTitre { get; set; }
        public decimal Prix { get; set; }
        public int NbPersonnes { get; set; }
        public decimal? EconomieVsSolo { get; set; }
        public string Description { get; set; }
        public bool EstFeatured { get; set; }
        public List<string> Caracteristiques { get; set; }
    }
}
