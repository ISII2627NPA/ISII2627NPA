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
                new Genero { Nombre = "Ficción" },
                new Genero { Nombre = "Fantasía" },
                new Genero { Nombre = "Drama" }
            };

            var editoriales = new List<Editorial>() {
                new Editorial { Nombre = "Planeta" },
                new Editorial { Nombre = "Anaya" }
            };

            var libros = new List<Libro>(){
                // Libro normal disponible
                new Libro { Titulo = "El Imperio Final", Genero = generos[1], Editorial = editoriales[0], Autor = "Brandon Sanderson", FechaLanzamiento = new DateTime(2006, 7, 17), PrecioCompra = 20.0m, Stock = 10 },
                // Libro normal disponible, más barato
                new Libro { Titulo = "1984", Genero = generos[0], Editorial = editoriales[1], Autor = "George Orwell", FechaLanzamiento = new DateTime(1949, 6, 8), PrecioCompra = 15.0m, Stock = 5 },
                // Libro SIN STOCK (no debería salir en el endpoint porque filtramos por Stock > 0)
                new Libro { Titulo = "Hamlet", Genero = generos[2], Editorial = editoriales[0], Autor = "William Shakespeare", FechaLanzamiento = new DateTime(1609, 1, 1), PrecioCompra = 10.0m, Stock = 0 }
            };

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

            // Caso 1: Sin filtros. Debe devolver los que tienen stock, ordenados por Título ("1984" va antes que "El Imperio Final").
            var expectedTC1 = new List<LibroParaComprarDTO>() { libroDTOs[1], libroDTOs[0] };
            
            // Caso 2: Filtro por Género "Fantasía".
            var expectedTC2 = new List<LibroParaComprarDTO>() { libroDTOs[0] };
            
            // Caso 3: Filtro por Precio Máximo 16.0.
            var expectedTC3 = new List<LibroParaComprarDTO>() { libroDTOs[1] };

            return new List<object[]>
            {
                // Parámetros del método: { string? genero, double? precioMaximo, IList<LibroParaComprarDTO> expectedLibros }
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
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            // Act
            var result = await controller.GetLibrosParaComprar(genero, precioMaximo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var librosActual = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, librosActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            // Act: Enviamos un precio máximo negativo, lo cual debe saltar nuestra validación manual
            var result = await controller.GetLibrosParaComprar(null, -5.0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problem = problemDetails.Errors.First().Value[0];

            Assert.Equal("El precio máximo no puede ser negativo", problem);
        }
    }
}