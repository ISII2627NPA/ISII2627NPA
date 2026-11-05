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



    