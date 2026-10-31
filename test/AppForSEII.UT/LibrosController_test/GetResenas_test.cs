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

namespace AppForSEII.UT.LibroController_test
{
    public class GetResenas_test : AppForSEII4SqliteUT
    {
        public GetResenas_test()
        {
            var editorial = new Editorial { Id = 1, Nombre = "Minotauro" };
            var generoFiccion = new Genero { Id = 1, Nombre = "Ficción" };
            var generoDrama = new Genero { Id = 2, Nombre = "Drama" };

            _context.Editoriales.Add(editorial);
            _context.Generos.AddRange(generoFiccion, generoDrama);
            _context.SaveChanges();

            var libro1 = new Libro("Dune", "Frank Herbert", new DateTime(2000, 1, 1), 20m, 10, editorial.Id, generoFiccion.Id, "Bolsillo", 4.5m) { Id = 1 };
            var libro2 = new Libro("1984", "George Orwell", new DateTime(2000, 1, 1), 25m, 10, editorial.Id, generoFiccion.Id, "Tapa dura", 4.8m) { Id = 2 };
            var libro3 = new Libro("Hamlet", "William Shakespeare", new DateTime(2000, 1, 1), 15m, 10, editorial.Id, generoDrama.Id, "Bolsillo", 5.0m) { Id = 3 };

            _context.Libros.AddRange(libro1, libro2, libro3);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesForGetLibros()
        {
            var dto1 = new LibroParaReseñarDTO(1, "Dune", "Bolsillo", "Ficción", "Minotauro", "Frank Herbert", 4.5m, new DateTime(2000, 1, 1));
            var dto2 = new LibroParaReseñarDTO(2, "1984", "Tapa dura", "Ficción", "Minotauro", "George Orwell", 4.8m, new DateTime(2000, 1, 1));
            var dto3 = new LibroParaReseñarDTO(3, "Hamlet", "Bolsillo", "Drama", "Minotauro", "William Shakespeare", 5.0m, new DateTime(2000, 1, 1));

            yield return new object[] { null!, null!, new List<LibroParaReseñarDTO> { dto2, dto1, dto3 } };
            yield return new object[] { null!, "Ficción", new List<LibroParaReseñarDTO> { dto2, dto1 } };
            yield return new object[] { "Bolsillo", null!, new List<LibroParaReseñarDTO> { dto1, dto3 } };
            yield return new object[] { "Bolsillo", "Drama", new List<LibroParaReseñarDTO> { dto3 } };
        }

        [Theory]
        [MemberData(nameof(TestCasesForGetLibros))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaReseñar_OK_test(string? tipoLibro, string? genero, List<LibroParaReseñarDTO> expectedLibros)
        {
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            var result = await controller.GetLibrosParaReseñar(tipoLibro, genero);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualLibros = Assert.IsAssignableFrom<IList<LibroParaReseñarDTO>>(okResult.Value);
            
           
            var expectedSorted = expectedLibros.OrderBy(l => l.Titulo).ToList();
            var actualSorted = actualLibros.OrderBy(l => l.Titulo).ToList();

            Assert.Equal(expectedSorted, actualSorted);
        }
    }
}