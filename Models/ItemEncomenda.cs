using System.ComponentModel.DataAnnotations.Schema;

namespace Ecomerce.Models
{
    public class ItemEncomenda
    {
        public int Id { get; set; }
        public int EncomendaId { get; set; }
        public Encomenda? Encomenda { get; set; }
        public int ProdutoId { get; set; }        
        public int Quantidade { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecoNaCompra { get; set; }
    }
}