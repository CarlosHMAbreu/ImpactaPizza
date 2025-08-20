using Microsoft.AspNetCore.Mvc;
using Pizzaria.Models;
using Pizzaria.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pizzaria.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Pedido()
        {
            var cardapio = new List<Pizza>
            {
                new Pizza { Id = 1, Nome = "Margherita", Descricao = "Molho de tomate, mussarela, manjericão", Preco = 30.00m, ImagemUrl = "/images/margherita.jpg" },
                new Pizza { Id = 2, Nome = "Pepperoni", Descricao = "Molho de tomate, mussarela, pepperoni", Preco = 35.00m, ImagemUrl = "/images/pepperoni.jpg" },
                new Pizza { Id = 3, Nome = "Calabresa", Descricao = "Molho de tomate, mussarela, calabresa, cebola", Preco = 32.00m, ImagemUrl = "/images/calabresa.jpg" },
                new Pizza { Id = 4, Nome = "Quatro Queijos", Descricao = "Molho de tomate, mussarela, parmesão, gorgonzola, provolone", Preco = 38.00m, ImagemUrl = "/images/quatro-queijos.jpg" },
                new Pizza { Id = 5, Nome = "Portuguesa", Descricao = "Molho de tomate, mussarela, presunto, ovo, cebola, azeitona", Preco = 36.00m, ImagemUrl = "/images/portuguesa.jpg" },
                new Pizza { Id = 6, Nome = "Frango com Catupiry", Descricao = "Molho de tomate, mussarela, frango desfiado, catupiry", Preco = 37.00m, ImagemUrl = "/images/frango-catupiry.jpg" }
            };
            
            return View(cardapio);
        }

        public IActionResult ResumoPedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Pedido");
            }

            return View(pedido);
        }
        [HttpPost]
        public async Task<IActionResult> FazerPedido(Dictionary<int, int> pizzasQuantidades, string enderecoEntrega, string observacoes, bool pagamentoEntrega = false)
        {
            // Filtrar apenas as pizzas com quantidade > 0
            var pizzasSelecionadas = new List<int>();
            foreach (var (pizzaId, quantidade) in pizzasQuantidades)
            {
                for (int i = 0; i < quantidade; i++)
                {
                    pizzasSelecionadas.Add(pizzaId);
                }
            }

            if (pizzasSelecionadas.Count == 0)
            {
                TempData["Erro"] = "Selecione pelo menos uma pizza.";
                return RedirectToAction("Pedido");
            }

            // Obter usuário atual (simplificado)
            var usuario = _context.Usuarios.FirstOrDefault();
            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado. Faça login novamente.";
                return RedirectToAction("Login", "Auth");
            }

            // Garantir que observacoes não seja null
            observacoes ??= string.Empty;

            // Calcular total e criar itens do pedido
            decimal total = 0;
            var itensPedido = new List<ItemPedido>();
            var pizzaPrices = new Dictionary<int, (string Nome, decimal Preco)>
            {
                {1, ("Margherita", 30.00m)},
                {2, ("Pepperoni", 35.00m)},
                {3, ("Calabresa", 32.00m)},
                {4, ("Quatro Queijos", 38.00m)},
                {5, ("Portuguesa", 36.00m)},
                {6, ("Frango com Catupiry", 37.00m)}
            };

            // Agrupar por pizzaId para obter quantidades
            var pizzaQuantidades = pizzasSelecionadas
                .GroupBy(p => p)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var (pizzaId, quantidade) in pizzaQuantidades)
            {
                if (pizzaPrices.ContainsKey(pizzaId))
                {
                    var (nome, preco) = pizzaPrices[pizzaId];
                    total += preco * quantidade;
                    
                    itensPedido.Add(new ItemPedido
                    {
                        PizzaId = pizzaId,
                        NomePizza = nome,
                        PrecoPizza = preco,
                        Quantidade = quantidade
                    });
                }
            }

            // Definir status baseado no tipo de pagamento
            string status = pagamentoEntrega ? "Aguardando Entrega" : "Aguardando Pagamento";

            // Criar pedido
            var pedido = new Pedido
            {
                UsuarioId = usuario.Id,
                EnderecoEntrega = enderecoEntrega,
                Observacoes = observacoes,
                Total = total,
                DataPedido = DateTime.Now,
                Status = status,
                Itens = itensPedido
            };

            // Salvar no banco
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            // Redirecionar conforme o tipo de pagamento
            if (pagamentoEntrega)
            {
                return Redirect($"/Home/Confirmacao/{pedido.Id}");
            }
            else
            {
                return Redirect($"/Home/ResumoPedido/{pedido.Id}");
            }
        }
        

        public IActionResult Pagamento(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefault(p => p.Id == id);
            
            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Pedido");
            }

            ViewBag.PedidoId = id;
            ViewBag.Total = pedido.Total.ToString("C");
            
            return View();
        }

        [HttpPost]
        public IActionResult ProcessarPagamento(int pedidoId)
        {
            // Buscar o pedido
            var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Pedido");
            }

            // Aqui é onde vai acontecer a integração com um gateway de pagamento real
            // Por enquanto, apenas simulamos o sucesso do pagamento
            
            pedido.Status = "Pago - Em Preparação";
            _context.SaveChanges();

            return RedirectToAction("Confirmacao", new { id = pedidoId });
        }

        public IActionResult Confirmacao(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Pedido");
            }

            return View(pedido);
        }
    }
}