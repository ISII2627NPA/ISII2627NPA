using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.UT.ResenasController_test
{
    public class CreateResena_test : AppForSEII4SqliteUT
    {
        public CreateResena_test()
        {
            var user = new ApplicationUser("Peter", "Jackson", "Calle", "600") { Id = "1", UserName = "peter@uclm.es", Email = "peter@uclm.es" };
            var editorial = new Editorial { Nombre = "Minotauro" };
            var genero = new Genero { Nombre = "Ciencia Ficción" };
            
            _context.Users.Add(user);
            _context.Editoriales.Add(editorial);
            _context.Generos.Add(genero);
            _context.SaveChanges();

            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 4m) { Id = 1 };
            _context.Libros.Add(libro);
            _context.SaveChanges();
            
           var resenaPrevia = new Resena(user.Id, DateTime.Now, "Previa") { Usuario = user };
            _context.Resenas.Add(resenaPrevia);
            _context.SaveChanges();
            _context.ResenaItems.Add(new ResenaItem("Bien", 4, libro.Id, resenaPrevia.Id));
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Success_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var nuevaResena = new ResenaForCreateDTO
            {
                UsuarioUserName = "peter@uclm.es",
                Titulo = "Excelente",
                Nombre = "Peter",
                Apellidos = "Jackson",
                Direccion = "Calle",
                Telefono = "600",
                // Corregido con el nombre de clase exacto de tu proyecto y su constructor
                ResenaItems = new List<ResenaItemDTO> 
                {
                    new ResenaItemDTO(1, "Dune", "Minotauro", "Bolsillo", 5, "Genial")
                }
            };

            var result = await controller.CreateResena(nuevaResena);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var detail = Assert.IsType<ResenaDetailDTO>(createdResult.Value);
            Assert.Equal("Excelente", detail.Titulo);
            
            var libroEnBd = _context.Libros.First(l => l.Titulo == "Dune");
            Assert.Equal(4.5m, libroEnBd.CalificacionMedia); 
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Error_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var nuevaResena = new ResenaForCreateDTO
            {
                UsuarioUserName = "peter@uclm.es",
                Titulo = "Reseña vacía",
                // Corregido con el nombre de clase exacto de tu proyecto
                ResenaItems = new List<ResenaItemDTO>() 
            };

            var result = await controller.CreateResena(nuevaResena);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
            Assert.True(problemDetails.Errors.ContainsKey("ResenaItems"));
        }
    }
}