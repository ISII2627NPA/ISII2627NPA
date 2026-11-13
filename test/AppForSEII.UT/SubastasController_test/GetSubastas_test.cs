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
    // Esta clase hereda de 'AppForSEII4SqliteUT', lo que proporciona un contexto de base de datos 
    // SQLite en memoria y aislado para que cada test se ejecute en un entorno limpio.
    public class GetSubastas_test : AppForSEII4SqliteUT
    {
        // 1. Constructor (Arrange global):
        // Este código se ejecuta automáticamente antes de que arranque cualquier test de esta clase.
        // Su objetivo es "sembrar" (poblar) la base de datos con una subasta de prueba completamente construida,
        // incluyendo todas sus relaciones (usuario, pago, libro, etc.) para luego intentar recuperarla.
        public GetSubastas_test()
        {
            // 1.1. Creamos el usuario base.
            ApplicationUser usuario = new ApplicationUser("Peter", "Jackson", "Direccion prueba", "666555444");
            usuario.UserName = "peter.jackson@uclm.es";
            usuario.Email = "peter.jackson@uclm.es";
            // 1.2. Creamos el método de pago.
            var paypal = new PayPal("600000000");
            // 1.3. Creamos la taxonomía del libro (editorial y género).
            var editorial = new Editorial { Nombre = "Minotauro" };
            var genero = new Genero { Nombre = "Sci-Fi" };
            // 1.4. Creamos el libro y le asignamos su editorial y género.
            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 08, 01), 19.99m, 50, 1, 1, "Bolsillo", 4.5m);
            libro.Editorial = editorial;
            libro.Genero = genero;
            // 1.5. Creamos la cabecera de la Subasta.
            var subasta = new Subasta
            {
                Usuario = usuario,
                FechaSubasta = DateTime.Today,
                MetodoPago = paypal,
                PrecioSubasta = 25.50m,
                SubastaItems = new List<SubastaItem>()   // Inicializamos la lista de libros pujados.
            };
            // 1.6. Creamos la línea de detalle (Item) que vincula la subasta con el libro y fija el precio pujado.
            var itemSubasta = new SubastaItem
            {
                Libro = libro,
                Subasta = subasta,
                PrecioPuja = 25.50m,
                Descripcion = "Puja inicial por Dune"
            };
            // Añadimos el item a la lista de la subasta.
            subasta.SubastaItems.Add(itemSubasta);
            // 1.7. Guardamos TODA esta estructura jerárquica en la base de datos en memoria.
            _context.Add(usuario);
            _context.Add(paypal);
            _context.Add(editorial);
            _context.Add(genero);
            _context.Add(libro);
            _context.Add(subasta);
            _context.SaveChanges();
        }

        // 2. Test del Flujo Básico (Éxito al encontrar la subasta):
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_Found_test()
        {
            // ARRANGE (Organizar): Preparamos el controlador.
            // Inyectamos un Logger falso porque solo queremos testear la lógica de datos del controlador.
            var simuladorMock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = simuladorMock.Object;
            var controlador = new SubastasController(_context, logger);

            // Construimos a mano el DTO (Data Transfer Object) con los datos EXACTOS 
            // que sabemos que están en la base de datos de prueba y que el controlador debería devolvernos.
            var itemSubastaEsperado = new SubastaItemDTO(1, "Dune", "Frank Herbert", "Minotauro", 25.50m, "Puja inicial por Dune");
            
            var detalleSubastaEsperado = new SubastaDetailDTO(    //Mirar
                id: 1,   // Sabemos que es el ID 1 porque es el primer y único elemento insertado.
                usuarioUserName: "peter.jackson@uclm.es",
                nombre: "Peter",
                apellidos: "Jackson",
                telefono: "666555444",
                fechaSubasta: DateTime.Today,
                precioSubasta: 25.50m,
                metodoPago: 1,
                subastaItems: new List<SubastaItemDTO> { itemSubastaEsperado }   // Metemos el item en la lista.
            );

            // ACT (Actuar): Simulamos la petición del cliente llamando al endpoint pasando el ID = 1.
            var resultado = await controlador.GetDetailsForSubastas(1);

            // ASSERT (Afirmar): Evaluamos la respuesta.
            // 2.1. Comprobamos que el controlador devolvió un código HTTP 200 (OK).
            var resultadoOk = Assert.IsType<OkObjectResult>(resultado);
            
            // 2.2. Extraemos el objeto que venía dentro de ese HTTP 200 y comprobamos que es un SubastaDetailDTO.
            var detalleSubastaObtenido = Assert.IsType<SubastaDetailDTO>(resultadoOk.Value);
            
            // 2.3. Comparamos el DTO reconstruido por el controlador contra nuestro DTO esperado.
            // Usa el método 'Equals' sobreescrito para verificar cada uno de los campos profundamente
            Assert.Equal(detalleSubastaEsperado, detalleSubastaObtenido);
        }

        // 3. Test del Flujo Alternativo (Fallo por ID inexistente):
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDetailsForSubastas_NotFound_test()
        {
            // ARRANGE (Organizar): Preparamos el controlador de la misma forma.
            var simuladorMock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = simuladorMock.Object;
            var controlador = new SubastasController(_context, logger);

            // ACT (Actuar): Intentamos buscar un ID exageradamente alto (999) 
            // que estamos 100% seguros de que no existe en nuestra pequeña BD de prueba.
            var resultado = await controlador.GetDetailsForSubastas(999);

            // ASSERT (Afirmar): 
            // Verificamos que el controlador atrapó correctamente el fallo (subasta == null) 
            // y devolvió una respuesta HTTP 404 (Not Found) estándar, tal y como dictan las buenas prácticas REST.
            Assert.IsType<NotFoundResult>(resultado);
        }
    }
}