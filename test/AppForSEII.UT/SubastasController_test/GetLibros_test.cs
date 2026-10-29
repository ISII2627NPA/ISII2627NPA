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

namespace AppForSEII.UT.LibroController_test
{
    public class GetLibros_test : AppForSEII4SqliteUT
    {
        public GetLibros_test()
        {
            // 1. Organizar (Arrange) general: Base de datos en memoria
            var genero = new Genero { Nombre = "Sci-Fi" }; 

            var editoriales = new List<Editorial>()
            {
                new Editorial { Nombre = "Minotauro" },
                new Editorial { Nombre = "Gigamesh" }
            };

            var libros = new List<Libro>()
            {
                // Añadimos Genero = genero a cada libro
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 08, 01), 19.99m, 50, 1, 1, "Bolsillo", 4.5m) { Editorial = editoriales[0], Genero = genero },

                new Libro("Neuromante", "William Gibson", new DateTime(1984, 07, 01), 15.50m, 20, 1, 1, "Tapa dura", 4.0m) { Editorial = editoriales[1], Genero = genero },

                new Libro("Fundacion", "Isaac Asimov", new DateTime(1951, 05, 01), 12.00m, 0, 1, 1, "Bolsillo", 4.8m) { Editorial = editoriales[0], Genero = genero }
            };

            _context.Add(genero); 
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // Método estático que provee los datos para los casos de prueba [MemberData]
        public static IEnumerable<object[]> CasosDePruebaPara_GetLibrosParaSubastar_OK()
        {
            var dtosEsperados = new List<LibroParaSubastarDTO>()
            {
                new LibroParaSubastarDTO(1, "Dune", "Frank Herbert", "Minotauro", "Bolsillo"),
                new LibroParaSubastarDTO(2, "Neuromante", "William Gibson", "Gigamesh", "Tapa dura")
                // "Fundacion" no está aquí porque tiene stock 0
            };

            // Caso 1: Sin filtros (null, null). Debe devolver Dune y Neuromante ordenados por título.
            var caso1SinFiltros = new List<LibroParaSubastarDTO> { dtosEsperados[0], dtosEsperados[1] }
                .OrderBy(l => l.Titulo).ToList();

            // Caso 2: Filtro por título "Dune". Solo devuelve Dune.
            var caso2FiltroTitulo = new List<LibroParaSubastarDTO> { dtosEsperados[0] };

            // Caso 3: Filtro por editorial "Gigamesh". Solo devuelve Neuromante.
            var caso3FiltroEditorial = new List<LibroParaSubastarDTO> { dtosEsperados[1] };

            var todosLosCasos = new List<object[]>
            {
                // Formato: filtroTitulo, filtroEditorial, listaEsperada
                new object[] { null, null, caso1SinFiltros },
                new object[] { "Dune", null, caso2FiltroTitulo },
                new object[] { null, "Gigamesh", caso3FiltroEditorial }
            };

            return todosLosCasos;
        }

        [Theory]
        [MemberData(nameof(CasosDePruebaPara_GetLibrosParaSubastar_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaSubastar_OK_test(string? filtroTitulo, string? filtroEditorial, IList<LibroParaSubastarDTO> librosEsperados)
        {
            // Organizar (Arrange)
            var simuladorMock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = simuladorMock.Object;
            var controlador = new LibroController(_context, logger);

            // Actuar (Act)
            var resultado = await controlador.GetLibrosParaSubastar(filtroTitulo, filtroEditorial);

            // Afirmar (Assert)
            var resultadoOk = Assert.IsType<OkObjectResult>(resultado);
            var dtosObtenidos = Assert.IsType<List<LibroParaSubastarDTO>>(resultadoOk.Value);

            Assert.Equal(librosEsperados, dtosObtenidos);
        }
    }
}