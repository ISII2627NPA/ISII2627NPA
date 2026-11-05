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






// --- ETIQUETAS DE CONFIGURACIÓN DEL ENDPOINT (Decoradores) ---

// [HttpGet] le dice al servidor que este método solo se ejecutará cuando alguien pida leer datos. 
// Es como cuando escribes una dirección web en el navegador y le das a Enter (eso es una petición GET). No modifica la base de datos.
[HttpGet]

// [Route("[action]")] define cómo será la URL para llegar hasta aquí. "[action]" se sustituye por el nombre del método.
// La URL final será algo como: mi-servidor.com/api/Libro/GetLibrosParaReseñar
[Route("[action]")]

// [ProducesResponseType] sirve para la documentación automática (Swagger). 
// Le dice a otros programadores: "Si todo va bien (Código HTTP 200 OK), te voy a devolver una Lista (IList) de LibroParaReseñarDTO".
[ProducesResponseType(typeof(IList<LibroParaReseñarDTO>), (int)HttpStatusCode.OK)]

// Y esta le dice: "Si me mandas datos erróneos en la petición, te devolveré un Error 400 (BadRequest) con los detalles del problema".
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]


// --- DECLARACIÓN DEL MÉTODO ---

// "public": Cualquier parte del sistema o cliente externo puede llamar a esta función.
// "async Task": Significa que el método es "asíncrono". Mientras el servidor espera a que la base de datos conteste, 
// no se queda congelado, sino que puede atender a otros usuarios. Es obligatorio en aplicaciones web modernas.
// "ActionResult": Es un envoltorio estándar de ASP.NET que permite devolver tanto datos (los libros) como códigos de estado (200 OK, 404 NotFound...).
// "(string? tipoLibro, string? genero)": Son los parámetros que el usuario puede enviar por la URL. 
// El símbolo de interrogación (?) significa que son opcionales (pueden ser nulos si el usuario no quiere filtrar por nada).
public async Task<ActionResult> GetLibrosParaReseñar(string? tipoLibro, string? genero)
{
    // --- CONSULTA A LA BASE DE DATOS (LINQ a SQL) ---

    // Creamos una variable llamada "librosSeleccionados" que almacenará una Lista (IList) de nuestros DTOs.
    // "await": Significa "Pausa este bloque de código hasta que la base de datos responda".
    // "_context.Libros": Entramos a la base de datos y apuntamos directamente a la tabla "Libros".
    IList<LibroParaReseñarDTO> librosSeleccionados = await _context.Libros
        
        // ".Include": Es el equivalente a un "INNER JOIN" en bases de datos (SQL).
        // "l => l.Editorial": "l" es una variable inventada sobre la marcha (expresión Lambda) que representa a cada libro. 
        // Se lee como: "De cada libro 'l', cárgame también los datos completos de su 'Editorial' asociada".
        .Include(l => l.Editorial)
        
        // Hacemos lo mismo para traer el nombre del Género, ya que el libro por defecto solo guarda un número (GeneroId).
        .Include(l => l.Genero)
        
        // ".Where": Es el filtro de la búsqueda. Solo pasaran los libros que cumplan las condiciones de dentro.
        .Where(l => 
            // Primera condición: (tipoLibro == null || l.TipoLibro.Equals(tipoLibro))
            // Es un truco muy común. Se lee así: "Si el usuario NO ha enviado un filtro de tipoLibro (es null), ignora esta línea y déjalo pasar (||). 
            // PERO si el usuario SÍ ha enviado un tipo, comprueba que el TipoLibro de la base de datos coincida exactamente (.Equals) con el que piden".
            (tipoLibro == null || l.TipoLibro.Equals(tipoLibro)) 
            
            && // "&&" significa "Y ADEMÁS". Ambas condiciones (tipo y género) deben cumplirse a la vez.

            // Segunda condición: Misma lógica pero para el género.
            (genero == null || l.Genero.Nombre.Equals(genero))
        )
        
        // ".OrderBy": Ordena los resultados que pasaron el filtro.
        // "l => l.Titulo": Los ordena alfabéticamente fijándose en la columna "Titulo". 
        // (Esto fue lo que añadimos para asegurar que los tests siempre comparen las listas en el mismo orden).
        .OrderBy(l => l.Titulo)
        
        // ".Select": Se llama "Proyección". Sirve para transformar la Entidad en crudo (el Libro de la base de datos) en nuestro DTO (el sobre postal).
        // Evita enviar datos privados o innecesarios que el frontend no ha pedido.
        .Select(l => new LibroParaReseñarDTO(
            l.Id, 
            l.Titulo, 
            l.TipoLibro, 
            l.Genero.Nombre, // Aquí aprovechamos el .Include de arriba para sacar directamente el nombre en texto, en lugar del ID numérico.
            l.Editorial.Nombre, // Igual aquí, sacamos el nombre de la editorial.
            l.Autor, 
            l.CalificacionMedia,
            l.FechaLanzamiento
        ))
        
        // ".ToListAsync()": MUY IMPORTANTE. Entity Framework es "perezoso". Todas las líneas anteriores solo estaban "preparando" la pregunta.
        // Es al ejecutar "ToListAsync()" cuando realmente se traduce todo a lenguaje SQL, se lanza la petición a SQLite, y se guarda en la memoria del programa.
        .ToListAsync();


    // --- MANEJO DE RESULTADOS Y RESPUESTA ---

    // "!": Significa "NO".
    // ".Any()": Pregunta si la lista tiene algún elemento. Es mucho más rápido que contar todos los elementos (.Count > 0).
    // Por tanto, la línea entera significa: "Si NO hay ningún elemento en la lista..."
    if (!librosSeleccionados.Any())
    {
        // "... usamos el Logger para dejar un mensaje escrito en la consola del servidor (para nosotros los desarrolladores)".
        // Así sabremos que a esta hora alguien hizo una búsqueda tan específica que no devolvió nada.
        _logger.LogInformation($"{DateTime.Now} Info: No se encontraron libros para reseñar con los filtros dados.");
    }

    // Finalmente, el método "Ok()" envuelve nuestra lista de libros dentro de un sobre con el sello verde de "HTTP 200 (Éxito)".
    // Automáticamente, el framework transforma nuestra lista de C# en un texto en formato JSON para que el navegador del usuario lo pueda leer.
    return Ok(librosSeleccionados);
        }
    }
}