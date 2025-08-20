using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pizzaria.Models
{
    public class Pedido
{
    public int Id { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }
    
    [Required]
    public string EnderecoEntrega { get; set; } = string.Empty;
    
    public string Observacoes { get; set; } = string.Empty; // Já está correto
    
    [Required]
    public decimal Total { get; set; }
    
    [Required]
    public DateTime DataPedido { get; set; } = DateTime.Now;
    
    [Required]
    public string Status { get; set; } = "Pendente";
    
    // Relacionamento com ItensPedido
    public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}

    public class ItemPedido
    {
        public int Id { get; set; }
        
        [Required]
        public int PedidoId { get; set; }
        
        [Required]
        public int PizzaId { get; set; }
        
        [Required]
        public string NomePizza { get; set; } = string.Empty;
        
        [Required]
        public decimal PrecoPizza { get; set; }
        
        [Required]
        public int Quantidade { get; set; } = 1;
    }
}