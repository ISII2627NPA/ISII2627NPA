using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII.UT.ComprasController_test
{
    public class GetCompras_test : AppForSEII4SqliteUT
    {
        public GetCompras_test()
        {
            var generos = new List<Genero>() {
                new Genero { Nombre = "Sci - Fi" },
                new Genero { Nombre = "Drama" }
            };

            var editoriales = new List<Editorial>() {
                new Editorial { Nombre = "Planeta" }
            };

            var libros = new List<Libro>(){
                new Libro { Titulo = "The lord of the rings", Genero = generos[0], Editorial = editoriales[0], Autor = "Tolkien", FechaLanzamiento = new DateTime(2011, 10, 20), PrecioCompra = 10.0m, Stock = 5 },
                new Libro { Titulo = "The man in the high castle", Genero = generos[1], Editorial = editoriales[0], Autor = "Philip K. Dick", FechaLanzamiento = new DateTime(2015, 01, 01), PrecioCompra = 10.0m, Stock = 0 }
            };

            // Creamos un método de pago necesario para el Include del controlador
            var metodoPago = new PayPal("6785849");
            metodoPago.Id = 1;

            // Creamos el usuario con datos ficticios estándar
            ApplicationUser user = new ApplicationUser("1", "Peter", "Jackson", "peter.jackson@uclm.es")
            {
                Nombre = "Peter",
                Apellidos = "Jackson",
                Direccion = "Avda. España s/n, Albacete 02071",
                Telefono = "123456789",
                UserName = "peter.jackson@uclm.es"
            };

            var fechaCompra = new DateTime(2026, 10, 29);

            // Constructor de Compra: (applicationUserId, metodoPagoId, fechaCompra, precioTotal, codigoDescuento)
            var compra = new Compra(user.Id, 1, fechaCompra, 10.0m, null)
            {
                ApplicationUser = user,
                MetodoPago = metodoPago
            };

            // Añadimos el ítem de la compra
            compra.CompraItems.Add(new CompraItem { 
                Libro = libros[0], 
                Cantidad = 1, 
                Compra = compra, 
                
            });

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.Add(metodoPago);
            _context.Add(compra);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetCompra_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForCompras(0);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetCompra_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);
            
            var fechaCompra = new DateTime(2026, 10, 29); 

            // Constructor de CompraDetailDTO con los datos ficticios
            var expectedCompra = new CompraDetailDTO(
                1, 
                fechaCompra, 
                10.0m, 
                "peter.jackson@uclm.es", 
                "Peter", 
                "Jackson",
                "Avda. España s/n, Albacete 02071", 
                "123456789", 
                1, 
                null, 
                new List<CompraItemDTO>()
            );

            // Constructor de CompraItemDTO
            expectedCompra.CompraItems.Add(new CompraItemDTO(1, "The lord of the rings", "Sci - Fi", "Planeta", 1, 10.0m));

            // Act 
            var result = await controller.GetDetailsForCompras(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var compraDTOActual = Assert.IsType<CompraDetailDTO>(okResult.Value);
            
            Assert.Equal(expectedCompra, compraDTOActual);
        }
    }
}