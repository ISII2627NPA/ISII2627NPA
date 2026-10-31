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
    public class CreateSubastas_test : AppForSEII4SqliteUT
    {
        // Constantes para reutilizar en los tests y mantener la limpieza
        private const string _userName = "peter.jackson@uclm.es";
        private const string _nombre = "Peter";
        private const string _apellidos = "Jackson";
        private const string _telefono = "666555444";

        private const string _libroTitle = "Dune";
        private const string _libroAutor = "Frank Herbert";
        private const string _editorial = "Minotauro";

        public CreateSubastas_test()
        {
            var genero = new Genero { Nombre = "Ficción" };
            var editorial = new Editorial { Nombre = _editorial };

            // Le forzamos el Id = 1 porque en los casos de prueba usamos MétodoPago = 1
            var paypal = new PayPal("600000000") { Id = 1 };

            ApplicationUser user = new ApplicationUser(_nombre, _apellidos, "Calle Falsa 123", _telefono)
            {
                Id = "1",
                UserName = _userName,
                Email = _userName
            };

            // 1. Guardamos PRIMERO las dependencias para que SQLite les genere sus IDs reales
            _context.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.Add(paypal);
            _context.SaveChanges();

            // 2. AHORA creamos el libro usando los IDs reales de las dependencias que acabamos de guardar
            var libro = new Libro(_libroTitle, _libroAutor, new DateTime(1965, 08, 01), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 4.5m)
            {
                Id = 1,
                Editorial = editorial,
                Genero = genero
            };

            // 3. Guardamos el libro finalmente
            _context.Add(libro);
            _context.SaveChanges();
        }

        // Método que provee los casos de error (Flujos Alternativos)
        // Método que provee los casos de error adaptado EXACTAMENTE a tu controlador
        public static IEnumerable<object[]> TestCasesFor_CreateSubasta()
        {
            var itemValido = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, _libroTitle, _libroAutor, _editorial, 25.50m, "Descripción válida")
            };

            // 1. Error: Lista de pujas vacía
            var subastaSinItems = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 1, new List<SubastaItemDTO>());

            // 2. Error: Precio de puja menor que 0
            var itemsPrecioNegativo = new List<SubastaItemDTO> { new SubastaItemDTO(1, _libroTitle, _libroAutor, _editorial, -5.00m, "Descripción válida") };
            var subastaPrecioNegativo = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 1, itemsPrecioNegativo);

            // 3. Error: Usuario no registrado
            var subastaUsuarioFalso = new SubastaForCreateDTO("falso@uclm.es", _nombre, _apellidos, _telefono, 1, itemValido);

            // 4. Error: Método de pago no existe (Le pasamos un Id de pago que no hemos creado, ej: 99)
            var subastaPagoFalso = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 99, itemValido);

            // 5. Error: Libro no disponible (Le pasamos un título de libro que no existe en la BD de prueba)
            var itemsLibroFalso = new List<SubastaItemDTO> { new SubastaItemDTO(1, "LibroFalso", _libroAutor, _editorial, 25.50m, "Descripción válida") };
            var subastaLibroFalso = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 1, itemsLibroFalso);

            var todosLosCasos = new List<object[]>
        {
            // Textos copiados EXACTAMENTE de tu archivo SubastasController.cs
            new object[] { subastaSinItems, "Error! Debe incluir al menos un libro en la lista de pujas." },
            new object[] { subastaPrecioNegativo, "Error! El precio de puja para el libro 'Dune' no puede ser menor que 0." },
            new object[] { subastaUsuarioFalso, "Error! El usuario no está registrado." },
            new object[] { subastaPagoFalso, "¡Error! El método de pago seleccionado no existe." },
            new object[] { subastaLibroFalso, "Error! El libro 'LibroFalso' no está disponible para subastar." }
        };

            return todosLosCasos;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateSubasta))]
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.CreateSubasta(subastaDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // DTO de envío con datos correctos
            var itemsDTO = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, _libroTitle, _libroAutor, _editorial, 25.50m, "Esta es una descripcion lo suficientemente larga")
            };
            var subastaCrearDTO = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 1, itemsDTO);

            // DTO de respuesta esperado según el Paso 7 del Flujo Básico
            var subastaEsperadaDTO = new SubastaDetailDTO(
                id: 1,
                usuarioUserName: _userName,
                nombre: _nombre,
                apellidos: _apellidos,
                telefono: _telefono,
                fechaSubasta: DateTime.Today, // Se asume que el controlador asigna Today o Now
                precioSubasta: 25.50m, // El controlador debe calcular el total sumando las pujas
                metodoPago: 1,
                subastaItems: itemsDTO
            );

            // Act
            var result = await controller.CreateSubasta(subastaCrearDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var subastaActual = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            Assert.Equal(subastaEsperadaDTO, subastaActual);
        }
    }
}