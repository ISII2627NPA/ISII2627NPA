using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs; 
using System;
using AppForSEII.API.Models;


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
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForResena(int id)
        {
            
            var resena = await _context.Resenas
                .Where(r => r.Id == id)
                .Include(r => r.Usuario)
                .Include(r => r.ResenaItems)
                    .ThenInclude(ri => ri.Libro)
                        .ThenInclude(l => l.Editorial)
                .Select(r => new ResenaDetailDTO(
                    r.Id,
                    r.FechaResena,
                    r.Usuario.UserName,
                    r.Usuario.Nombre,
                    r.Usuario.Apellidos,
                    r.Usuario.Direccion,
                    r.Usuario.Telefono,
                    r.Titulo,
                    r.ResenaItems.Select(ri => new ResenaItemDTO(
                        ri.Libro.Id,
                        ri.Libro.Titulo,
                        ri.Libro.Editorial.Nombre,
                        ri.Libro.TipoLibro,
                        ri.Calificacion,
                        ri.Descripcion
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (resena == null)
            {
                _logger.LogError($"Error: La reseña con id {id} no existe");
                return NotFound();
            }

            return Ok(resena);
            

        }










        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ResenaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateResena(ResenaForCreateDTO resenaForCreate) 
        {
            // Validación: El sistema exige que haya al menos un libro seleccionado (Flujo Alternativo 1)
            if (resenaForCreate.ResenaItems == null || resenaForCreate.ResenaItems.Count == 0)
                ModelState.AddModelError("ResenaItems", "¡Error! Debes seleccionar al menos un libro para hacer la reseña.");

            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == resenaForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("Usuario", "¡Error! El usuario no está registrado.");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var titulosLibros = resenaForCreate.ResenaItems.Select(ri => ri.Titulo).ToList();
            
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => titulosLibros.Contains(l.Titulo))
                .ToListAsync();

            Resena resena = new Resena
            {
                Usuario = user,
                FechaResena = DateTime.Now,
                Titulo = resenaForCreate.Titulo,
                ResenaItems = new List<ResenaItem>()
            };

            foreach (var item in resenaForCreate.ResenaItems) 
            {
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);
                
                if (libro == null) 
                {
                    ModelState.AddModelError("Libro", $"¡Error! El libro '{item.Titulo}' no existe en el sistema.");
                }
                else 
                {
                    resena.ResenaItems.Add(new ResenaItem
                    {
                        LibroId = libro.Id,
                        Resena = resena,
                        Calificacion = item.Calificacion,
                        Descripcion = item.Descripcion
                    });
                }
            }

            if (ModelState.ErrorCount > 0) 
                return BadRequest(new ValidationProblemDetails(ModelState));

            _context.Add(resena);

            try{
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return Conflict("Error al guardar la reseña: " + ex.Message);
            }

            var resenaDetail = new ResenaDetailDTO(
                resena.Id, 
                resena.FechaResena,
                user.UserName, 
                resenaForCreate.Nombre, 
                resenaForCreate.Apellidos, 
                resenaForCreate.Direccion, 
                resenaForCreate.Telefono, 
                resena.Titulo,
                resena.ResenaItems.Select(ri => new ResenaItemDTO(
                    ri.LibroId, 
                    libros.First(l => l.Id == ri.LibroId).Titulo, 
                    libros.First(l => l.Id == ri.LibroId).Editorial.Nombre, 
                    libros.First(l => l.Id == ri.LibroId).TipoLibro, 
                    ri.Calificacion, 
                    ri.Descripcion
                )).ToList()
            );

            return CreatedAtAction("GetDetailsForResena", new { id = resena.Id }, resenaDetail);
        }
    }
}