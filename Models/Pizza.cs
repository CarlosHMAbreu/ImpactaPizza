using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pizzaria.Models
{
    public class Pizza
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da pizza é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 1000, ErrorMessage = "O preço deve ser maior que zero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        public string ImagemUrl { get; set; } = "/images/pizza-default.jpg";

        public bool Disponivel { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
    }
}