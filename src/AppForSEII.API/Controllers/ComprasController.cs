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
        [ProducesResponseType(typeof(CompraDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            /*
            var compra = await _context.Compras
                .Where(c => c.Id == id)
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Genero) 
                .Include(c => c.CompraItems) 
                    .ThenInclude(ci => ci.Libro) 
                        .ThenInclude(libro => libro.Editorial) 
                .Include(c => c.ApplicationUser) 
                
                
                .Select(c => new CompraDetailDTO(
                    c.Id, 
                    c.FechaCompra, 
                    c.ApplicationUser.UserName, 
                    c.ApplicationUser.Nombre, 
                    c.ApplicationUser.Apellidos,
                    c.ApplicationUser.Direccion, 
                    c.ApplicationUser.Telefono,
                    (MetodoPago)c.MetodoPago,
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
*/ //Aqui hay que poner return Ok(compra); cuando se fusione CompraDetailDTO en main
            return Ok();
           
        }
    }
}