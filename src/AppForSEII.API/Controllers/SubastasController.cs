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



        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            if (subastaForCreate.SubastaItems == null || subastaForCreate.SubastaItems.Count == 0)
                ModelState.AddModelError("SubastaItems", "Error! Debe incluir al menos un libro en la lista de pujas.");

            foreach (var item in subastaForCreate.SubastaItems)
            {
                if (item.PrecioPuja < 0)
                    ModelState.AddModelError("PrecioPuja", $"Error! El precio de puja para el libro '{item.Titulo}' no puede ser menor que 0.");
            }

            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == subastaForCreate.UsuarioUserName);
            if (user == null)
                ModelState.AddModelError("Usuario", "Error! El usuario no está registrado.");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            user.Nombre = subastaForCreate.Nombre;
            user.Apellidos = subastaForCreate.Apellidos;
            user.Telefono = subastaForCreate.Telefono;

            var bookTitles = subastaForCreate.SubastaItems.Select(si => si.Titulo).ToList();
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => bookTitles.Contains(l.Titulo))
                .ToListAsync();

            Subasta subasta = new Subasta
            {
                Usuario = user,
                FechaSubasta = DateTime.Now,
                MetodoPago = subastaForCreate.MetodoPago,
                SubastaItems = new List<SubastaItem>(),
                PrecioSubasta = 0
            };

            foreach (var item in subastaForCreate.SubastaItems)
            {
                var libro = libros.FirstOrDefault(l => l.Titulo == item.Titulo);

                if (libro == null)
                {
                    ModelState.AddModelError("Libro", $"Error! El libro '{item.Titulo}' no está disponible para subastar.");
                }
                else
                {
                    subasta.SubastaItems.Add(new SubastaItem
                    {
                        LibroId = libro.Id,
                        Subasta = subasta,
                        PrecioPuja = item.PrecioPuja,
                        Descripcion = item.Descripcion
                    });
                }
            }

            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            _context.Add(subasta);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Conflict("Error al guardar la subasta: " + ex.Message);
            }

            var subastaDetail = new SubastaDetailDTO(
                subasta.Id,
                user.UserName,
                user.Nombre,
                user.Apellidos,
                user.Telefono,
                subasta.FechaSubasta,
                subasta.PrecioSubasta,
                subasta.MetodoPago,
                subasta.SubastaItems.Select(si => new SubastaItemDTO(
                    si.LibroId,
                    libros.First(l => l.Id == si.LibroId).Titulo,
                    libros.First(l => l.Id == si.LibroId).Autor,
                    libros.First(l => l.Id == si.LibroId).Editorial.Nombre,
                    si.PrecioPuja,
                    si.Descripcion
                )).ToList()
            );

            return CreatedAtAction("GetDetailsForSubastas", new { id = subasta.Id }, subastaDetail);
        }
    }
}