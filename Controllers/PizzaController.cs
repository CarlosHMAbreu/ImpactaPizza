using Microsoft.AspNetCore.Mvc;
using Pizzaria.Data;
using Pizzaria.Models;
using Microsoft.EntityFrameworkCore;

namespace Pizzaria.Controllers
{
    public class PizzaController : Controller
    {
        private readonly AppDbContext _context;

        public PizzaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Pizza/Admin - Lista de pizzas para admin
        public IActionResult Admin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            var pizzas = _context.Pizzas.OrderBy(p => p.Nome).ToList();
            return View(pizzas);
        }

        // GET: /Pizza/Cadastrar
        public IActionResult Cadastrar()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            return View();
        }

        // POST: /Pizza/Cadastrar
        [HttpPost]
        public async Task<IActionResult> Cadastrar(Pizza pizza)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            if (ModelState.IsValid)
            {
                if (_context.Pizzas.Any(p => p.Nome == pizza.Nome))
                {
                    ModelState.AddModelError("Nome", "Já existe uma pizza com este nome");
                    return View(pizza);
                }

                pizza.DataCriacao = DateTime.Now;
                _context.Pizzas.Add(pizza);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Pizza cadastrada com sucesso!";
                return RedirectToAction("Admin");
            }

            return View(pizza);
        }

        // GET: /Pizza/Editar/5
        public IActionResult Editar(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            var pizza = _context.Pizzas.Find(id);
            if (pizza == null)
            {
                return RedirectToAction("Admin");
            }

            return View(pizza);
        }

        // POST: /Pizza/Editar/5
        [HttpPost]
        public async Task<IActionResult> Editar(int id, Pizza pizza)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            if (id != pizza.Id)
            {
                return RedirectToAction("Admin");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pizza.DataAtualizacao = DateTime.Now;
                    _context.Pizzas.Update(pizza);
                    await _context.SaveChangesAsync();

                    TempData["Sucesso"] = "Pizza atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PizzaExists(pizza.Id))
                    {
                        return RedirectToAction("Admin");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Admin");
            }

            return View(pizza);
        }

        // POST: /Pizza/Excluir/5
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                return RedirectToAction("NaoAutorizado", "Home");
            }

            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                _context.Pizzas.Remove(pizza);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Pizza excluída com sucesso!";
            }

            return RedirectToAction("Admin");
        }

        private bool PizzaExists(int id)
        {
            return _context.Pizzas.Any(e => e.Id == id);
        }
    }
}