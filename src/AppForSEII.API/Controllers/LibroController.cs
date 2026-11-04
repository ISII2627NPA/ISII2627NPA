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

        
       // indica que es para LEER datos. En este caso, pedir el catálogo de libros.
        [HttpGet]
        // [Route("[action]")] coge el nombre del método para la URL: /api/Libro/GetLibrosParaComprar
        [Route("[action]")]
        // Documentación para Swagger: Indica qué devuelve si todo va bien (una lista de DTOs con código 200 OK)
        [ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
        // Documentación para Swagger: Indica qué devuelve si hay un error de validación (código 400 Bad Request)
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]

        // Define una puerta de entrada pública y asíncrona que entregará un paquete web (éxito o error) basándose en dos filtros opcionales proporcionados por el usuario.
        public async Task<ActionResult> GetLibrosParaComprar(string? genero, double? precioMaximo)
        {
            
            // Comprueba si el cliente ha enviado un número y, en caso afirmativo, si ese número es menor que cero.
            if (precioMaximo != null && precioMaximo < 0)
        {
            // Anota el fallo en la "libreta de errores" de la aplicación, asociando el mensaje de texto al campo que ha fallado.
            ModelState.AddModelError("precioMaximo", "El precio máximo no puede ser negativo");
    
            // Guarda un registro interno e invisible en el servidor con la fecha y hora exacta del fallo para que el equipo técnico lo revise.
            _logger.LogError($"{DateTime.Now} Error: precioMaximo no puede ser negativo");
    
            // Detiene la ejecución del método y le devuelve al cliente su error estructurado junto a un código HTTP 400.
             return BadRequest(new ValidationProblemDetails(ModelState));
        }

        // Conecta con la base de datos y prepara una lista para guardar los libros encontrados.
        IList<LibroParaComprarDTO> selectLibros = await _context.Libros
            // Trae también los datos del género y editorial del libro haciendo un JOIN de las dos tablas(datos comunes entre las dos).
            .Include(l => l.Genero)
            .Include(l => l.Editorial)
            // FILTROS: Solo mostramos libros que se puedan comprar (Stock mayor que 0)
            .Where(l => 
                l.Stock > 0 

                //Si 'genero' es null, ignora esta condición (devuelve true). 
                 // Si 'genero' trae algo, busca que coincida exactamente con el nombre del género del libro.
                && (genero == null || l.Genero.Nombre.Equals(genero)) 

                //Igual que el anterior, pero comprobando que el precio no supere el máximo.
                && (precioMaximo == null || (double)l.PrecioCompra <= precioMaximo) 
            )
            // Ordena la lista alfabéticamente por el título del libro
            .OrderBy(l => l.Titulo)

            // Por cada 'Libro' (entidad de base de datos) que haya superado los filtros, 
            // llama al constructor de nuestro DTO pasándole los datos exactos.
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
            // Devuelve un HTTP 200 OK con la lista de libros filtrada
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



    