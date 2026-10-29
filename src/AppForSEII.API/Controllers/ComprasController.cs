using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(ApplicationDbContext context, ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
       
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            var compra = await _context.Compras
                .Where(c => c.Id == id)
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Genero) 
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Editorial) 
                .Include(c => c.ApplicationUser) 
                .Include(c => c.MetodoPago)
                .Select(c => new CompraDetailDTO(
                    c.Id, 
                    c.FechaCompra, 
                    c.PrecioTotal, // <--- Añadido el precio total aquí
                    c.ApplicationUser.UserName,
                    c.ApplicationUser.Nombre, 
                    c.ApplicationUser.Apellidos,
                    c.ApplicationUser.Direccion, 
                    c.ApplicationUser.Telefono,
                    c.MetodoPagoId,
                    c.CodigoDescuento,
                    c.CompraItems.Select(ci => new CompraItemDTO(
                        ci.Libro.Id,
                        ci.Libro.Titulo, 
                        ci.Libro.Genero.Nombre, 
                        ci.Libro.Editorial.Nombre,
                        ci.Cantidad, 
                        ci.Libro.PrecioCompra
                    )).ToList<CompraItemDTO>()
                ))
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                _logger.LogError($"Error: Compra con id {id} no existe");
                return NotFound();
            }

            return Ok(compra); // <--- Actualizado para devolver el objeto
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateCompra(CompraForCreateDTO compraForCreate) 
        {
            if (compraForCreate.CompraItems.Count == 0)
                ModelState.AddModelError("CompraItems", "Error! You must include at least one book to be purchased");

            foreach (var item in compraForCreate.CompraItems) {
                if (item.Cantidad <= 1) { 
                    ModelState.AddModelError("CompraItems", $"Error! La cantidad para el libro '{item.Titulo}' debe ser mayor que 1");
                }
            }

            if (!string.IsNullOrEmpty(compraForCreate.CodigoDescuento) && (compraForCreate.CodigoDescuento.Length < 5 || compraForCreate.CodigoDescuento.Length > 10)) {
                ModelState.AddModelError("CodigoDescuento", "Error! El código de descuento debe tener entre 5 y 10 caracteres.");
            }
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == compraForCreate.UsuarioUserName);
           
            if (user == null)
                ModelState.AddModelError("CompraApplicationUser", "Error! UserName is not registered");
            var metodoPago = await _context.MetodosPago.FindAsync(compraForCreate.MetodoPagoId);
            if (metodoPago == null)
            {
                ModelState.AddModelError("MetodoPagoId", "Error! El método de pago especificado no existe.");
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var libroTitulos = compraForCreate.CompraItems.Select(ci => ci.Titulo).ToList<string>();

            var libros = _context.Libros
                .Where(l => libroTitulos.Contains(l.Titulo))
                .ToList();

            user!.Nombre = compraForCreate.Nombre;
            user!.Apellidos = compraForCreate.Apellidos;
            user!.Direccion = compraForCreate.Direccion;
            user!.Telefono = compraForCreate.Telefono;

            Compra compra = new Compra {
                ApplicationUser = user,
                FechaCompra = DateTime.Now,
                MetodoPago = metodoPago,
                CodigoDescuento = compraForCreate.CodigoDescuento,
                PrecioTotal = 0,
                CompraItems = new List<CompraItem>()
            };

            foreach (var item in compraForCreate.CompraItems) {
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);
                
                if ((libro == null) || (item.Cantidad > libro.Stock)) {
                    ModelState.AddModelError("CompraItems", $"Error! El libro '{item.Titulo}' no está disponible en la cantidad solicitada");
                }
                else {
                    compra.CompraItems.Add(new CompraItem {
                        LibroId = libro.Id,
                        Compra = compra,
                        Cantidad = item.Cantidad
                    });
                    item.PrecioCompra = libro.PrecioCompra;
                    
                    libro.Stock -= item.Cantidad;
                }
            }

            compra.PrecioTotal = compraForCreate.CompraItems.Sum(ci => ci.PrecioCompra * ci.Cantidad);

            if (ModelState.ErrorCount > 0) {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            _context.Add(compra);

            try {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) {
                ModelState.AddModelError("Compra", $"Error! Hubo un error al guardar tu compra, por favor, inténtalo más tarde");
                return Conflict("Error" + ex.Message);
            }

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
                compraForCreate.CompraItems
            );

            return CreatedAtAction("GetDetailsForCompras", new { id = compra.Id }, compraDetail);
        }
    }
}