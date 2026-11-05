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
    // Esta clase hereda de 'AppForSEII4SqliteUT', lo que proporciona un contexto de base de datos 
    // SQLite en memoria y aislado para que cada vez que se ejecuten las pruebas, los datos partan de cero.
    public class GetLibros_test : AppForSEII4SqliteUT
    {
        // 1. Constructor (Arrange global):
        // Todo este bloque se ejecuta automáticamente antes de iniciar las pruebas.
        // Sirve para "sembrar" (poblar) la base de datos de prueba con los datos necesarios.
        public GetLibros_test()
        {
            // Creamos dependencias básicas: un género y un par de editoriales.
            var genero = new Genero { Nombre = "Sci-Fi" }; 

            var editoriales = new List<Editorial>()
            {
                new Editorial { Nombre = "Minotauro" },
                new Editorial { Nombre = "Gigamesh" }
            };
            // Creamos un catálogo de 3 libros para nuestras pruebas.
            var libros = new List<Libro>()
            {
                // Libro 1: Stock 50. Será válido para subastar.
                new Libro("Dune", "Frank Herbert", new DateTime(1965, 08, 01), 19.99m, 50, 1, 1, "Bolsillo", 4.5m) { Editorial = editoriales[0], Genero = genero },
                // Libro 2: Stock 20. Será válido para subastar.
                new Libro("Neuromante", "William Gibson", new DateTime(1984, 07, 01), 15.50m, 20, 1, 1, "Tapa dura", 4.0m) { Editorial = editoriales[1], Genero = genero },
                // Libro 3: Stock 0. MUY IMPORTANTE. Se crea para comprobar que el controlador 
                // cumple la regla de negocio (Flujo Alternativo 0) y NO lo devuelve en las consultas.
                new Libro("Fundacion", "Isaac Asimov", new DateTime(1951, 05, 01), 12.00m, 0, 1, 1, "Bolsillo", 4.8m) { Editorial = editoriales[0], Genero = genero }
            };
            // Guardamos todo en la base de datos de prueba.
            _context.Add(genero); 
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // 2. Método proveedor de datos (MemberData):
        // Prepara los distintos escenarios (filtros) que le vamos a pasar a la prueba.
        public static IEnumerable<object[]> CasosDePruebaPara_GetLibrosParaSubastar_OK()
        {
            // Primero, definimos cómo se verían los DTOs que esperamos que el sistema devuelva.
            // Fíjate que "Fundacion" no se incluye aquí porque su stock es 0.
            var dtosEsperados = new List<LibroParaSubastarDTO>()
            {
                new LibroParaSubastarDTO(1, "Dune", "Frank Herbert", "Minotauro", "Bolsillo"),
                new LibroParaSubastarDTO(2, "Neuromante", "William Gibson", "Gigamesh", "Tapa dura")
                // "Fundacion" no está aquí porque tiene stock 0
            };

            // Escenario 1: El usuario entra sin poner filtros (titulo = null, editorial = null). 
            // Esperamos que devuelva Dune y Neuromante, y los ordenamos alfabéticamente por título 
            // para que coincidan exactamente con el orden que devolverá el controlador.
            var caso1SinFiltros = new List<LibroParaSubastarDTO> { dtosEsperados[0], dtosEsperados[1] }
                .OrderBy(l => l.Titulo).ToList();

            // Escenario 2: El usuario filtra buscando el título "Dune".
            // Esperamos una lista que solo contenga el DTO de Dune.
            var caso2FiltroTitulo = new List<LibroParaSubastarDTO> { dtosEsperados[0] };

            // Escenario 3: El usuario filtra buscando la editorial "Gigamesh".
            // Esperamos una lista que solo contenga el DTO de Neuromante.
            var caso3FiltroEditorial = new List<LibroParaSubastarDTO> { dtosEsperados[1] };
            // Empaquetamos los tres escenarios. Cada array contiene: [FiltroTitulo, FiltroEditorial, ListaEsperada]
            var todosLosCasos = new List<object[]>
            {
                // Formato: filtroTitulo, filtroEditorial, listaEsperada
                new object[] { null, null, caso1SinFiltros },
                new object[] { "Dune", null, caso2FiltroTitulo },
                new object[] { null, "Gigamesh", caso3FiltroEditorial }
            };

            return todosLosCasos;   // Se lo mandamos al test.
        }

        // 3. Test parametrizado (Flujos de éxito):
        // El atributo [Theory] indica que este test se ejecutará múltiples veces (una por cada escenario del método anterior).
        [Theory]
        [MemberData(nameof(CasosDePruebaPara_GetLibrosParaSubastar_OK))]   // Conecta el test con los datos de arriba.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        // Recibe los tres parámetros empaquetados en cada escenario: el título a buscar, la editorial a buscar y el resultado correcto.
        public async Task GetLibrosParaSubastar_OK_test(string? filtroTitulo, string? filtroEditorial, IList<LibroParaSubastarDTO> librosEsperados)
        {
            // ARRANGE (Organizar): Preparamos el controlador.
            // Usamos Mock para inyectar un ILogger falso, ya que no nos interesa probar el registro de logs.
            var simuladorMock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = simuladorMock.Object;
            var controlador = new LibroController(_context, logger);

            // ACT (Actuar): Llamamos al método GET del controlador pasándole los filtros del escenario actual.
            var resultado = await controlador.GetLibrosParaSubastar(filtroTitulo, filtroEditorial);

            // ASSERT (Afirmar): Comprobamos que el controlador hizo su trabajo.
            // 1. Verificamos que devuelve un código HTTP 200 OK.
            var resultadoOk = Assert.IsType<OkObjectResult>(resultado);
            // 2. Extraemos la lista de DTOs del cuerpo de la respuesta.
            var dtosObtenidos = Assert.IsType<List<LibroParaSubastarDTO>>(resultadoOk.Value);
            // 3. Comparamos la lista que esperábamos con la que devolvió el controlador.
            // Gracias a que sobreescribiste el método 'Equals' en el DTO, Assert.Equal validará el contenido de los libros 
            // uno por uno en lugar de comparar espacios de memoria.
            Assert.Equal(librosEsperados, dtosObtenidos);
        }
    }
}