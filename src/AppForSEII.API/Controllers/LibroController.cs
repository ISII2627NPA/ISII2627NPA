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



        [HttpGet]      // Define que este endpoint responderá a peticiones HTTP de tipo GET.
        [Route("[action]")]        // Construye la URL usando el nombre del método en el controlador (ej. /api/Libros/GetLibrosParaSubastar).
        [ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]        // Documenta para Swagger que en caso de éxito devolverá una lista de DTOs con un código 200 (OK).
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]         // Documenta que puede devolver un error 400 (Bad Request) si los parámetros enviados son inválidos.
        public async Task<ActionResult> GetLibrosParaSubastar(string? titulo, string? editorial)        // Método asíncrono que recibe los filtros opcionales (el '?' indica que pueden ser nulos) por parámetros en la URL.
        {
            // Consulta a la base de datos usando Entity Framework Core mediante LINQ de forma asíncrona.
            IList<LibroParaSubastarDTO> librosSeleccionados = await _context.Libros
                
                // 1. Eager Loading (Carga ansiosa): Realiza un JOIN en SQL con la tabla 'Editorial'.
                // Es obligatorio hacerlo porque más abajo en el 'Select' vamos a acceder a una propiedad de esa tabla (l.Editorial.Nombre).
                .Include(l => l.Editorial)

                // 2. Cláusula WHERE: Aplica las reglas de negocio base y los filtros solicitados por el usuario.
                .Where(l => 
                    l.Stock > 0 // Flujo Alternativo 0 (regla de negocio estricta): Solo mostrar si hay stock disponible
                    && (titulo == null || l.Titulo.Contains(titulo)) // Flujo Alternativo 1: Filtro por título. Si 'titulo' es nulo, la condición es True y no filtra. Si trae texto, busca que el título del libro contenga ese fragmento.
                    && (editorial == null || l.Editorial.Nombre.Equals(editorial)) // Flujo Alternativo 1: Filtro por editorial. Si 'editorial' es nulo, no filtra. Si trae texto, busca una coincidencia exacta con el nombre de la editorial.
                )

                // 3. Ordenación: Garantiza que la respuesta de la base de datos venga ordenada alfabéticamente por el título.
                .OrderBy(l => l.Titulo)

                // 4. Proyección: Transforma la entidad de base de datos 'Libro' (que puede tener decenas de campos internos) 
                // en un objeto DTO limpio (LibroParaSubastarDTO) que solo contiene la información que la interfaz de usuario necesita.
                .Select(l => new LibroParaSubastarDTO(
                    l.Id, 
                    l.Titulo, 
                    l.Autor, 
                    l.Editorial.Nombre,     // Extrae el nombre directamente de la relación mapeada en el 'Include'.
                    l.TipoLibro
                ))
                .ToListAsync();      // Desencadena la ejecución real de toda la consulta SQL en la base de datos y la convierte en una lista en memoria.

            if (!librosSeleccionados.Any())      // Comprueba de forma eficiente si la lista materializada está vacía (es decir, no hubo resultados para esos filtros).
            {
                // Utiliza el servicio ILogger inyectado para dejar un rastro en los registros del servidor (útil para depurar o monitorizar qué buscan los usuarios sin éxito).
                _logger.LogInformation($"{DateTime.Now} Info: No se encontraron libros para subastar con los filtros dados.");
            }

            return Ok(librosSeleccionados);    // Devuelve un HTTP 200 (OK) adjuntando en el cuerpo de la respuesta la lista final en formato JSON.
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







        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<LibroParaReseñarDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetLibrosParaReseñar(string? tipoLibro, string? genero)
        {
            IList<LibroParaReseñarDTO> librosSeleccionados = await _context.Libros
                .Include(l => l.Editorial)
                .Include(l => l.Genero)
                .Where(l => 
                    (tipoLibro == null || l.TipoLibro.Equals(tipoLibro)) && 
                    (genero == null || l.Genero.Nombre.Equals(genero))
                )
                .OrderBy(l => l.Titulo)
                .Select(l => new LibroParaReseñarDTO(
                    l.Id, 
                    l.Titulo, 
                    l.TipoLibro, 
                    l.Genero.Nombre, 
                    l.Editorial.Nombre, 
                    l.Autor, 
                    l.CalificacionMedia,
                    l.FechaLanzamiento
                ))
                .ToListAsync();

            if (!librosSeleccionados.Any())
            {
                _logger.LogInformation($"{DateTime.Now} Info: No se encontraron libros para reseñar con los filtros dados.");
            }

            return Ok(librosSeleccionados);
        }


    }




            
}



    