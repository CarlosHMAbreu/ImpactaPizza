using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pizzaria.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O ID do pedido é obrigatório")]
        public int PedidoId { get; set; }
        
        [Required(ErrorMessage = "O ID da pizza é obrigatório")]
        public int PizzaId { get; set; }
        
        [ForeignKey("PizzaId")]
        public Pizza? Pizza { get; set; }
        
        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, 100, ErrorMessage = "A quantidade deve ser entre 1 e 100")]
        public int Quantidade { get; set; } = 1;
        
        [Required(ErrorMessage = "O nome da pizza é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string NomePizza { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 1000, ErrorMessage = "O preço deve ser maior que zero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoPizza { get; set; }
        
        [NotMapped]
        public decimal Subtotal => PrecoPizza * Quantidade;
    }
}