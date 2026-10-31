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
        public GetDetailsForResena_test()
        {
            var user = new ApplicationUser("Peter", "Jackson", "Calle", "600") { Id = "1", UserName = "peter@uclm.es", Email = "peter@uclm.es" };
            var editorial = new Editorial { Id = 1, Nombre = "Minotauro" };
            var genero = new Genero { Id = 1, Nombre = "Ciencia Ficción" };

            _context.Users.Add(user);
            _context.Editoriales.Add(editorial);
            _context.Generos.Add(genero);
            _context.SaveChanges();

            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 5m) { Id = 1 };
            _context.Libros.Add(libro);
            _context.SaveChanges();

            var resena = new Resena(user.Id, new DateTime(2023, 1, 1), "Reseña de prueba") { Id = 1, Usuario = user };
            _context.Resenas.Add(resena);
            _context.SaveChanges();

           
            var resenaItem = new ResenaItem("Genial, me ha encantado el libro", 5, libro.Id, resena.Id) { Libro = libro, Resena = resena };
            _context.ResenaItems.Add(resenaItem);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_Found_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var expectedItems = new List<ResenaItemDTO>
            {
                
                new ResenaItemDTO(1, "Dune", "Minotauro", "Bolsillo", 5, "Genial, me ha encantado el libro")
            };

            
            var expectedDetail = new ResenaDetailDTO(
                1,
                new DateTime(2023, 1, 1),
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle",
                "600",
                "Reseña de prueba",
                expectedItems
            );

            var result = await controller.GetDetailsForResena(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDetail = Assert.IsType<ResenaDetailDTO>(okResult.Value);

            
            Assert.Equal(expectedDetail, actualDetail);
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