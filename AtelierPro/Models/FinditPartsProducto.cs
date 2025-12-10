using System.Collections.Generic;

namespace AtelierPro.Models
{
    public class FinditPartsProducto
    {
        public string Url { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string BrandInfo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string CrossReferences { get; set; } = string.Empty;
        public string Images { get; set; } = string.Empty;
    }

    public class FinditPartsResponse
    {
        public bool Success { get; set; }
        public List<FinditPartsProducto> Resultados { get; set; } = new();
        public int Total { get; set; }
        public string? Error { get; set; }
    }
}
