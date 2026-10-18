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
        [ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetLibrosParaSubastar(string? titulo, string? editorial)
        {
            // Consulta a la base de datos usando Entity Framework Core
            IList<LibroParaSubastarDTO> librosSeleccionados = await _context.Libros
                
                // 1. Join con la tabla Editorial para poder acceder a su nombre
                .Include(l => l.Editorial)

                // 2. Aplicamos los filtros correspondientes a los flujos del caso de uso
                .Where(l => 
                    l.Stock > 0 // Flujo Alternativo 0: Solo mostrar si hay stock disponible
                    && (titulo == null || l.Titulo.Contains(titulo)) // Flujo Alternativo 1: Filtro por título
                    && (editorial == null || l.Editorial.Nombre.Equals(editorial)) // Flujo Alternativo 1: Filtro por editorial
                )

                // 3. Ordenamos por título (opcional, pero buena práctica heredada del modelo)
                .OrderBy(l => l.Titulo)

                // 4. Proyectamos la entidad Libro hacia nuestro LibroParaSubastarDTO
                .Select(l => new LibroParaSubastarDTO(
                    l.Id, 
                    l.Titulo, 
                    l.Autor, 
                    l.Editorial.Nombre, // Sacamos el nombre de la tabla unida
                    l.TipoLibro
                ))
                .ToListAsync();

            if (!librosSeleccionados.Any())
            {
                // Opcional: registrar en el logger si no se encuentran libros
                _logger.LogInformation($"{DateTime.Now} Info: No se encontraron libros para subastar con los filtros dados.");
            }

            return Ok(librosSeleccionados);
        }
    }
}