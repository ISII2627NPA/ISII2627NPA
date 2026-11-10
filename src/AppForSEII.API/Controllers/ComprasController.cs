using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII.API.Controllers
{
    // Indica la ruta base de la API. "[controller]" se sustituye automáticamente por "Compras" (el nombre de la clase sin "Controller").
    [Route("api/[controller]")]
    // Activa características automáticas como la validación de Data Annotations (los [Required] de tus DTOs).
    [ApiController]
    public class ComprasController : ControllerBase
    {
        // Variables de solo lectura (readonly) para evitar que se modifiquen accidentalmente durante la ejecución.
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }
        // Indica que este método responde a peticiones HTTP GET (lectura de datos).
        [HttpGet]
        [Route("[action]")]
        // Documentación para Swagger: Indica qué códigos HTTP y qué tipos de objetos puede devolver este metodo
        [ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.Created)] // Devuelve un objeto CompraDetailDTO con código 201 (Created) si la compra se creó correctamente.
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]// Devuelve un objeto ValidationProblemDetails con código 400 (Bad Request) si hay errores de validación.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)] // Devuelve un string con código 409 (Conflict) si hay conflictos, como errores al guardar la compra.
       
       // Task<ActionResult>: Indica que es un método asíncrono (async) para no bloquear el servidor mientras consulta la BD.
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            // Consulta a la base de datos usando LINQ y Entity Framework Core
            var compra = await _context.Compras

                .Where(c => c.Id == id) //Filtramos para buscar solo la compra con el ID que nos piden.

                // Include / ThenInclude: "Carga Ansiosa" 
                // Le decimos a la base de datos que no solo traiga la tabla Compras, sino que haga los JOINs 
                // necesarios para traer los ítems, libros, géneros, editoriales, usuario y método de pago asociados.
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Genero) 
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Editorial) 
                .Include(c => c.ApplicationUser) 
                .Include(c => c.MetodoPago)

                // Select: Proyección.
                // Transformamos la entidad "Compra" (pesada y vinculada a la BD) en un "CompraDetailDTO" (ligero y seguro).
                .Select(c => new CompraDetailDTO(
                    c.Id, 
                    c.FechaCompra, 
                    c.PrecioTotal, 
                    c.ApplicationUser.UserName,
                    c.ApplicationUser.Nombre, 
                    c.ApplicationUser.Apellidos,
                    c.ApplicationUser.Direccion, 
                    c.ApplicationUser.Telefono,
                    c.MetodoPagoId,
                    c.CodigoDescuento,

                    // Transformamos también la lista interna de CompraItems en CompraItemDTOs.
                    c.CompraItems.Select(ci => new CompraItemDTO(
                        ci.Libro.Id,
                        ci.Libro.Titulo, 
                        ci.Libro.Genero.Nombre, 
                        ci.Libro.Editorial.Nombre,
                        ci.Cantidad, 
                        ci.Libro.PrecioCompra
                    )).ToList<CompraItemDTO>()
                ))
                // Ejecuta la consulta de forma asíncrona y devuelve el primer resultado, o 'null' si no existe.
                .FirstOrDefaultAsync();

            // Cláusula de guarda: Si el ID no existe, registramos el error y devolvemos un error HTTP 404.
            if (compra == null)
            {
                _logger.LogError($"Error: Compra con id {id} no existe");
                return NotFound();
            }

            // Si todo va bien, devolvemos un HTTP 200 (Ok) empaquetando nuestro DTO de detalle.
            return Ok(compra); 
        }

       [HttpPost] // Indica que este método se ejecutará cuando llegue una petición HTTP tipo POST (usada para crear datos).
