using System;
using System.Collections.Generic;
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
    public class GetDetailsForResena_test : AppForSEII4SqliteUT
    {
        // Arrange: Preparamos la BD en memoria
        public GetDetailsForResena_test()
        {
            // 1. Asignamos un Id manual ("1") al usuario
            var user = new ApplicationUser("Peter", "Jackson", "Calle", "600") { Id = "1", UserName = "peter@uclm.es", Email = "peter@uclm.es" };
            var editorial = new Editorial { Nombre = "Minotauro" };
            var genero = new Genero { Nombre = "Ciencia Ficción" };
            _context.Users.Add(user);
            _context.Editoriales.Add(editorial);
            _context.Generos.Add(genero);
            _context.SaveChanges();

            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 5m);
            _context.Libros.Add(libro);
            _context.SaveChanges();

            // 2. Le pasamos el objeto 'user' completo a la propiedad Usuario
            var resena = new Resena(user.Id, new DateTime(2026, 1, 1), "Mi reseña") { Usuario = user };
            _context.Resenas.Add(resena);
            _context.SaveChanges();

            var resenaItem = new ResenaItem("Buena", 5, libro.Id, resena.Id);
            _context.ResenaItems.Add(resenaItem);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_Found_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);
            
            var expectedItem = new ResenaItemDTO(1, "Dune", "Minotauro", "Bolsillo", 5, "Buena");
            var expectedResena = new ResenaDetailDTO(1, new DateTime(2026, 1, 1), "peter@uclm.es", "Peter", "Jackson", "Calle", "600", "Mi reseña", new List<ResenaItemDTO> { expectedItem });

            var result = await controller.GetDetailsForResena(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResena = Assert.IsType<ResenaDetailDTO>(okResult.Value);
            Assert.Equal(expectedResena, actualResena);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_NotFound_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var result = await controller.GetDetailsForResena(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}