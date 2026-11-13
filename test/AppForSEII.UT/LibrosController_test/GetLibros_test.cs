using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII.UT.LibrosController_test
{
    public class GetLibros_test : AppForSEII4SqliteUT 
    {
        public GetLibros_test()
        {
            var generos = new List<Genero>() {
                new Genero("Ficción"),
                new Genero("Fantasía"),
                new Genero("Drama")
            };
            generos[0].Id = 1;
            generos[1].Id = 2;
            generos[2].Id = 3;

            var editoriales = new List<Editorial>() {
                new Editorial("Planeta"),
                new Editorial("Anaya")
            };
            editoriales[0].Id = 1;
            editoriales[1].Id = 2;

            var libro1 = new Libro("El Imperio Final", "Brandon Sanderson", new DateTime(2006, 7, 17), 20.0m, 10, editoriales[0].Id, generos[1].Id, "Bolsillo", 4.5m);
            libro1.Id = 1;
            libro1.Genero = generos[1];
            libro1.Editorial = editoriales[0];

            var libro2 = new Libro("1984", "George Orwell", new DateTime(1949, 6, 8), 15.0m, 5, editoriales[1].Id, generos[0].Id, "Bolsillo", 4.8m);
            libro2.Id = 2;
            libro2.Genero = generos[0];
            libro2.Editorial = editoriales[1];

            var libro3 = new Libro("Hamlet", "William Shakespeare", new DateTime(1609, 1, 1), 10.0m, 0, editoriales[0].Id, generos[2].Id, "Bolsillo", 5.0m);
            libro3.Id = 3;
            libro3.Genero = generos[2];
            libro3.Editorial = editoriales[0];

            var libros = new List<Libro>() { libro1, libro2, libro3 };

            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            var libroDTOs = new List<LibroParaComprarDTO>() {
                new LibroParaComprarDTO(1, "El Imperio Final", "Fantasía", "Planeta", "Brandon Sanderson", new DateTime(2006, 7, 17), 20.0, 10),
                new LibroParaComprarDTO(2, "1984", "Ficción", "Anaya", "George Orwell", new DateTime(1949, 6, 8), 15.0, 5)
            };

            var expectedTC1 = new List<LibroParaComprarDTO>() { libroDTOs[1], libroDTOs[0] };
            var expectedTC2 = new List<LibroParaComprarDTO>() { libroDTOs[0] };
            var expectedTC3 = new List<LibroParaComprarDTO>() { libroDTOs[1] };

            return new List<object[]>
            {
                new object[] { null, null, expectedTC1 },
                new object[] { "Fantasía", null, expectedTC2 },
                new object[] { null, 16.0, expectedTC3 }
            };
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_OK_test(string? genero, double? precioMaximo, IList<LibroParaComprarDTO> expectedLibros)
        {
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            var result = await controller.GetLibrosParaComprar(genero, precioMaximo);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var librosActual = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);

            Assert.Equal(expectedLibros, librosActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            var result = await controller.GetLibrosParaComprar(null, -5.0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problem = problemDetails.Errors.First().Value[0];

            Assert.Equal("El precio máximo no puede ser negativo", problem);
        }
    }
}