using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pizzaria.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
        
        [Required]
        public string EnderecoEntrega { get; set; } = string.Empty;
        
        public string Observacoes { get; set; } = string.Empty;
        
        [Required]
        public decimal Total { get; set; }
        
        [Required]
        public DateTime DataPedido { get; set; } = DateTime.Now;
        
        [Required]
        public string Status { get; set; } = "Pendente";
        
        // Relacionamento com ItensPedido
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    }
}