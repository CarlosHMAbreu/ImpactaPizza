using Microsoft.AspNetCore.Mvc;
using Pizzaria.Data;
using Pizzaria.Models;
using System.Linq;
using Microsoft.AspNetCore.Http; // Adicione este using para sessões

namespace Pizzaria.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login 
    
        [HttpGet]
        public IActionResult Login()
        {
            // Se já estiver logado, redireciona para a página apropriada
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId != null)
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                if (usuario != null)
                {
                    if (usuario.IsAdmin)
                    {
                        return RedirectToAction("AdminPedidos", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Pedido", "Home");
                    }
                }
            }
            
            return View();
        }

        // POST: /Auth/Login  
        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senha);
            
            if (usuario != null)
            {
                // Armazena o ID do usuário na sessão
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("UsuarioNome", usuario.NomeCompleto);
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                
                // Verifica se é administrador (na tabela Administradores)
                var isAdmin = _context.Administradores.Any(a => a.UsuarioId == usuario.Id);
                
                // Também verifica o campo IsAdmin na tabela Usuarios por consistência
                if (isAdmin && !usuario.IsAdmin)
                {
                    // Atualiza o campo IsAdmin para manter consistência
                    usuario.IsAdmin = true;
                    _context.SaveChanges();
                }
                
                HttpContext.Session.SetString("IsAdmin", isAdmin.ToString());
                
                // Login bem-sucedido - redireciona conforme o tipo de usuário
                if (isAdmin)
                {
                    // Atualiza data do último acesso
                    var admin = _context.Administradores.FirstOrDefault(a => a.UsuarioId == usuario.Id);
                    if (admin != null)
                    {
                        admin.DataUltimoAcesso = DateTime.Now;
                        _context.SaveChanges();
                    }
                    
                    return RedirectToAction("AdminPedidos", "Home");
                }
                else
                {
                    return RedirectToAction("Pedido", "Home");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Email ou senha incorretos";
                return View();
            }
        }

        // GET: /Auth/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Auth/Cadastro
        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        // POST: /Auth/Cadastro
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

                TempData["Sucesso"] = "Usuario cadastrado com sucesso! Faça login para continuar.";
                return RedirectToAction("Login");
            }
            return View(usuario);
        }

        // GET: /Auth/CadastroAdmin
        [HttpGet]
        public IActionResult CadastroAdmin()
        {
            return View();
        }

        // POST: /Auth/CadastroAdmin
        [HttpPost]
        public IActionResult CadastroAdmin(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (_context.Usuarios.Any(u => u.Email == usuario.Email))
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado");
                    return View(usuario);
                }
                
                // Primeiro cria o usuário normal COM IsAdmin = true
                usuario.IsAdmin = true; // ESTA LINHA ESTAVA FALTANDO admin = true!
                
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                
                // Depois adiciona como administrador na tabela específica
                var administrador = new Administrador
                {
                    UsuarioId = usuario.Id,
                    NivelAcesso = "Total",
                    DataCriacao = DateTime.Now
                };
                
                _context.Administradores.Add(administrador);
                _context.SaveChanges();
                
                TempData["Sucesso"] = "Administrador cadastrado com sucesso! Faça login para continuar.";
                return RedirectToAction("Login");
            }
            return View(usuario);
        }
    }
}