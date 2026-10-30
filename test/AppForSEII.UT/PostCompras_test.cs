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

namespace AppForSEII.UT.ComprasController_test
{
    public class PostCompras_test : AppForSEII4SqliteUT
    {
        private const string _userName = "peter.jackson@uclm.es";
        private const string _nombre = "Peter";
        private const string _apellidos = "Jackson";
        private const string _direccion = "Avda. España s/n, Albacete 02071";
        private const string _telefono = "123456789";

        private const string _libro1Title = "The lord of the rings";
        private const string _libro1Genero = "Sci - Fi";
        private const string _editorial = "Planeta";
        private const string _libro2Title = "The man in the high castle";
        private const string _libro2Genero = "Drama";

        public PostCompras_test()
        {
            var generos = new List<Genero>() {
                new Genero(_libro1Genero),
                new Genero(_libro2Genero)
            };

            var editoriales = new List<Editorial>() {
                new Editorial(_editorial)
            };

            var libros = new List<Libro>(){
                new Libro { Titulo = _libro1Title, Genero = generos[0], Editorial = editoriales[0], Autor = "Tolkien", FechaLanzamiento = new DateTime(2011, 10, 20), PrecioCompra = 10.0m, Stock = 5 },
                new Libro { Titulo = _libro2Title, Genero = generos[1], Editorial = editoriales[0], Autor = "Philip K. Dick", FechaLanzamiento = new DateTime(2015, 01, 01), PrecioCompra = 10.0m, Stock = 0 }
            };

            var metodoPago = new PayPal("6785849");
            metodoPago.Id = 1;

            ApplicationUser user = new ApplicationUser("1", _nombre, _apellidos, _userName)
            {
                Nombre = _nombre,
                Apellidos = _apellidos,
                Direccion = _direccion,
                Telefono = _telefono,
                UserName = _userName
            };

            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.Add(metodoPago);
            _context.SaveChanges();
        }

        // Aquí definimos los casos de prueba de ERROR basándonos en los errores de tu controlador
        public static IEnumerable<object[]> TestCasesFor_CreateCompra()
        {
            // 1. Error: Sin items
            var compraNoItem = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, new List<CompraItemDTO>());

            // Base de items válidos para reutilizar (Ponemos cantidad 2 porque tu controlador da error si es <= 1)
            var compraItemsValidos = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) };

            // 2. Error: Usuario no registrado
            var compraUsuarioFalso = new CompraForCreateDTO("falso@uclm.es", _nombre, _apellidos, _direccion, _telefono, 1, null, compraItemsValidos);

            // 3. Error: Cantidad no válida (<= 1)
            var itemsCantidadInvalida = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 1, 10.0m) };
            var compraCantidadInvalida = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, itemsCantidadInvalida);

            // 4. Error: Código de descuento inválido (menos de 5 caracteres)
            var compraDescuentoInvalido = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, "123", compraItemsValidos);

            // 5. Error: Método de pago no existe
            var compraMetodoPagoInvalido = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 99, null, compraItemsValidos);

            // 6. Error: Stock insuficiente (pedimos 10, pero en el seed data solo hay 5)
            var itemsSinStock = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 10, 10.0m) };
            var compraSinStock = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, itemsSinStock);

            var allTests = new List<object[]>
            {
                new object[] { compraNoItem, "Error! You must include at least one book to be purchased" },
                new object[] { compraUsuarioFalso, "Error! UserName is not registered" },
                new object[] { compraCantidadInvalida, $"Error! La cantidad para el libro '{_libro1Title}' debe ser mayor que 1" },
                new object[] { compraDescuentoInvalido, "Error! El código de descuento debe tener entre 5 y 10 caracteres." },
                new object[] { compraMetodoPagoInvalido, "Error! El método de pago especificado no existe." },
                new object[] { compraSinStock, $"Error! El libro '{_libro1Title}' no está disponible en la cantidad solicitada" }
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateCompra))]
        public async Task CreateCompra_Error_test(CompraForCreateDTO compraDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.CreateCompra(compraDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateCompra_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // Importante: Cantidad = 2 para pasar la validación de tu controlador
            var compraItems = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) };
            
            var compraDTO = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, compraItems);

            // Calculamos que el precio total esperado son 20.0m (2 libros * 10.0m)
            var expectedCompraDetailDTO = new CompraDetailDTO(
                1, 
                DateTime.Now, 
                20.0m, 
                _userName, 
                _nombre, 
                _apellidos, 
                _direccion, 
                _telefono, 
                1, 
                null, 
                new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) }
            );

            // Act
            var result = await controller.CreateCompra(compraDTO);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualCompraDetailDTO = Assert.IsType<CompraDetailDTO>(createdResult.Value);

            Assert.Equal(expectedCompraDetailDTO, actualCompraDetailDTO);
        }
    }
}