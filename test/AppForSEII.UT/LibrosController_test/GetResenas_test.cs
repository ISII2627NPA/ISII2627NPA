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
        // CONSTRUCTOR: Insertamos 3 libros distintos (con diferentes géneros y tipos) para poder poner a prueba los filtros de búsqueda.
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

        // --- TEST CASES (Datos de entrada paramétricos) ---
        // Genera los distintos escenarios que simulan búsquedas del usuario.
        public static IEnumerable<object[]> TestCasesForGetLibros()
        {
            // Recreamos los DTOs de salida tal y como deben quedar.
            var dto1 = new LibroParaReseñarDTO(1, "Dune", "Bolsillo", "Ficción", "Minotauro", "Frank Herbert", 4.5m, new DateTime(2000, 1, 1));
            var dto2 = new LibroParaReseñarDTO(2, "1984", "Tapa dura", "Ficción", "Minotauro", "George Orwell", 4.8m, new DateTime(2000, 1, 1));
            var dto3 = new LibroParaReseñarDTO(3, "Hamlet", "Bolsillo", "Drama", "Minotauro", "William Shakespeare", 5.0m, new DateTime(2000, 1, 1));

            // Cada "yield return" es una batería de pruebas. 
            // Parámetro 1: filtro de Tipo, Parámetro 2: filtro de Género, Parámetro 3: La lista que ESPERAMOS que devuelva el controlador.
            
            // Escenario 1: Sin filtros (null, null). Esperamos todos los libros.
            yield return new object[] { null!, null!, new List<LibroParaReseñarDTO> { dto2, dto1, dto3 } };
            // Escenario 2: Filtrar solo por Ficción. Esperamos que excluya Hamlet.
            yield return new object[] { null!, "Ficción", new List<LibroParaReseñarDTO> { dto2, dto1 } };
            // Escenario 3: Filtrar solo por Bolsillo. Esperamos que excluya 1984.
            yield return new object[] { "Bolsillo", null!, new List<LibroParaReseñarDTO> { dto1, dto3 } };
            // Escenario 4: Filtrar por Bolsillo Y Drama a la vez. Esperamos solo Hamlet.
            yield return new object[] { "Bolsillo", "Drama", new List<LibroParaReseñarDTO> { dto3 } };
        }

        // [Theory] le indica al runner que este test se va a ejecutar varias veces inyectándole datos.
        [Theory]
        // Le decimos de qué método sacar esos datos (el que creamos arriba).
        [MemberData(nameof(TestCasesForGetLibros))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaReseñar_OK_test(string? tipoLibro, string? genero, List<LibroParaReseñarDTO> expectedLibros)
        {
            // --- ARRANGE ---
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            // --- ACT ---
            // Le pasamos al controlador los filtros que toquen en esta vuelta de tuerca.
            var result = await controller.GetLibrosParaReseñar(tipoLibro, genero);

            // --- ASSERT ---
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualLibros = Assert.IsAssignableFrom<IList<LibroParaReseñarDTO>>(okResult.Value);
            
            // TRUCO DEL SCRUM MASTER:
            // Assert.Equal comparando listas exige que los elementos estén EN LA MISMA POSICIÓN exacta (índice 0 con índice 0).
            // Para asegurar que el test es de hierro, cogemos ambas listas (la inventada aquí y la real que dio el controlador)
            // y usamos LINQ para reordenarlas alfabéticamente antes de compararlas.
            var expectedSorted = expectedLibros.OrderBy(l => l.Titulo).ToList();
            var actualSorted = actualLibros.OrderBy(l => l.Titulo).ToList();

            // Ahora sí, evalúa que ambas listas son idénticas en contenido y en orden.
            Assert.Equal(expectedSorted, actualSorted);
        }
    }
}