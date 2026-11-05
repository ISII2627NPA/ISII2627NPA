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