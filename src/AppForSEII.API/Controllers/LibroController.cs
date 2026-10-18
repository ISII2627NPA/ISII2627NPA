using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AppForSEII.API.DTOs;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LibroController> _logger;

        public LibroController(ApplicationDbContext context, ILogger<LibroController> logger)
        {
            _context = context;
            _logger = logger;
        }
[HttpGet]
[Route("[action]")]
[ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
public async Task<ActionResult> GetLibrosParaComprar(string? genero, double? precioMaximo)
{
    // Validación básica: el precio máximo no puede ser negativo
    if (precioMaximo != null && precioMaximo < 0)
    {
        ModelState.AddModelError("precioMaximo", "El precio máximo no puede ser negativo");
        _logger.LogError($"{DateTime.Now} Error: precioMaximo no puede ser negativo");
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    IList<LibroParaComprarDTO> selectLibros = await _context.Libros
        .Include(l => l.Genero)
        .Include(l => l.Editorial)
        .Where(l => 
            l.Stock > 0 // Flujo básico 2: Libros disponibles (stock > 0)
            && (genero == null || l.Genero.Nombre.Equals(genero)) // Flujo alternativo 1: filtro por género
            && (precioMaximo == null || (double)l.PrecioCompra <= precioMaximo) // Flujo alternativo 1: filtro por precio
        )
        .OrderBy(l => l.Titulo)
        // Aquí usamos tu constructor exacto con los 8 parámetros en tu orden
        .Select(l => new LibroParaComprarDTO(
            l.Id, 
            l.Titulo, 
            l.Genero.Nombre, 
            l.Editorial.Nombre, 
            l.Autor, 
            l.FechaLanzamiento, 
            (double)l.PrecioCompra, 
            l.Stock
        ))
        .ToListAsync();

    return Ok(selectLibros);
}
    }
    
}