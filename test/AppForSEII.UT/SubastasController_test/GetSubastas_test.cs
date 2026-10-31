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

namespace AppForSEII.UT.SubastasController_test
{
    public class GetSubastas_test : AppForSEII4SqliteUT
    {
        public GetSubastas_test()
        {
            // 1. Organizar (Arrange) general: Inicializamos datos en la BD en memoria
            ApplicationUser usuario = new ApplicationUser("Peter", "Jackson", "Direccion prueba", "666555444");
            usuario.UserName = "peter.jackson@uclm.es";
            usuario.Email = "peter.jackson@uclm.es";

            var paypal = new PayPal("600000000");

            var editorial = new Editorial { Nombre = "Minotauro" };
            var genero = new Genero { Nombre = "Sci-Fi" };
            
            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 08, 01), 19.99m, 50, 1, 1, "Bolsillo", 4.5m);
            libro.Editorial = editorial;
            libro.Genero = genero;

            var subasta = new Subasta
            {
                Usuario = usuario,
                FechaSubasta = DateTime.Today,
                MetodoPago = paypal,
                PrecioSubasta = 25.50m,
                SubastaItems = new List<SubastaItem>()
            };

            var itemSubasta = new SubastaItem
            {
                Libro = libro,
                Subasta = subasta,
                PrecioPuja = 25.50m,
                Descripcion = "Puja inicial por Dune"
            };
            subasta.SubastaItems.Add(itemSubasta);

            _context.Add(usuario);
            _context.Add(paypal);
            _context.Add(editorial);
            _context.Add(genero);
            _context.Add(libro);
            _context.Add(subasta);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_Found_test()
        {
            // Organizar (Arrange)
            var simuladorMock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = simuladorMock.Object;
            var controlador = new SubastasController(_context, logger);

            // Construimos el DTO que esperamos recibir exactamente de la base de datos
            var itemSubastaEsperado = new SubastaItemDTO(1, "Dune", "Frank Herbert", "Minotauro", 25.50m, "Puja inicial por Dune");
            
            var detalleSubastaEsperado = new SubastaDetailDTO(
                id: 1,
                usuarioUserName: "peter.jackson@uclm.es",
                nombre: "Peter",
                apellidos: "Jackson",
                telefono: "666555444",
                fechaSubasta: DateTime.Today,
                precioSubasta: 25.50m,
                metodoPago: 1,
                subastaItems: new List<SubastaItemDTO> { itemSubastaEsperado }
            );

            // Actuar (Act)
            var resultado = await controlador.GetDetailsForSubastas(1);

            // Afirmar (Assert)
            // Verificamos que el tipo de respuesta es OK (HTTP 200)
            var resultadoOk = Assert.IsType<OkObjectResult>(resultado);
            
            // Verificamos que el contenido es un SubastaDetailDTO
            var detalleSubastaObtenido = Assert.IsType<SubastaDetailDTO>(resultadoOk.Value);
            
            // Verificamos que los datos esperados y actuales son iguales
            Assert.Equal(detalleSubastaEsperado, detalleSubastaObtenido);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_NotFound_test()
        {
            // Organizar (Arrange)
            var simuladorMock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = simuladorMock.Object;
            var controlador = new SubastasController(_context, logger);

            // Actuar (Act): Buscamos un ID que sabemos que no existe en nuestra BD en memoria
            var resultado = await controlador.GetDetailsForSubastas(999);

            // Afirmar (Assert)
            // Verificamos que la respuesta es un NotFound (HTTP 404)
            Assert.IsType<NotFoundResult>(resultado);
        }
    }
}