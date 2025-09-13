using System.ComponentModel.DataAnnotations;

namespace Pizzaria.Models
{
    public class Administrador
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        // Propriedade de navegação
        public Usuario? Usuario { get; set; }
        
        [Required]
        public string NivelAcesso { get; set; } = "Basico"; // Basico, Total, etc.
        
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataUltimoAcesso { get; set; }
    }
}