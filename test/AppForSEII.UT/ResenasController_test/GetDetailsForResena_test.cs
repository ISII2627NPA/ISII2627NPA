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
        // CONSTRUCTOR: Preparamos la BD en memoria con una reseña completa (con un libro dentro).
        public GetDetailsForResena_test()
        {
            // (La creación de los datos base es idéntica al archivo anterior).
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

            // Truco: Usamos una fecha estática ("new DateTime(2023, 1, 1)") en vez de "DateTime.Now"
            // para evitar que el test falle luego si hay milisegundos de diferencia al comparar fechas.
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
            // --- ARRANGE ---
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            // Construimos a mano la respuesta PERFECTA que esperamos que nos devuelva el controlador.
            // Para que no salten fallos, usamos EXACTAMENTE el orden de constructores que definiste en tus DTOs.
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

            // --- ACT ---
            // Le pedimos al controlador la reseña 1 que guardamos en el constructor.
            var result = await controller.GetDetailsForResena(1);

            // --- ASSERT ---
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDetail = Assert.IsType<ResenaDetailDTO>(okResult.Value);

            // LA CLAVE DEL ÉXITO: 
            // Como "ResenaDetailDTO" y "ResenaItemDTO" tienen el método Equals sobrescrito (usando SequenceEqual), 
            // este Assert compara campo por campo y lista por lista. Si no tuvieras el Equals, daría error de "Values differ" 
            // porque compararía las referencias en la memoria RAM en lugar del texto real.
            Assert.Equal(expectedDetail, actualDetail);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_NotFound_test()
        {
            // --- ARRANGE ---
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            // --- ACT ---
            // Le pedimos un ID exagerado que sabemos que no hemos insertado en el constructor.
            var result = await controller.GetDetailsForResena(999);

            // --- ASSERT ---
            // Verificamos que no petó, sino que nos devolvió el error HTTP 404 (NotFound) de forma limpia.
            Assert.IsType<NotFoundResult>(result);
        }
    }
}