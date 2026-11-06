using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]      // Define la ruta base para este controlador. Al usar "[controller]", la ruta será dinámicamente "/api/Subastas".
    [ApiController]        // Aplica comportamientos automáticos para APIs (como responder automáticamente con errores 400 si falla la validación de un modelo).
    public class SubastasController : ControllerBase
    {
        // Variables de solo lectura para almacenar las dependencias inyectadas.
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubastasController> _logger;

        // Constructor donde el contenedor de dependencias de ASP.NET Core inyecta el contexto de la BD y el servicio de registro (logger).
        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]        // Indica que este endpoint se ejecutará cuando reciba una petición HTTP GET.
        [Route("[action]")]        // Añade el nombre del método a la ruta. La URL final será "/api/Subastas/GetDetailsForSubastas".
        // Documentación para Swagger: Indica qué tipos de respuestas puede devolver este endpoint.
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]      // Devuelve el DTO con un 200 OK si encuentra la subasta.
        [ProducesResponseType((int)HttpStatusCode.NotFound)]      // Devuelve un error 404 si el ID no existe.
        public async Task<ActionResult> GetDetailsForSubastas(int id)      // Método asíncrono que recibe el 'id' de la subasta a consultar.
        {
            var subasta = await _context.Subastas        // Consulta asíncrona a la base de datos usando Entity Framework Core y LINQ.
                .Where(s => s.Id == id)          // 1. Filtro inicial: Buscamos únicamente la subasta cuyo ID coincida con el parámetro recibido.
                                                 // 2. Eager Loading (Carga de relaciones): Hacemos JOINs explícitos con las tablas relacionadas.
                                                 // Nota: Al usar .Select() más abajo, EF Core a menudo infiere estos JOINs automáticamente, 
                                                 // pero dejarlos explícitos ayuda a la legibilidad y asegura la carga de toda la jerarquía.
                .Include(s => s.SubastaItems)
                    .ThenInclude(si => si.Libro)      // Navegamos al Libro dentro de cada Item...
                        .ThenInclude(l => l.Editorial)      // ...y a la Editorial dentro de cada Libro.
                .Include(s => s.Usuario)       // Cargamos los datos del usuario creador.
                .Include(s => s.MetodoPago)    // Cargamos el método de pago asociado.

            // 3. Proyección (Mapeo): Transformamos la entidad de base de datos 'Subasta' en un 'SubastaDetailDTO'.
            .Select(s => new SubastaDetailDTO(
                s.Id,
                s.Usuario.UserName,      // Extraemos los datos navegando por las propiedades del IdentityUser.
                s.Usuario.Nombre,
                s.Usuario.Apellidos,
                s.Usuario.Telefono,
                s.FechaSubasta,
                s.PrecioSubasta,
                s.MetodoPago.Id,
                s.SubastaItems.Select(si => new SubastaItemDTO(     // Sub-proyección: Convertimos la lista de entidades 'SubastaItem' en una lista de 'SubastaItemDTO'.
                    si.Libro.Id,
                    si.Libro.Titulo,
                    si.Libro.Autor,
                    si.Libro.Editorial.Nombre,     // Accedemos al nombre de la editorial gracias a la relación.
                    si.PrecioPuja,
                    si.Descripcion
                )).ToList()      // Materializa esta sub-lista en memoria.
            ))
            // 4. Ejecución: Envía la consulta SQL a la base de datos y devuelve el primer resultado encontrado,
            // o 'null' si la consulta no devuelve ninguna fila para ese ID.
            .FirstOrDefaultAsync();

            if (subasta == null)     // 5. Flujo Alternativo: Comprobamos si la subasta no fue encontrada en la base de datos.
            {
                _logger.LogError($"Error: Subasta con id {id} no existe");   // Dejamos un registro en el servidor (consola/archivo) indicando qué ID causó el error.
                return NotFound();   // Devolvemos la respuesta HTTP 404 (Not Found) al cliente.
            }

            return Ok(subasta);   // 6. Flujo Básico (Éxito): Devolvemos la subasta mapeada en formato JSON con un código HTTP 200.


        }



        [HttpPost]    // Define que este endpoint responderá a peticiones HTTP de tipo POST (usado para crear nuevos recursos).
        [Route("[action]")]      // Construye la URL dinámicamente usando el nombre del método (ej. /api/Subastas/CreateSubasta).
        // Documentación para Swagger sobre los posibles códigos HTTP que puede devolver este método:
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]    // 201: Creado con éxito, devuelve el DTO con los detalles.
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]     // 400: Error de validación en los datos enviados.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]     // 409: Conflicto al guardar en la base de datos.
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            // 1. Validación (Flujo Alternativo): Comprueba que la lista de libros a subastar no esté vacía o nula.
            if (subastaForCreate.SubastaItems.Count == 0)
                // ModelState acumula los errores de validación. Clave "SubastaItems", Valor "mensaje de error".
                ModelState.AddModelError("SubastaItems", "Error! Debe incluir al menos un libro en la lista de pujas.");

            // 2. Validación (Flujo Alternativo): Recorre la lista de pujas para verificar reglas de negocio individuales.
            foreach (var item in subastaForCreate.SubastaItems)
            {
                if (item.PrecioPuja < 0)    // El precio de la puja no puede ser negativo.
                    ModelState.AddModelError("PrecioPuja", $"Error! El precio de puja para el libro '{item.Titulo}' no puede ser menor que 0.");
            }

            // 3. Validación de Usuario: Busca al usuario en la BD usando el email (UserName) que viene en el DTO.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == subastaForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("Usuario", "Error! El usuario no está registrado.");

            // 4. Validación del Método de Pago: Verifica que el ID del método de pago enviado realmente exista en la BD.
            var metodoPago = await _context.MetodosPago.FirstOrDefaultAsync(mp => mp.Id == subastaForCreate.MetodoPago);
            if (metodoPago == null)
            {
                ModelState.AddModelError("MetodoPago", "¡Error! El método de pago seleccionado no existe.");
            }

            // 5. Primer chequeo de errores: Si alguna de las validaciones anteriores falló, detenemos la ejecución aquí.
            // Devolvemos un error 400 (BadRequest) con todos los mensajes acumulados en el ModelState.
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // 6. Actualización de datos (Flujo Básico): Actualizamos los datos personales del usuario con los introducidos en el formulario.
            user.Nombre = subastaForCreate.Nombre;
            user.Apellidos = subastaForCreate.Apellidos;
            user.Telefono = subastaForCreate.Telefono;

            // 7. Optimización de consulta: Extraemos todos los títulos de los libros que vienen en el DTO.
            var bookTitles = subastaForCreate.SubastaItems.Select(si => si.Titulo).ToList();
            // Hacemos UNA sola consulta a la BD para traernos todos los libros que coincidan con esos títulos, incluyendo su Editorial.
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => bookTitles.Contains(l.Titulo))
                .ToListAsync();

            // 8. Construcción de la entidad principal (Subasta) que vamos a guardar en la base de datos.
            Subasta subasta = new Subasta
            {
                Usuario = user,    // Vinculamos la subasta con el usuario recuperado.
                FechaSubasta = DateTime.Now,    // Asignamos la fecha actual.
                MetodoPago = metodoPago,     // Vinculamos el método de pago recuperado.
                SubastaItems = new List<SubastaItem>(),    // Inicializamos la lista de items vacía.
                PrecioSubasta = 0    // Inicializamos el total a 0 (lo calcularemos más abajo).
            };

            // 9. Procesamiento de los items: Recorremos los datos que nos envió el usuario.
            foreach (var item in subastaForCreate.SubastaItems)
            {
                // Buscamos el libro en la lista que trajimos de la BD en el paso 7.
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);

                if (libro == null)
                {
                    // Flujo Alternativo: Si el usuario mandó un título que no existe en nuestra BD, añadimos un error.
                    ModelState.AddModelError("Libro", $"Error! El libro '{item.Titulo}' no está disponible para subastar.");
                }
                else
                {
                    // Si el libro existe, creamos el 'SubastaItem' (relación N:M) y lo añadimos a la subasta.
                    subasta.SubastaItems.Add(new SubastaItem
                    {
                        LibroId = libro.Id,   // Clave foránea al libro.
                        Subasta = subasta,     // Referencia a la subasta padre.
                        PrecioPuja = item.PrecioPuja,     // Dato del DTO.
                        Descripcion = item.Descripcion     // Dato del DTO.
                    });
                }
            }
            // 10. Cálculo del total: Sumamos el PrecioPuja de todos los items válidos y se lo asignamos a la subasta.
            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);
            // 11. Segundo chequeo de errores: Si algún libro no existía (paso 9), devolvemos BadRequest antes de tocar la BD.
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // 12. Guardado en Base de Datos: Añadimos la subasta al contexto (EF Core rastrea el objeto).
            _context.Add(subasta);

            try
            {
                // Guardamos los cambios físicamente en la BD de forma asíncrona.
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si hay un error a nivel de SQL (ej. violación de claves foráneas), lo registramos y devolvemos Error 409.
                _logger.LogError(ex.Message);
                return Conflict("Error al guardar la subasta: " + ex.Message);
            }
            // 13. Proyección (Flujo Básico, Paso Final): Tras guardar con éxito, construimos el DTO de respuesta detallado.
            var subastaDetail = new SubastaDetailDTO(
                subasta.Id,   // El ID ya ha sido generado automáticamente por la base de datos tras el SaveChanges.
                user.UserName,
                user.Nombre,
                user.Apellidos,
                user.Telefono,
                subasta.FechaSubasta,
                subasta.PrecioSubasta,
                subasta.MetodoPago.Id,
                subasta.SubastaItems.Select(si => new SubastaItemDTO(   // Mapeamos los items guardados al formato DTO para la respuesta.
                    si.LibroId,
                    libros.First(l => l.Id == si.LibroId).Titulo,   // Recuperamos los datos extra de la lista de libros en memoria.
                    libros.First(l => l.Id == si.LibroId).Autor,
                    libros.First(l => l.Id == si.LibroId).Editorial.Nombre,
                    si.PrecioPuja,
                    si.Descripcion
                )).ToList()
            );
            // 14. Retorno HTTP 201 (Created): Según los estándares REST, indicamos la ruta del método GET ("GetDetailsForSubastas") 
            // donde se puede consultar este nuevo recurso, el ID recién creado, y el objeto DTO en el cuerpo del mensaje.
            return CreatedAtAction("GetDetailsForSubastas", new { id = subasta.Id }, subastaDetail);
        }

        //Datos para probar el post de subastas
        /*
        {
            "usuarioUserName": "peter@uclm.es",
            "nombre": "Peter",
            "apellidos": "Caballero",
            "telefono": "666666666",
            "metodoPago": 1,
            "subastaItems": [
                {
                    "libroId": 1,
                    "titulo": "Dune",
                    "autor": "Frank Herbert",
                    "editorial": "Minotauro",
                    "precioPuja": 25.50,
                    "descripcion": "Libro en perfecto estado, casi sin uso."
                }
            ]
        }
        */
    }
}