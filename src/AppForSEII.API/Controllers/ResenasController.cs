using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    // --- ETIQUETAS DE CLASE ---
    // [Route] define la URL base para todas las funciones de este archivo.
    // Al poner "[controller]", el framework toma el nombre de la clase ("ResenasController"), le quita la palabra "Controller" y la URL queda como: api/Resenas
    [Route("api/[controller]")]
    
    // [ApiController] es una etiqueta inteligente de ASP.NET. Hace varias cosas automáticas, por ejemplo:
    // Si un DTO entra con un email mal escrito (según las Data Annotations), esta etiqueta rechaza la petición con un Error 400 antes de que el código siquiera empiece a ejecutarse.
    [ApiController]
    public class ResenasController : ControllerBase
    {
        // --- INYECCIÓN DE DEPENDENCIAS ---
        // "readonly" significa que estas variables solo se pueden asignar una vez al construir la clase, protegiéndolas de borrados accidentales.
        private readonly ApplicationDbContext _context; // Nuestra conexión directa a las tablas de la base de datos SQLite.
        private readonly ILogger<ResenasController> _logger; // Herramienta para escribir mensajes y errores en la terminal del servidor.

        // Constructor: Cuando alguien hace una petición a la API, el servidor crea una instancia de este controlador
        // y le inyecta (le pasa automáticamente) el context y el logger ya configurados y listos para usar.
        public ResenasController(ApplicationDbContext context, ILogger<ResenasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==============================================================================
        // MÉTODO 1: GET DETAILS (Caso de Uso: Ver detalles de una reseña específica)
        // ==============================================================================

        // [HttpGet] indica que esto es una consulta de solo lectura (como entrar a una página web).
        [HttpGet]
        // "[action]" añade el nombre del método a la URL final: api/Resenas/GetDetailsForResena
        [Route("[action]")]
        // Documentación Swagger: Avisamos de que podemos devolver el DTO completo (HTTP 200 OK) o un error si no existe (HTTP 404 NotFound).
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        
        // El método recibe un número entero "id" por la URL (ej: api/Resenas/GetDetailsForResena?id=1)
        public async Task<ActionResult> GetDetailsForResena(int id)
        {
            // Buscamos en la base de datos de forma asíncrona ("await")
            var resena = await _context.Resenas
                
                // ".Where": Primera criba. Queremos únicamente la reseña cuyo ID coincida con el número que nos han pasado.
                .Where(r => r.Id == id)
                
                // ".Include": Hacemos un INNER JOIN a la tabla de Usuarios para traernos quién escribió la reseña.
                .Include(r => r.Usuario)
                
                // ".Include": Hacemos otro INNER JOIN a la tabla intermedia "ResenaItems" para ver qué libros se valoraron.
                .Include(r => r.ResenaItems)
                    // ".ThenInclude": Como estamos dentro de "ResenaItems", damos un salto más profundo hacia el Libro asociado a ese ítem.
                    .ThenInclude(ri => ri.Libro)
                        // ".ThenInclude": Y desde el Libro, damos un último salto para traernos el nombre de su Editorial.
                        .ThenInclude(l => l.Editorial)
                
                // ".Select": Proyección. Transformamos todo ese amasijo de tablas SQL en nuestro elegante ResenaDetailDTO.
                .Select(r => new ResenaDetailDTO(
                    r.Id,
                    r.FechaResena,
                    r.Usuario.UserName,  // Sacado del primer .Include
                    r.Usuario.Nombre,
                    r.Usuario.Apellidos,
                    r.Usuario.Direccion,
                    r.Usuario.Telefono,
                    r.Titulo,
                    
                    // Como el DTO pide una Lista interna de ítems, abrimos otro .Select interno para transformar la colección de libros.
                    r.ResenaItems.Select(ri => new ResenaItemDTO(
                        ri.Libro.Id,
                        ri.Libro.Titulo, // Sacado del primer .ThenInclude
                        ri.Libro.Editorial.Nombre, // Sacado del segundo .ThenInclude
                        ri.Libro.TipoLibro,
                        ri.Calificacion,
                        ri.Descripcion
                    )).ToList() // .ToList() convierte este sub-select en una lista de C# real.
                ))
                // ".FirstOrDefaultAsync()": Ejecuta la consulta SQL entera y nos devuelve el primer resultado. 
                // Si el Where no encontró ningún ID válido, devuelve "null" sin dar un error del programa.
                .FirstOrDefaultAsync();

            // Si es null, significa que nos han pedido un ID que no existe (ej: ID = 999).
            if (resena == null)
            {
                // Dejamos constancia en la consola del servidor.
                _logger.LogError($"Error: La reseña con id {id} no existe");
                // Le devolvemos al cliente un Error HTTP 404 (Not Found / No Encontrado).
                return NotFound();
            }

            // Si lo encontró, lo envuelve en un código 200 (OK) y lo manda.
            return Ok(resena);
        }


        // ==============================================================================
        // MÉTODO 2: POST CREATE (Caso de Uso: Crear una nueva reseña en el sistema)
        // ==============================================================================

        // [HttpPost] indica que esta petición trae un paquete de datos oculto en el cuerpo (Body) para insertarlo en la base de datos.
        [HttpPost]
        [Route("[action]")]
        // Documentación Swagger: Avisamos de que si sale bien, devolvemos un 201 (Created). Si hay error de datos, 400 (BadRequest). Si peta el servidor, 409 (Conflict).
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        
        // Recibe automáticamente el JSON del cliente y lo convierte en el objeto "resenaForCreate" (ResenaForCreateDTO).
        public async Task<ActionResult> CreateResena(ResenaForCreateDTO resenaForCreate) 
        {
            // --- VALIDACIONES DE NEGOCIO ---
            
            // Verificamos el Flujo Alternativo 1: ¿La lista de libros viene vacía o nula?
            if (resenaForCreate.ResenaItems.Count == 0)
                // "ModelState.AddModelError" es el almacén de errores de ASP.NET. Añadimos un error manual al saco.
                ModelState.AddModelError("ResenaItems", "¡Error! Debes seleccionar al menos un libro para hacer la reseña.");

            // Verificamos si el correo electrónico (UserName) pertenece a un usuario que realmente exista en la tabla ApplicationUsers.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == resenaForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("Usuario", "¡Error! El usuario no está registrado.");

            // Si después de estas comprobaciones el saco de errores tiene algo (ErrorCount > 0)...
            if (ModelState.ErrorCount > 0)
                // Cortamos de raíz y le devolvemos al cliente un Error HTTP 400 junto con los detalles de qué hizo mal.
                return BadRequest(new ValidationProblemDetails(ModelState));

            // --- LECTURA PREVIA PARA PODER ACTUALIZAR LAS NOTAS ---
            
            // Sacamos solo los textos (Títulos) de los libros que el usuario ha mandado en su JSON.
            var titulosLibros = resenaForCreate.ResenaItems.Select(ri => ri.Titulo).ToList();
            
            // Vamos a la base de datos y cargamos esos libros en memoria.
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                // OJO: Incluimos sus reseñas anteriores (ResenaItems) porque las necesitaremos para calcular la media aritmética luego.
                .Include(l => l.ResenaItems)
                // ".Contains" en SQL se traduce como un "WHERE Titulo IN ('Dune', '1984', ...)"
                .Where(l => titulosLibros.Contains(l.Titulo))
                .ToListAsync();

            // --- PREPARACIÓN DEL OBJETO A GUARDAR ---
            
            // Creamos un nuevo objeto del Modelo "Resena" (la entidad real de la base de datos).
            Resena resena = new Resena
            {
                Usuario = user, // Enlazamos el usuario que encontramos arriba.
                FechaResena = DateTime.Now, // Le estampamos la fecha y hora actuales del servidor.
                Titulo = resenaForCreate.Titulo,
                ResenaItems = new List<ResenaItem>() // Inicializamos la lista de libros valorados vacía para irla llenando.
            };

            // Recorremos uno por uno los ítems que mandó el usuario en su petición (el DTO).
            foreach (var item in resenaForCreate.ResenaItems) 
            {
                // Buscamos si este título existe en la lista de libros que sacamos de la BD.
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);
                
                if (libro == null) 
                {
                    // Si el usuario manipuló el JSON y mandó el título de un libro que no existe.
                    ModelState.AddModelError("Libro", $"¡Error! El libro '{item.Titulo}' no existe en el sistema.");
                }
                else 
                {
                    // === CÁLCULO MATEMÁTICO DE LA NUEVA CALIFICACIÓN MEDIA ===
                    // 1. Entramos al historial (libro.ResenaItems), cogemos todas sus calificaciones antiguas, y las sumamos (.Sum).
                    // Luego a esa suma histórica, le sumamos la calificación de la reseña nueva que está entrando ahora (+ item.Calificacion).
                    var sumaCalificaciones = libro.ResenaItems.Sum(ri => (double)ri.Calificacion) + item.Calificacion;
                    
                    // 2. Contamos cuántas reseñas antiguas tenía (.Count) y le sumamos 1 (la nueva).
                    var totalResenas = libro.ResenaItems.Count + 1;
                    
                    // 3. Dividimos para sacar la media, y como Entity Framework está "vigilando" el objeto libro,
                    // al modificar su propiedad "CalificacionMedia", el sistema lanzará un "UPDATE Libros SET..." cuando guardemos.
                    libro.CalificacionMedia = (decimal)(sumaCalificaciones / totalResenas);

                    // Creamos el ítem de la tabla intermedia y lo metemos en la lista de la Reseña.
                    resena.ResenaItems.Add(new ResenaItem
                    {
                        LibroId = libro.Id, // El ID numérico del libro en la BD.
                        Resena = resena, // Le asignamos el objeto reseña padre (el que instanciamos arriba).
                        Calificacion = item.Calificacion,
                        Descripcion = item.Descripcion
                    });
                }
            }

            // Si algún libro del bucle no existía, saltará aquí este seguro y parará la inserción devolviendo un 400.
            if (ModelState.ErrorCount > 0) 
                return BadRequest(new ValidationProblemDetails(ModelState));

            // --- GUARDADO EN BASE DE DATOS ---
            
            // ".Add" coge nuestro objeto Resena entero (con todos sus ítems dentro) y le pone una marca de "Pendiente de insertar".
            _context.Add(resena);

            try{
                // ".SaveChangesAsync()" es el momento crítico. Convierte los objetos pendientes en sentencias "INSERT INTO" de SQL 
                // y los dispara contra la base de datos real. 
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) {
                // Si justo en ese milisegundo la base de datos se cayó o hubo un choque de IDs, lo atrapamos con este "catch".
                _logger.LogError(ex.Message); // Lo apuntamos en el log para investigarlo después.
                return Conflict("Error al guardar la reseña: " + ex.Message); // Y le devolvemos al usuario un Error HTTP 409 (Conflicto).
            }

            // --- RESPUESTA TRAS EL ÉXITO ---
            
            // Si el SaveChangesAsync funcionó, el objeto "resena" ya tiene un ID real asignado por SQLite.
            // Ahora, construimos manualmente un DTO "GetDetails" con toda la info, porque las buenas prácticas de las API REST
            // dictan que cuando creas algo (POST), debes devolverle al usuario el objeto recién creado tal como quedó.
            var resenaDetail = new ResenaDetailDTO(
                resena.Id, // El ID nuevecito generado por la BD.
                resena.FechaResena,
                user.UserName, 
                resenaForCreate.Nombre, 
                resenaForCreate.Apellidos, 
                resenaForCreate.Direccion, 
                resenaForCreate.Telefono, 
                resena.Titulo,
                resena.ResenaItems.Select(ri => new ResenaItemDTO(
                    ri.LibroId, 
                    // Como el libro real se guardó en nuestra variable "libros" durante la búsqueda inicial, 
                    // lo sacamos de ahí (.First) para poder poner su título y editorial en el DTO de salida sin hacer otra consulta SQL.
                    libros.First(l => l.Id == ri.LibroId).Titulo, 
                    libros.First(l => l.Id == ri.LibroId).Editorial.Nombre, 
                    libros.First(l => l.Id == ri.LibroId).TipoLibro, 
                    ri.Calificacion, 
                    ri.Descripcion
                )).ToList()
            );

            // Devolvemos HTTP 201 (Created). El primer parámetro avisa al usuario qué método (GetDetailsForResena) 
            // puede usar en el futuro para ver su reseña, el segundo es la ruta (su nuevo ID), y el tercero es el objeto DTO que acabamos de montar.
            return CreatedAtAction("GetDetailsForResena", new { id = resena.Id }, resenaDetail);
        }
    }








/*
    {
    "usuarioUserName": "peter@uclm.es", 
    "nombre": "Peter",
    "apellidos": "Jackson",
    "direccion": "Avda. España s/n, Albacete",
    "telefono": "600123456",
    "titulo": "Mi reseña de examen", 
    "resenaItems": [
        {
        "libroId": 1, 
        "titulo": "Dune",
        "editorial": "Minotauro",
        "tipoLibro": "Bolsillo",
        "calificacion": 5, 
        "descripcion": "Un clásico imprescindible. Me ha encantado."
        }
    ]
    }
*/





    
}