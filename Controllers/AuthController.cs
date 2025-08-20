using Microsoft.AspNetCore.Mvc;
using Pizzaria.Data;
using Pizzaria.Models;
using System.Linq;

namespace Pizzaria.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senha);
            
            if (usuario != null)
            {
                // Login bem-sucedido - redireciona para a página de pedidos
                return RedirectToAction("Pedido", "Home");
            }
            else
            {
                // Login falhou
                ViewBag.ErrorMessage = "Email ou senha incorretos";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verifica se o email já existe
                if (_context.Usuarios.Any(u => u.Email == usuario.Email))
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado");
                    return View(usuario);
                }
                
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(usuario);
        }
    }
}