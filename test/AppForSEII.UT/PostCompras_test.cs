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
        // 1. CONSTANTES: Guardamos los textos repetitivos en variables inmutables
        // para no equivocarnos al escribirlos 20 veces a lo largo del test.
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
            // 2. CONSTRUCTOR: Creamos la base de datos de prueba
            var generos = new List<Genero>() {
                new Genero(_libro1Genero),
                new Genero(_libro2Genero)
            };

            var editoriales = new List<Editorial>() {
                new Editorial(_editorial)
            };

            // Metemos los libros. ¡Ojo al stock! El primer libro tiene 5, el segundo tiene 0.
            var libros = new List<Libro>(){
                new Libro { Titulo = _libro1Title, Genero = generos[0], Editorial = editoriales[0], Autor = "Tolkien", FechaLanzamiento = new DateTime(2011, 10, 20), PrecioCompra = 10.0m, Stock = 5 },
                new Libro { Titulo = _libro2Title, Genero = generos[1], Editorial = editoriales[0], Autor = "Philip K. Dick", FechaLanzamiento = new DateTime(2015, 01, 01), PrecioCompra = 10.0m, Stock = 0 }
            };
            // Creamos un método de pago válido
            var metodoPago = new PayPal("6785849");
            metodoPago.Id = 1;

            // Registramos a Peter Jackson en el sistema usando las constantes de arriba
            ApplicationUser user = new ApplicationUser("1", _nombre, _apellidos, _userName)
            {
                Nombre = _nombre,
                Apellidos = _apellidos,
                Direccion = _direccion,
                Telefono = _telefono,
                UserName = _userName
            };

            // Guardamos todo en la base de datos SQLite en memoria
            _context.ApplicationUsers.Add(user);
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.Add(metodoPago);
            _context.SaveChanges();
        }

        // Método que devuelve una colección de paquetes con DTOs defectuosos y el mensaje de error esperado
        public static IEnumerable<object[]> TestCasesFor_CreateCompra()
        {
            // --- PREPARACIÓN DE CASOS DE ERROR ---

            // 1. Error: Carrito vacío (Lista de CompraItems vacía)
            var compraNoItem = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, new List<CompraItemDTO>());

            // Creamos una lista de items correcta para reutilizarla en los errores que no tienen que ver con los items.
            // Le ponemos Cantidad = 2 porque sabemos que tu controlador rechaza compras con Cantidad <= 1.
            var compraItemsValidos = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) };

            // 2. Error: Usuario fantasma (Email que no está en la base de datos)
            var compraUsuarioFalso = new CompraForCreateDTO("falso@uclm.es", _nombre, _apellidos, _direccion, _telefono, 1, null, compraItemsValidos);

            /// 3. Error: Cantidad no valida (Pedimos solo 1 unidad, y tu controlador exige > 1)
            var itemsCantidadInvalida = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 1, 10.0m) };
            var compraCantidadInvalida = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, itemsCantidadInvalida);

            // 4. Errores de longitud en el código de descuento
            // 4a: Demasiado corto ("123" tiene 3 letras, mínimo 5)
            var compraDescuentoInvalido = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, "123", compraItemsValidos);
           // 4b: Demasiado largo (12 letras, máximo 10)
            var compraDescuentoLargo = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, "DESCUENTAZO1", compraItemsValidos);
            
            // 5. Error: Método de pago inexistente (el 99 no se creó en el constructor)
            var compraMetodoPagoInvalido = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 99, null, compraItemsValidos);

            // 6. Error: Stock insuficiente (Intentamos comprar 10, pero solo metimos 5 en el constructor)
            var itemsSinStock = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 10, 10.0m) };
            var compraSinStock = new CompraForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, 1, null, itemsSinStock);
            var compraDireccion= new CompraForCreateDTO(_userName, _nombre, _apellidos, "Avenida de España", _telefono, 1, null, compraItemsValidos);
            

            // --- EMPAQUETADO ---
            // Unimos el DTO tramposo con la frase exacta que debe devolver tu controlador
            var allTests = new List<object[]>
            {
                new object[] { compraNoItem, "Error! You must include at least one book to be purchased" },
                new object[] { compraUsuarioFalso, "Error! UserName is not registered" },
                new object[] { compraCantidadInvalida, $"Error! La cantidad para el libro '{_libro1Title}' debe ser mayor que 1" },
                new object[] { compraDescuentoInvalido, "Error! El código de descuento debe tener entre 5 y 10 caracteres." },
                new object[] { compraDescuentoLargo, "Error! El código de descuento debe tener entre 5 y 10 caracteres." }, 
                new object[] { compraMetodoPagoInvalido, "Error! El método de pago especificado no existe." },
                new object[] { compraSinStock, $"Error! El libro '{_libro1Title}' no está disponible en la cantidad solicitada" },
                new object[]{compraDireccion,"Error! La dirección de envío debe contener la palabra Calle."},
                
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateCompra))]

        // El test recibe en cada vuelta un DTO defectuoso y su mensaje de error esperado
        public async Task CreateCompra_Error_test(CompraForCreateDTO compraDTO, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // --- ACT ---
            // Intentamos guardar la compra mala
            var result = await controller.CreateCompra(compraDTO);

            // --- ASSERT ---
            // 1. Confirmamos que la API nos rechaza con un HTTP 400 Bad Request
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // 2. Extraemos el objeto estándar de validaciones de errores de .NET
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // 3. Navegamos por el diccionario de errores para sacar el mensaje de texto
            var errorActual = problemDetails.Errors.First().Value[0];

            // 4. Comprobamos que el texto del error es exactamente el que esperábamos
            // Usamos StartsWith por si el framework añade algún punto o salto de línea al final
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

            // Creamos un carrito perfectamente válido (Cantidad = 2, libro con stock de 5)
            var compraItems = new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) };
            
            // Construimos el DTO de entrada simulando los datos que enviaría el Frontend
            var compraDTO = new CompraForCreateDTO(_userName, _nombre, _apellidos, "Calle Calvario", _telefono, 1, null, compraItems);

            // Construimos el DTO DE SALIDA que esperamos que nos devuelva el controlador una vez creada.
            // OJO AL CÁLCULO: Si el libro vale 10.0 y compramos 2, el controlador debe calcular un Total de 20.0
            var expectedCompraDetailDTO = new CompraDetailDTO(
                1,  // Será la compra con ID 1 porque es la primera que se guarda
                DateTime.Now, 
                20.0m,  // El precio total esperado
                _userName, 
                _nombre, 
                _apellidos, 
                "Calle Calvario", 
                _telefono, 
                1, 
                null, 
                new List<CompraItemDTO>() { new CompraItemDTO(1, _libro1Title, _libro1Genero, _editorial, 2, 10.0m) }
            );

           // --- ACT ---
            // Le pedimos al controlador que guarde la compra
            var result = await controller.CreateCompra(compraDTO);

            // --- ASSERT ---
            // 1. Cuando se CREA algo con éxito en una API, no devuelve un OK (200), 
            // sino un CREATED (201). En .NET esto es un CreatedAtActionResult.
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            
            // 2. Extraemos la compra recién creada y transformada a DTO
            var actualCompraDetailDTO = Assert.IsType<CompraDetailDTO>(createdResult.Value);


            Assert.Equal(expectedCompraDetailDTO, actualCompraDetailDTO);
        }
    }
}