[Route("[action]")] // Define que la URL incluirá el nombre exacto del método ("CreateCompra").
[ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.Created)] // Documenta en Swagger que si todo va bien, devolverá un HTTP 201 (Creado) y un CompraDetailDTO.
[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)] // Documenta que si hay fallos del cliente, devolverá un HTTP 400 (Bad Request).
[ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)] // Documenta que si falla la base de datos al guardar, devolverá un HTTP 409 (Conflicto).
       
       // Método asíncrono que recibe el DTO de creación con los datos que rellenó el cliente.
        public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate) 
        {
            if (compraForCreate.CompraItems.Count == 0)// Verifica si el array de libros enviados está vacío.
                ModelState.AddModelError("CompraItems", "Error! You must include at least one book to be purchased");

            // Inicia un bucle para revisar cada libro que el cliente intenta comprar.
            foreach (var item in compraForCreate.CompraItems) {
                if (item.Cantidad <= 1) { // Verifica si el usuario puso cantidad 1 o menos
                    ModelState.AddModelError("CompraItems", $"Error! La cantidad para el libro '{item.Titulo}' debe ser mayor que 1");
                }
            }
            // Revisa si hay descuento y si no cumple la longitud de 5 a 10 letras.
            if (!string.IsNullOrEmpty(compraForCreate.CodigoDescuento) && (compraForCreate.CodigoDescuento.Length < 5 || compraForCreate.CodigoDescuento.Length > 10)) {
               // Apunta el error del código
                ModelState.AddModelError("CodigoDescuento", "Error! El código de descuento debe tener entre 5 y 10 caracteres.");
            }
           
         
            // Busca en la base de datos el usuario exacto que coincide con el UserName del DTO.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == compraForCreate.UsuarioUserName);
           
           // Si la base de datos no encontró a ese usuario...
            if (user == null)
                ModelState.AddModelError("CompraApplicationUser", "Error! UserName is not registered");
            // Busca directamente por Clave Primaria (Id) el método de pago elegido.

            var metodoPago = await _context.MetodosPago.FindAsync(compraForCreate.MetodoPagoId);
            // Si el ID del método de pago no existe en la base de datos...
            if (metodoPago == null)
            {
                ModelState.AddModelError("MetodoPagoId", "Error! El método de pago especificado no existe.");
            }

          if (ModelState.ErrorCount > 0) // ¡Punto de control! Cuenta cuántos errores hemos apuntado en total hasta ahora.
              // Si hay 1 o más errores, detiene el código aquí y le lanza el HTTP 400 al cliente con la lista de fallos.
            return BadRequest(new ValidationProblemDetails(ModelState)); 


            // Extrae una lista simple de textos (strings) solo con los títulos de los libros pedidos.
            var libroTitulos = compraForCreate.CompraItems.Select(ci => ci.Titulo).ToList<string>();

            // Empieza una consulta a la tabla completa de Libros de la BDA.
            var libros = _context.Libros
                // Filtra para quedarse SOLO con los libros cuyo título esté en la lista que acabamos de sacar arriba.
                .Where(l => libroTitulos.Contains(l.Titulo))
                // Ejecuta la consulta de golpe, trayendo todos esos libros a la memoria RAM.
                .ToList();



             // Sobrescribe el nombre del usuario de la BD con el nombre nuevo enviado en el DTO (el '!' asume que user no es nulo).
             user!.Nombre = compraForCreate.Nombre;
             user!.Apellidos = compraForCreate.Apellidos; // Sobrescribe los apellidos.
             user!.Direccion = compraForCreate.Direccion; // Sobrescribe la dirección.
             
            // Sobrescribe el teléfono. Entity Framework nota estos cambios automáticamente y los preparará para guardarlos.
             user!.Telefono = compraForCreate.Telefono;


            Compra compra = new Compra { // Construye un objeto 'Compra' en blanco, que es la entidad real mapeada a la tabla SQL.
        ApplicationUser = user, // Vincula la compra con el objeto usuario que sacamos de la BD.
        FechaCompra = DateTime.Now, // Genera la fecha y hora actuales.
        MetodoPago = metodoPago, // Vincula la compra con el objeto método de pago de la BD.
        CodigoDescuento = compraForCreate.CodigoDescuento, // Traspasa el string del descuento.
        PrecioTotal = 0, // Inicia el coste a 0 (lo calcularemos más abajo).
        CompraItems = new List<CompraItem>() // Crea una lista vacía para ir metiendo los ítems; evita errores de "Null Reference".
    };

            foreach (var item in compraForCreate.CompraItems) {// Inicia un bucle recorriendo cada ítem del DTO enviado por el cliente.

            // Busca el objeto Libro real (sacado de la BD antes) que coincida con el título del DTO.
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);

                   
               
                if (!compraForCreate.Direccion.Contains("Calle"))
                {
                   ModelState.AddModelError("DireccionCalle", "Error! La dirección de envío debe contener la palabra Calle."); 
                }

                // Comprueba si el libro no existe, o si el cliente está pidiendo más unidades de las que hay en stock.
                if (item.Cantidad > libro.Stock) {
                    //Apunta u nerror de stock
                    ModelState.AddModelError("CompraItems", $"Error! El libro '{item.Titulo}' no está disponible en la cantidad solicitada");
                }
                else {// Si todo es correcto y hay stock suficiente...
                    compra.CompraItems.Add(new CompraItem { // Añade una nueva línea ("ticket") a la compra (esto va luegoa a la base de datos)
                    LibroId = libro.Id, // Guarda el ID oficial del libro.
                    Compra = compra, // Enlaza el ítem a la compra actual (padre).
                    Cantidad = item.Cantidad // Guarda las unidades pedidas.
                    });
                    // Guarda el precio de compra oficial del libro en el ítem de la compra (para que no cambie si el precio del libro cambia en el futuro).
                    item.PrecioCompra = libro.PrecioCompra;
                    
                    //Resta la cantidad pedida al stock de ese libro
                    libro.Stock -= item.Cantidad;
                }
            }

            // Calcula el precio total de la compra sumando el precio de cada ítem multiplicado por su cantidad.
            compra.PrecioTotal = compraForCreate.CompraItems.Sum(ci => ci.PrecioCompra * ci.Cantidad);

            // Último control de seguridad: Revisa la "libreta" por si hubo errores de stock dentro del bucle foreach anterior.
            if (ModelState.ErrorCount > 0) {
                // Si alguien se quedó sin stock, aborta la operación y devuelve un HTTP 400 con la lista de errores.
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Le decimos al vigilante de Entity Framework: "Haz un seguimiento de esta compra, prepárala para guardarla".
            _context.Add(compra);

            // Iniciamos un bloque "try" (intentar) para capturar fallos técnicos incontrolables (ej. se corta el internet).
            try {
             //Aquí Entity Framework traduce todo a SQL y viaja a la base de datos para guardar la compra y actualizar el stock.
                await _context.SaveChangesAsync();
            }
            // Si la base de datos revienta o da error, el programa salta directamente a este "catch" (capturar) en lugar de colgarse.
            catch (Exception ex) {
                // Apunta el error técnico en la libreta.
                ModelState.AddModelError("Compra", $"Error! Hubo un error al guardar tu compra, por favor, inténtalo más tarde");
                // Devuelve un error HTTP 409 (Conflicto) y le pega el mensaje técnico del fallo ('ex.Message').
                return Conflict("Error" + ex.Message);
            }

            // Nunca devolvemos la Entidad directa por seguridad. Empaquetamos los datos oficiales en una "caja de salida" (DTO).
            var compraDetail = new CompraDetailDTO(
                compra.Id, 
                compra.FechaCompra,
                compra.PrecioTotal,
                compra.ApplicationUser.UserName, 
                compra.ApplicationUser.Nombre,
                compra.ApplicationUser.Apellidos,
                compra.ApplicationUser.Direccion, 
                compra.ApplicationUser.Telefono, 
                metodoPago.Id,
                compra.CodigoDescuento,
                compraForCreate.CompraItems// Devolvemos la lista de lo que ha comprado a modo de "ticket impreso".
            );

            // Genera la respuesta HTTP 201 (Creado). 
            // "GetDetailsForCompras": Le dice al cliente cómo se llama el método GET para consultar esta compra.
            // "new { id = compra.Id }": Le pasa el número de ID nuevo para que sepa buscarlo.
            // "compraDetail": Le envía en el cuerpo del mensaje el resumen del ticket que acabamos de crear.
            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }
    }
}

/*
PRUEBA EN LA API POST
{
"usuarioUserName": "peter@uclm.es",
  "nombre": "Peter",
  "apellidos": "Jackson",
  "direccion": "Calle Nueva 123",
  "telefono": "600112233",
  "metodoPagoId": 1,
  "codigoDescuento": "UCLM1234",
  "compraItems": [
    {
      "libroId": 1,
      "titulo": "Dune",
      "genero": "Ciencia Ficción",
      "editorial": "Minotauro",
      "cantidad": 2,
      "precioCompra": 0
    }
  ],
  "precioTotal": 0
}
*/
