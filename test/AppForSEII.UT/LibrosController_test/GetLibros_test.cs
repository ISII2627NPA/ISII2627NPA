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

namespace AppForSEII.UT.LibrosController_test
{
    public class GetLibros_test : AppForSEII4SqliteUT 
    {
        public GetLibros_test()
        {
           // 1. Creamos datos de prueba (Géneros y Editoriales) que necesitamos para los libros.
            var generos = new List<Genero>() {
                new Genero { Nombre = "Ficción" },
                new Genero { Nombre = "Fantasía" },
                new Genero { Nombre = "Drama" }
            };
            // 2. Creamos los libros de prueba. ¡Ojo a los detalles clave para los tests!
            var editoriales = new List<Editorial>() {
                new Editorial { Nombre = "Planeta" },
                new Editorial { Nombre = "Anaya" }
            };

            var libros = new List<Libro>(){
                // Libro normal disponible
                new Libro { Titulo = "El Imperio Final", Genero = generos[1], Editorial = editoriales[0], Autor = "Brandon Sanderson", FechaLanzamiento = new DateTime(2006, 7, 17), PrecioCompra = 20.0m, Stock = 10 },
                // Libro normal disponible, más barato
                new Libro { Titulo = "1984", Genero = generos[0], Editorial = editoriales[1], Autor = "George Orwell", FechaLanzamiento = new DateTime(1949, 6, 8), PrecioCompra = 15.0m, Stock = 5 },
                // Libro SIN STOCK (0 unidades). Esto es crucial: lo metemos para comprobar 
                // más tarde que nuestro controlador filtra bien y NO lo devuelve
                new Libro { Titulo = "Hamlet", Genero = generos[2], Editorial = editoriales[0], Autor = "William Shakespeare", FechaLanzamiento = new DateTime(1609, 1, 1), PrecioCompra = 10.0m, Stock = 0 }
            };

            // 3. Guardamos todo en la base de datos de prueba en memoria (_context)
            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            //ejecuta realmente la escritura definitiva de los datos en la base de datos de prueba.
            _context.SaveChanges();
        }

        // Este método devuelve una colección de arrays de objetos (IEnumerable<object[]>).
        // Cada array (object[]) representa UNA ejecución del test, y los elementos del array 
        // son los parámetros que recibirá el test.
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
           // 1. Preparamos los DTOs exactos que esperamos que nos devuelva el controlador
            var libroDTOs = new List<LibroParaComprarDTO>() {
                new LibroParaComprarDTO(1, "El Imperio Final", "Fantasía", "Planeta", "Brandon Sanderson", new DateTime(2006, 7, 17), 20.0, 10),
                new LibroParaComprarDTO(2, "1984", "Ficción", "Anaya", "George Orwell", new DateTime(1949, 6, 8), 15.0, 5)
            };

            // 2. Definimos qué esperamos recibir en cada escenario (Caso):

            // Caso 1: Si no enviamos filtros. Debe devolver los que tienen stock (>0), 
            // ordenados alfabéticamente por Título ("1984" va antes que "El Imperio Final").
            var expectedTC1 = new List<LibroParaComprarDTO>() { libroDTOs[1], libroDTOs[0] };
            
            // Caso 2: Si filtramos por Género "Fantasía". Solo debe devolver "El Imperio Final".
            var expectedTC2 = new List<LibroParaComprarDTO>() { libroDTOs[0] };
            
            // 3. Empaquetamos los casos. Cada 'new object[]' es una llamada al test.
            var expectedTC3 = new List<LibroParaComprarDTO>() { libroDTOs[1] };

            return new List<object[]>
            {
              // Orden de los parámetros: { string? genero, double? precioMaximo, IList<LibroParaComprarDTO> expectedLibros } 
                new object[] { null, null, expectedTC1 },// Se ejecuta el test con el Caso 1
                new object[] { "Fantasía", null, expectedTC2 },// Se ejecuta el test con el Caso 2
                new object[] { null, 16.0, expectedTC3 }// Se ejecuta el test con el Caso 3
            };
        }

        // [Theory] indica que es un test que recibe parámetros (varios casos).
        [Theory]
        // [MemberData] le dice de qué método tiene que sacar esos parámetros (el de la Parte 2).
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_OK_test(string? genero, double? precioMaximo, IList<LibroParaComprarDTO> expectedLibros)
        {
            
            // 1. ARRANGE (Preparar): Configuramos lo que necesitamos para llamar al controlador.
            // Creamos un "Mock" (un doble/simulador) del Logger porque el controlador lo pide en su constructor, 
            // pero en el test no nos importa que guarde logs reales.
            var mock = new Mock<ILogger<LibroController>>();
            // Instanciamos el controlador pasándole la BD falsa (_context) y el Logger falso (mock.Object)
            var controller = new LibroController(_context, mock.Object);

           // 2. ACT (Actuar): Ejecutamos la acción que queremos probar.
            // Llamamos al método pasándole los parámetros que han entrado al test en esta vuelta.
            var result = await controller.GetLibrosParaComprar(genero, precioMaximo);

            // 3. ASSERT (Comprobar): Verificamos que el resultado es exactamente el que esperábamos.
            // Comprobamos que el controlador devolvió un HTTP 200 OK (OkObjectResult)
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Extraemos el "Value" (el cuerpo de la respuesta) y comprobamos que es una Lista de LibroParaComprarDTO
            var librosActual = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);

            // Comparamos la lista que nos ha devuelto el controlador (librosActual) 
            // con la lista que nosotros preparamos a mano en la Parte 2 (expectedLibros)
            Assert.Equal(expectedLibros, librosActual);
        }

        // [Fact] indica que es un test simple, sin parámetros externos. Se ejecuta solo una vez.
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_badrequest_test()
        {
            // 1. ARRANGE: Preparamos el controlador igual que antes.
            var mock = new Mock<ILogger<LibroController>>();
            var controller = new LibroController(_context, mock.Object);

            // 2. ACT: Forzamos el error a propósito. 
            // Enviamos un precio máximo negativo (-5.0), lo cual debería estar prohibido.
            var result = await controller.GetLibrosParaComprar(null, -5.0);

            // 3. ASSERT: Comprobamos cómo reacciona el controlador ante el ataque.
            
            // Verificamos que devuelve un HTTP 400 Bad Request
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Cuando ASP.NET Core devuelve un BadRequest por validaciones (ModelState), 
            // lo envuelve en un objeto llamado ValidationProblemDetails. Verificamos que sea de ese tipo.
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Navegamos por el diccionario de errores. 
            // .Errors.First() coge el primer campo que falló.
            // .Value[0] coge el primer mensaje de texto de ese fallo.
            var problem = problemDetails.Errors.First().Value[0];

            // Finalmente, comprobamos que el mensaje de error es exactamente el que programamos en el controlador.
            Assert.Equal("El precio máximo no puede ser negativo", problem);
        }
    }
}