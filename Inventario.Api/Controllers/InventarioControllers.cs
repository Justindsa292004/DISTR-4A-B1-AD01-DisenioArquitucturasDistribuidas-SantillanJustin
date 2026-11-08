using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers
{
    [ApiController]
    [Route("api/bodegas")]
    public class BodegasController : ControllerBase
    {
        private readonly InventarioDbContext _context;
        public BodegasController(InventarioDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Bodegas.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);
            return bodega == null ? NotFound(new { mensaje = "Bodega no encontrada." }) : Ok(bodega);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Bodega bodega)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Bodegas.Add(bodega);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = bodega.IdBodega }, bodega);
        }
    }

    [ApiController]
    [Route("api/existencias")]
    public class ExistenciasController : ControllerBase
    {
        private readonly InventarioDbContext _context;
        public ExistenciasController(InventarioDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Existencias.Include(e => e.Bodega).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var existencia = await _context.Existencias
                .Include(e => e.Bodega)
                .FirstOrDefaultAsync(e => e.IdExistencia == id);

            return existencia == null ? NotFound(new { mensaje = "Registro de existencia no encontrado." }) : Ok(existencia);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Existencia existencia)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == existencia.IdBodega))
                return NotFound(new { mensaje = $"La bodega con ID {existencia.IdBodega} no existe." });

            _context.Existencias.Add(existencia);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = existencia.IdExistencia }, existencia);
        }
    }

    [ApiController]
    [Route("api/movimientos")]
    public class MovimientosController : ControllerBase
    {
        private readonly InventarioDbContext _context;
        public MovimientosController(InventarioDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Movimientos.Include(m => m.Bodega).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movimiento = await _context.Movimientos
                .Include(m => m.Bodega)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            return movimiento == null ? NotFound(new { mensaje = "Movimiento no encontrado." }) : Ok(movimiento);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MovimientoInventario movimiento)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == movimiento.IdBodega))
                return NotFound(new { mensaje = $"La bodega con ID {movimiento.IdBodega} no existe." });

            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e => e.IdBodega == movimiento.IdBodega && e.IdProducto == movimiento.IdProducto);

            if (movimiento.Tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
            {
                if (existencia == null || existencia.Stock < movimiento.Cantidad)
                    return BadRequest(new { mensaje = "Stock insuficiente en la bodega para realizar la salida." });

                existencia.Stock -= movimiento.Cantidad;
            }
            else if (movimiento.Tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
            {
                if (existencia == null)
                {
                    existencia = new Existencia
                    {
                        IdBodega = movimiento.IdBodega,
                        IdProducto = movimiento.IdProducto,
                        Stock = movimiento.Cantidad
                    };
                    _context.Existencias.Add(existencia);
                }
                else
                {
                    existencia.Stock += movimiento.Cantidad;
                }
            }
            else
            {
                return BadRequest(new { mensaje = "El tipo de movimiento debe ser 'Entrada' o 'Salida'." });
            }

            movimiento.Fecha = DateTime.Now;
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = movimiento.IdMovimiento }, movimiento);
        }
    }

    [ApiController]
    [Route("api/reservas")]
    public class ReservasController : ControllerBase
    {
        private readonly InventarioDbContext _context;
        public ReservasController(InventarioDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Reservas.Include(r => r.Bodega).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Bodega)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            return reserva == null ? NotFound(new { mensaje = "Reserva no encontrada." }) : Ok(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservaStock reserva)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Bodegas.AnyAsync(b => b.IdBodega == reserva.IdBodega))
                return NotFound(new { mensaje = $"La bodega con ID {reserva.IdBodega} no existe." });

            var existencia = await _context.Existencias
                .FirstOrDefaultAsync(e => e.IdBodega == reserva.IdBodega && e.IdProducto == reserva.IdProducto);

            if (existencia == null || existencia.Stock < reserva.Cantidad)
                return BadRequest(new { mensaje = "No hay stock suficiente para reservar." });

            existencia.Stock -= reserva.Cantidad;
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = reserva.IdReserva }, reserva);
        }
    }
}