using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly CatalogoDbContext _context;
        public CategoriasController(CatalogoDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Categorias.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            return categoria == null ? NotFound(new { mensaje = "Categoría no encontrada." }) : Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Categoria categoria)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = categoria.IdCategoria }, categoria);
        }
    }

    [ApiController]
    [Route("api/marcas")]
    public class MarcasController : ControllerBase
    {
        private readonly CatalogoDbContext _context;
        public MarcasController(CatalogoDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Marcas.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            return marca == null ? NotFound(new { mensaje = "Marca no encontrada." }) : Ok(marca);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Marca marca)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = marca.IdMarca }, marca);
        }
    }

    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly CatalogoDbContext _context;
        public ProductosController(CatalogoDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Productos.Include(p => p.Categoria).Include(p => p.Marca).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            return producto == null ? NotFound(new { mensaje = "Producto no encontrado." }) : Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Producto producto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar FK físicas (Categoría y Marca deben existir en CatalogoDB)
            if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
                return NotFound(new { mensaje = $"La categoría con ID {producto.IdCategoria} no existe." });

            if (!await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca))
                return NotFound(new { mensaje = $"La marca con ID {producto.IdMarca} no existe." });

            if (await _context.Productos.AnyAsync(p => p.SKU == producto.SKU))
                return BadRequest(new { mensaje = "El SKU ya está registrado en otro producto." });

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = producto.IdProducto }, producto);
        }
    }
}