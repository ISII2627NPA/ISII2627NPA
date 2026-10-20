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
        // TODO: Descomentar esta línea cuando el CompraDetailDTO esté fusionado en main
        // [ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            var compra = await _context.Compras
                .Where(c => c.Id == id)
                .Include(c => c.CompraItems) // join table CompraItems
                    .ThenInclude(ci => ci.Libro) // then join table Libros
                        .ThenInclude(libro => libro.Genero) // then join table Genero
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Editorial) // then join table Editorial
                .Include(c => c.ApplicationUser) // join table ApplicationUser
                
                /* TODO: Descomentar todo este bloque Select cuando los DTOs existan en la rama principal
                .Select(c => new CompraDetailDTO(
                    c.Id, 
                    c.ApplicationUser.Nombre, 
                    c.ApplicationUser.Apellidos,
                    c.ApplicationUser.Direccion, 
                    c.ApplicationUser.Telefono,
                    c.FechaCompra, 
                    c.PrecioTotal,
                    (MetodoPago)c.MetodoPago,
                    c.CodigoDescuento,
                    c.CompraItems.Select(ci => new CompraItemDTO(
                        ci.Libro.Titulo, 
                        ci.Libro.Genero.Nombre, 
                        ci.Libro.Editorial.Nombre,
                        ci.Cantidad, 
                        ci.Libro.PrecioCompra
                    )).ToList<CompraItemDTO>()
                ))
                */
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                _logger.LogError($"Error: Compra con id {id} no existe");
                return NotFound();
            }

            // TODO: Cuando descomentes el Select de arriba, cambia este return a: return Ok(compra);
            // De momento devolvemos un Ok vacío para que el código compile y pase los checks de GitHub.
            return Ok();
        }
    }
}