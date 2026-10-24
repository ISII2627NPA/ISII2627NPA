using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppForSEII.API.Data;
// using AppForSEII.API.DTOs; // TODO: Descomentar cuando los DTOs estén fusionados

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResenasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResenasController> _logger;

        public ResenasController(ApplicationDbContext context, ILogger<ResenasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        // TODO: Descomentar la siguiente línea cuando ResenaDetailDTO esté fusionado
        // [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForResena(int id)
        {
            /* TODO: Descomentar la lógica real cuando ResenaDetailDTO y ResenaItemDTO estén fusionados
            var resena = await _context.Resenas
                .Where(r => r.Id == id)
                .Include(r => r.Usuario) 
                .Include(r => r.ResenaItems) 
                    .ThenInclude(ri => ri.Libro) 
                        .ThenInclude(l => l.Editorial) 
                .Select(r => new ResenaDetailDTO
                {
                    Id = r.Id,
                    FechaResena = r.FechaResena,
                    UsuarioUserName = r.Usuario.UserName,
                    Nombre = r.Usuario.Nombre,
                    Apellidos = r.Usuario.Apellidos,
                    Direccion = r.Usuario.Direccion,
                    Telefono = r.Usuario.Telefono,
                    Titulo = r.Titulo,
                    ResenaItems = r.ResenaItems.Select(ri => new ResenaItemDTO(
                        ri.Libro.Id,
                        ri.Libro.Titulo,
                        ri.Libro.Editorial.Nombre,
                        ri.Libro.TipoLibro,
                        ri.Calificacion,
                        ri.Descripcion
                    )).ToList()
                })
                .FirstOrDefaultAsync();

            if (resena == null)
            {
                _logger.LogError($"Error: La reseña con id {id} no existe");
                return NotFound();
            }

            return Ok(resena);
            */

            // Retorno temporal para que compile la rama de la Tarea 124
            return Ok(".");
        }
    }
}