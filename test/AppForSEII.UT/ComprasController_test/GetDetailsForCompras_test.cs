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
            // 1. Creamos las entidades básicas con sus Ids
            var generoSciFi = new Genero { Id = 1, Nombre = "Sci - Fi" };
            var generoDrama = new Genero { Id = 2, Nombre = "Drama" };
            var editorialPlaneta = new Editorial { Id = 1, Nombre = "Planeta" };

            // 2. Las guardamos PRIMERO en la BD para asegurar que los IDs están registrados
            _context.Generos.AddRange(generoSciFi, generoDrama);
            _context.Editoriales.Add(editorialPlaneta);
            _context.SaveChanges();

            // 3. Cambiamos a los constructores estrictos con paréntesis como pide la profesora.
            // Pasamos los .Id correspondientes.
            var libro1 = new Libro("The lord of the rings", "Tolkien", new DateTime(2011, 10, 20), 10.0m, 5, editorialPlaneta.Id, generoSciFi.Id, "Bolsillo", 4.5m) { Id = 1 };
            
            var libro2 = new Libro("The man in the high castle", "Philip K. Dick", new DateTime(2015, 01, 01), 10.0m, 0, editorialPlaneta.Id, generoDrama.Id, "Bolsillo", 4.5m) { Id = 2 };

            // 2. Creamos las dependencias obligatorias de una Compra: un Método de Pago y un Usuario
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

            // 3. Construimos la Compra principal enlazándola con el usuario y el método de pago
            // Constructor de Compra: (applicationUserId, metodoPagoId, fechaCompra, precioTotal, codigoDescuento)
            var compra = new Compra(user.Id, 1, fechaCompra, 10.0m, null)
            {
                ApplicationUser = user,
                MetodoPago = metodoPago
            };

           // 4. Añadimos el "ticket" a la compra (1 unidad de El Señor de los Anillos)
            compra.CompraItems.Add(new CompraItem { 
                Libro = libro1, 
                Cantidad = 1, 
                Compra = compra, 
                
            });

            // 5. Guardamos TODO en la base de datos de prueba. 
            // Si falta el SaveChanges(), la BD estará vacía y los tests fallarán.
            _context.ApplicationUsers.Add(user);
            _context.AddRange(libro1,libro2);
            _context.Add(metodoPago);
            _context.Add(compra);
            _context.SaveChanges();
        }

        // [Fact] porque solo probamos un escenario concreto sin parámetros de entrada
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetCompra_NotFound_test()
        {
            /// --- ARRANGE ---
            // Creamos el doble (Mock) del sistema de Logs para engañar al controlador
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            // Instanciamos el controlador con la BD llena del paso anterior
            var controller = new ComprasController(_context, logger);

            // --- ACT ---
            // Le pedimos al controlador que busque los detalles de la compra con ID = 0.
            // (Como las IDs en bases de datos empiezan en 1, sabemos seguro que la 0 no existe).
            var result = await controller.GetDetailsForCompras(0);

            // --- ASSERT ---
            // Comprobamos que el controlador ha respondido devolviendo un objeto de tipo NotFoundResult.
            // Esto equivale a que la API devuelve un código HTTP 404 (Not Found) al cliente.
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetCompra_Found_test()
        {
            // --- ARRANGE ---
            // Esta parte igual que antes
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;
            var controller = new ComprasController(_context, logger);

            // Fecha de la compra( la misma que arriba)
            var fechaCompra = new DateTime(2026, 10, 29); 

            // Construimos nuestra "Bola de Cristal": el DTO exacto que esperamos recibir.
            // Copiamos a mano los datos de Peter Jackson y del Señor de los Anillos 
            // que metimos en el constructor.
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
            // Añadimos el ítem esperado (1 unidad a 10.0€)
            expectedCompra.CompraItems.Add(new CompraItemDTO(1, "The lord of the rings", "Sci - Fi", "Planeta", 1, 10.0m));

            // --- ACT ---
            // Llamamos a la API pidiendo la compra con ID = 1 (la de Peter Jackson)
            var result = await controller.GetDetailsForCompras(1);

            // --- ASSERT ---
            // 1. Comprobamos la caja exterior: ¿Es un HTTP 200 OK?
            var okResult = Assert.IsType<OkObjectResult>(result);
            
            // 2. Desempaquetamos la caja: ¿El contenido es exactamente de tipo CompraDetailDTO?
            var compraDTOActual = Assert.IsType<CompraDetailDTO>(okResult.Value);

            // 3. La prueba definitiva: Comparamos el DTO que montamos a mano (expectedCompra)
            // contra el que acaba de calcular y devolver el controlador (compraDTOActual).
            Assert.Equal(expectedCompra, compraDTOActual);
        }
    }
}