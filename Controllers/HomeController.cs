using Microsoft.AspNetCore.Mvc;
using Pizzaria.Models;
using Pizzaria.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 


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
            // Verifica se há um usuário logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId != null && usuarioId != 0)
            {
                // Se estiver logado, faz logout automático
                HttpContext.Session.Clear();
                TempData["Mensagem"] = "Você foi desconectado por segurança.";
            }

            return View();
        }

        public IActionResult Pedido()
        {
            // Verifica se o usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null || usuarioId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var cardapio = _context.Pizzas
                .Where(p => p.Disponivel)
                .OrderBy(p => p.Nome)
                .ToList();

            return View(cardapio);
        }

        public IActionResult Cardapio()
        {
            var pizzas = _context.Pizzas
                .Where(p => p.Disponivel)
                .OrderBy(p => p.Nome)
                .ToList();

            return View(pizzas);
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
            try
            {
                Console.WriteLine("=== INICIANDO FAZER PEDIDO ===");

                if (pizzasQuantidades == null || pizzasQuantidades.Values.Sum() == 0)
                {
                    TempData["Erro"] = "Selecione pelo menos uma pizza.";
                    return RedirectToAction("Pedido");
                }

                // VERIFICAÇÃO EXTRA RIGOROSA DO USUÁRIO
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                Console.WriteLine($"UsuarioId da sessão: {usuarioId}");

                if (usuarioId == null || usuarioId == 0)
                {
                    Console.WriteLine("ERRO: Usuário não logado");
                    TempData["Erro"] = "Usuário não logado. Faça login novamente.";
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = _context.Usuarios
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Id == usuarioId);

                if (usuario == null)
                {
                    Console.WriteLine("ERRO: Usuário não encontrado no banco");
                    TempData["Erro"] = "Usuário não encontrado. Faça login novamente.";
                    return RedirectToAction("Login", "Auth");
                }

                // VALIDAÇÃO EXTRA: Verifica se o usuário tem dados válidos
                if (string.IsNullOrEmpty(usuario.Email) ||
                    string.IsNullOrEmpty(usuario.NomeCompleto) ||
                    string.IsNullOrEmpty(usuario.Telefone))
                {
                    Console.WriteLine("ERRO: Usuário com dados incompletos");
                    TempData["Erro"] = "Seu cadastro está incompleto. Atualize seus dados.";
                    return RedirectToAction("Login", "Auth");
                }

                Console.WriteLine($"Usuário válido: {usuario.NomeCompleto} ({usuario.Email})");

                // Calcular total e criar itens do pedido
                decimal total = 0;
                var itensPedido = new List<Pizzaria.Models.ItemPedido>();

                // REMOVA O dicionário pizzaPrices - vamos usar o banco de dados
                // Filtrar apenas as pizzas com quantidade > 0
                foreach (var (pizzaId, quantidade) in pizzasQuantidades)
                {
                    if (quantidade > 0)
                    {
                        // Busca a pizza no banco de dados
                        var pizza = await _context.Pizzas.FindAsync(pizzaId);
                        if (pizza != null && pizza.Disponivel)
                        {
                            total += pizza.Preco * quantidade;

                            itensPedido.Add(new Pizzaria.Models.ItemPedido
                            {
                                PizzaId = pizza.Id,
                                Quantidade = quantidade,
                                NomePizza = pizza.Nome,
                                PrecoPizza = pizza.Preco
                            });

                            Console.WriteLine($"Adicionada: {quantidade}x {pizza.Nome} - R$ {pizza.Preco}");
                        }
                    }
                }

                // Verifica se algum item foi adicionado
                if (itensPedido.Count == 0)
                {
                    TempData["Erro"] = "Nenhuma pizza válida selecionada.";
                    return RedirectToAction("Pedido");
                }

                // Garantir que observacoes não seja null
                observacoes ??= string.Empty;

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

                Console.WriteLine($"Pedido criado com ID: {pedido.Id} para usuário ID: {pedido.UsuarioId}");

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
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO CRÍTICO: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Erro"] = "Erro ao processar pedido. Tente novamente.";
                return RedirectToAction("Pedido");
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
            // Verifica se o pedido existe
            var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("Pedido");
            }

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

        public IActionResult AcompanharPedido()
        {
            return View();
        }

        public IActionResult AdminPedidos()
        {
            // Verificação dupla por segurança
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "True")
            {
                return RedirectToAction("NaoAutorizado");
            }

            var pedidos = _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.DataPedido)
                .ToList();

            return View(pedidos);
        }

        public IActionResult DetalhesPedido(int id)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.Id == id);

            if (pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("AcompanharPedido");
            }

            // Determina de onde o usuário veio baseado no tipo de usuário
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            ViewBag.EhAdmin = isAdmin;
            ViewBag.Origem = isAdmin ? "AdminPedidos" : "AcompanharPedido";

            return View(pedido);
        }

        [HttpPost]
        public IActionResult BuscarPedido(int numeroPedido)
        {
            var pedido = _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.Id == numeroPedido);

            if (numeroPedido <= 0 || pedido == null)
            {
                TempData["Erro"] = "Pedido não encontrado.";
                return RedirectToAction("AcompanharPedido");
            }

            // Determina a origem baseado no tipo de usuário
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
            ViewBag.EhAdmin = isAdmin;
            ViewBag.Origem = isAdmin ? "AdminPedidos" : "AcompanharPedido";

            return View("DetalhesPedido", pedido);
        }

        [HttpPost]
        public IActionResult AtualizarStatus(int pedidoId, string novoStatus)
        {
            var pedido = _context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);

            if (pedido != null)
            {
                pedido.Status = novoStatus;
                _context.SaveChanges();
                TempData["Sucesso"] = "Status do pedido atualizado com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Pedido não encontrado.";
            }

            return RedirectToAction("AdminPedidos");
        }
        
        public IActionResult NaoAutorizado()
        {
            return View();
        }

    }
}