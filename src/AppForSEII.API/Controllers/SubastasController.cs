using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using AppForSEII.API.Data;   
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubastasController> _logger;

        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForSubastas(int id)
        {
            var subasta = await _context.Subastas
                .Where(s => s.Id == id)
                .Include(s => s.SubastaItems)
                    .ThenInclude(si => si.Libro)
                        .ThenInclude(l => l.Editorial)
                .Include(s => s.Usuario) 
                .Include(s => s.MetodoPago) 
    
            
            .Select(s => new SubastaDetailDTO(
                s.Id,
                s.Usuario.UserName, 
                s.Usuario.Nombre,
                s.Usuario.Apellidos,
                s.Usuario.Telefono,
                s.FechaSubasta,
                s.PrecioSubasta,
                s.MetodoPago,
                s.SubastaItems.Select(si => new SubastaItemDTO(
                    si.Libro.Id,
                    si.Libro.Titulo,
                    si.Libro.Autor,
                    si.Libro.Editorial.Nombre,
                    si.PrecioPuja,
                    si.Descripcion
                )).ToList()
            ))
            
            .FirstOrDefaultAsync();

            if (subasta == null)
            {
                _logger.LogError($"Error: Subasta con id {id} no existe");
                return NotFound();
            }

            return Ok(subasta);
            
            
        }
    }
}