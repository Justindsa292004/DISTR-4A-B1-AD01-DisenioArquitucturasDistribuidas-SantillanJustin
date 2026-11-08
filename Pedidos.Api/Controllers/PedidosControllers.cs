using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly PedidosDbContext _context;
        public ClientesController(PedidosDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Clientes.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            return cliente == null ? NotFound(new { mensaje = "Cliente no encontrado." }) : Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula))
                return BadRequest(new { mensaje = "Ya existe un cliente registrado con esa cédula." });

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, cliente);
        }
    }

    [ApiController]
    [Route("api/pedidos")]
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDbContext _context;
        public PedidosController(PedidosDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return pedido == null ? NotFound(new { mensaje = "Pedido no encontrado." }) : Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Pedido pedido)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar FK física: el cliente debe existir en PedidosDB
            if (!await _context.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente))
                return NotFound(new { mensaje = $"El cliente con ID {pedido.IdCliente} no existe." });

            pedido.Fecha = DateTime.Now;

            // Si el pedido viene con detalles, calculamos el total automáticamente
            if (pedido.Detalles != null && pedido.Detalles.Any())
            {
                pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = pedido.IdPedido }, pedido);
        }
    }

    [ApiController]
    [Route("api/detalles-pedido")]
    public class DetallesPedidoController : ControllerBase
    {
        private readonly PedidosDbContext _context;
        public DetallesPedidoController(PedidosDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.DetallesPedido.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detalle = await _context.DetallesPedido.FindAsync(id);
            return detalle == null ? NotFound(new { mensaje = "Detalle de pedido no encontrado." }) : Ok(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetallePedido detalle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pedido = await _context.Pedidos.FindAsync(detalle.IdPedido);
            if (pedido == null)
                return NotFound(new { mensaje = $"El pedido con ID {detalle.IdPedido} no existe." });

            _context.DetallesPedido.Add(detalle);

            // Actualizar el total del pedido automáticamente
            pedido.Total += detalle.Cantidad * detalle.PrecioUnitario;

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = detalle.IdDetalle }, detalle);
        }
    }
}