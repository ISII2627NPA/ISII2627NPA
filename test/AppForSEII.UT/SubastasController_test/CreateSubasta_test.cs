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
    // Esta clase hereda de 'AppForSEII4SqliteUT', lo que significa que arranca una base de datos 
    // SQLite en memoria, totalmente limpia y aislada, cada vez que se ejecutan los tests.
    public class CreateSubastas_test : AppForSEII4SqliteUT
    {
        // 1. Constantes: 
        // Centralizamos los datos que vamos a usar repetidamente. Así, si mañana decides cambiar 
        // el nombre de "Peter" a "Juan", solo lo cambias aquí y se actualiza en todos los tests.
        private const string _userName = "peter.jackson@uclm.es";
        private const string _nombre = "Peter";
        private const string _apellidos = "Jackson";
        private const string _telefono = "666555444";

        private const string _libroTitle = "Dune";
        private const string _libroAutor = "Frank Herbert";
        private const string _editorial = "Minotauro";

        // 2. Constructor de la clase de Test (Fase Arrange Global):
        // Todo lo que pongas aquí se ejecutará ANTES de cada test de forma automática.
        // Sirve para "sembrar" (seed) la base de datos con los datos mínimos que necesitan los tests para funcionar.
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

            // PASO CLAVE (Guardado Escalonado): 
            // Primero guardamos las entidades independientes (Usuario, Género, Editorial, Pago).
            // Si no hacemos esto primero, al crear el Libro dará un error de Clave Foránea (Foreign Key),
            // porque el libro intentaría apuntar a una editorial que aún no existe en la base de datos.
            _context.Add(user);
            _context.Add(genero);
            _context.Add(editorial);
            _context.Add(paypal);
            _context.SaveChanges();

            // Ahora creamos el libro usando los IDs reales (editorial.Id, genero.Id) generados en el paso anterior.
            var libro = new Libro(_libroTitle, _libroAutor, new DateTime(1965, 08, 01), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 4.5m)
            {
                Id = 1,
                Editorial = editorial,
                Genero = genero
            };

            // Guardamos el libro finalmente. La base de datos ya está lista para las pruebas.
            _context.Add(libro);
            _context.SaveChanges();
        }

        // 3. Proveedor de Datos para los Tests de Error:
        // Este método estático prepara todos los escenarios de "Flujos Alternativos" (donde el usuario se equivoca).
        // Devuelve una lista de arrays de objetos. Cada array contiene dos cosas: 
        // [0] El DTO con datos incorrectos.
        // [1] El mensaje de error exacto que esperamos que devuelva el controlador.
        public static IEnumerable<object[]> TestCasesFor_CreateSubasta()
        {
            var itemValido = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, _libroTitle, _libroAutor, _editorial, 25.50m, "Descripción válida")
            };
            // Preparación de los 5 escenarios de error:
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
            // Empaquetamos todo vinculando el DTO erróneo con su mensaje correspondiente.
            var todosLosCasos = new List<object[]>
        {
            
            new object[] { subastaSinItems, "Error! Debe incluir al menos un libro en la lista de pujas." },
            new object[] { subastaPrecioNegativo, "Error! El precio de puja para el libro 'Dune' no puede ser menor que 0." },
            new object[] { subastaUsuarioFalso, "Error! El usuario no está registrado." },
            new object[] { subastaPagoFalso, "¡Error! El método de pago seleccionado no existe." },
            new object[] { subastaLibroFalso, "Error! El libro 'LibroFalso' no está disponible para subastar." }
        };

            return todosLosCasos;
        }
        // 4. Test de Errores (Flujos Alternativos):
        // La etiqueta [Theory] indica que este test se va a ejecutar varias veces (una por cada escenario del método anterior).
        // Recibe por parámetro el DTO erróneo y el string del error esperado.
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]   // Indica que usa su propia BD en memoria, no una compartida.
        [MemberData(nameof(TestCasesFor_CreateSubasta))]    // Le dice de dónde sacar los datos.
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaDTO, string errorExpected)
        {
            // ARRANGE: Preparamos el entorno. 
            // Usamos 'Mock' para crear un Logger falso, ya que no nos interesa probar el sistema de logs, solo el controlador.
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // ACT: Ejecutamos el método POST del controlador pasándole los datos malos.
            var result = await controller.CreateSubasta(subastaDTO);

            // ASSERT: Comprobamos que el resultado es exactamente el que queríamos.
            // 1. Verificamos que devolvió un Error 400 (BadRequest).
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // 2. Extraemos los detalles del problema de validación del cuerpo de la respuesta.
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            // 3. Cogemos el primer mensaje de error que ha escupido el ModelState.
            var errorActual = problemDetails.Errors.First().Value[0];
            // 4. Comprobamos que el error real del controlador coincide con el que nosotros esperábamos.
            Assert.StartsWith(errorExpected, errorActual);
        }

        // 5. Test de Éxito (Flujo Básico):
        // La etiqueta [Fact] indica que es un test único y directo, sin parámetros externos.
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateSubasta_Success_test()
        {
            // ARRANGE: Instanciamos el controlador con el Logger falso.
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Preparamos unos datos de entrada TOTALMENTE VÁLIDOS.
            var itemsDTO = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, _libroTitle, _libroAutor, _editorial, 25.50m, "Esta es una descripcion lo suficientemente larga")
            };
            var subastaCrearDTO = new SubastaForCreateDTO(_userName, _nombre, _apellidos, _telefono, 1, itemsDTO);

            // Construimos a mano la respuesta exacta que ESPERAMOS que nos devuelva el controlador tras guardarlo todo.
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

            // ACT: Llamamos al controlador simulando que el usuario acaba de pulsar "Publicar subasta".
            var result = await controller.CreateSubasta(subastaCrearDTO);

            // ASSERT: Verificamos el resultado.
            // 1. Comprobamos que devuelve un código HTTP 201 (CreatedAtAction).
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            // 2. Extraemos el objeto que nos devuelve en el cuerpo de la respuesta.
            var subastaActual = Assert.IsType<SubastaDetailDTO>(createdResult.Value);
            // 3. Magia: Comparamos el objeto que esperábamos con el que realmente devolvió.
            // Como sobrescribiste el método 'Equals' en SubastaDetailDTO, Assert.Equal no compara direcciones de memoria, 
            // sino que compara propiedad por propiedad (precios, listas internas, nombres...) garantizando que el guardado fue perfecto.
            Assert.Equal(subastaEsperadaDTO, subastaActual);
        }
    }
}