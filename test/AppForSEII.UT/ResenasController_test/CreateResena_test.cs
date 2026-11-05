using System;
using System.Collections.Generic;
using System.Linq;
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
    // "AppForSEII4SqliteUT" es la clase base que os dio el profesor. 
    // Al heredar de ella, se crea automáticamente una base de datos SQLite "en memoria" (RAM) para cada test, 
    // limpia y aislada de la base de datos real del programa.
    public class CreateResena_test : AppForSEII4SqliteUT
    {
        // CONSTRUCTOR: Se ejecuta antes de CADA test. Sirve para poblar la BD en memoria con los datos mínimos necesarios.
        public CreateResena_test()
        {
            // Creamos un usuario, editorial y género genéricos.
            var user = new ApplicationUser("Peter", "Jackson", "Calle", "600") { Id = "1", UserName = "peter@uclm.es", Email = "peter@uclm.es" };
            var editorial = new Editorial { Nombre = "Minotauro" };
            var genero = new Genero { Nombre = "Ciencia Ficción" };
            
            _context.Users.Add(user);
            _context.Editoriales.Add(editorial);
            _context.Generos.Add(genero);
            _context.SaveChanges();

            // Creamos un libro ("Dune") al que le asignamos una nota inicial de 4.0.
            var libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 8, 1), 19.99m, 50, editorial.Id, genero.Id, "Bolsillo", 4m) { Id = 1 };
            _context.Libros.Add(libro);
            _context.SaveChanges();
            
            // Le añadimos una reseña previa simulando el "historial" del libro. 
            // Esto es CRUCIAL para comprobar luego que nuestro recálculo de la Calificación Media funciona.
            var resenaPrevia = new Resena(user.Id, DateTime.Now, "Previa") { Usuario = user };
            _context.Resenas.Add(resenaPrevia);
            _context.SaveChanges();
            _context.ResenaItems.Add(new ResenaItem("Bien", 4, libro.Id, resenaPrevia.Id));
            _context.SaveChanges();
        }

        // [Fact]: Etiqueta de xUnit para indicar que esto es un test individual sin parámetros externos.
        [Fact]
        [Trait("LevelTesting", "Unit Testing")] // Etiqueta organizativa para agrupar los tests en Visual Studio.
        public async Task CreateResena_Success_test()
        {
            // --- ARRANGE (Preparar) ---
            
            // "Mock" es un doble de acción. Como no nos interesa probar el Logger (solo estorba en la prueba), 
            // creamos uno falso y se lo enchufamos al controlador.
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            // Creamos el DTO de entrada. Simulamos lo que escribiría el usuario en el formulario web.
            // Le va a dar un 5 al libro "Dune".
            var nuevaResena = new ResenaForCreateDTO
            {
                UsuarioUserName = "peter@uclm.es",
                Titulo = "Excelente",
                Nombre = "Peter",
                Apellidos = "Jackson",
                Direccion = "Calle",
                Telefono = "600",
                ResenaItems = new List<ResenaItemDTO> 
                {
                    new ResenaItemDTO(1, "Dune", "Minotauro", "Bolsillo", 5, "Genial")
                }
            };

            // --- ACT (Ejecutar) ---
            var result = await controller.CreateResena(nuevaResena);

            // --- ASSERT (Comprobar) ---
            
            // 1. Verificamos que el controlador devolvió un "201 Created".
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            // 2. Extraemos el objeto que venía dentro del 201 y verificamos que es del tipo DTO correcto.
            var detail = Assert.IsType<ResenaDetailDTO>(createdResult.Value);
            // 3. Comprobamos que el título se guardó bien.
            Assert.Equal("Excelente", detail.Titulo);
            
            // 4. LA PRUEBA REINA: Buscamos el libro en la BD para ver si su media se actualizó.
            var libroEnBd = _context.Libros.First(l => l.Titulo == "Dune");
            // Tenía un 4.0 histórico. Le acabamos de dar un 5.0. La media debe ser exactamente 4.5.
            Assert.Equal(4.5m, libroEnBd.CalificacionMedia); 
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Error_test()
        {
            // --- ARRANGE ---
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            // Simulamos un usuario malicioso o torpe: envía el formulario sin haber valorado ningún libro (lista vacía).
            var nuevaResena = new ResenaForCreateDTO
            {
                UsuarioUserName = "peter@uclm.es",
                Titulo = "Reseña vacía",
                ResenaItems = new List<ResenaItemDTO>() 
            };

            // --- ACT ---
            var result = await controller.CreateResena(nuevaResena);

            // --- ASSERT ---
            // 1. Verificamos que el controlador detuvo la ejecución y lanzó un "400 Bad Request".
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            
            // 2. Extraemos el diccionario de errores (ValidationProblemDetails).
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
            
            // 3. Comprobamos que el diccionario contiene el error específico de "ResenaItems" que programamos.
            Assert.True(problemDetails.Errors.ContainsKey("ResenaItems"));
        }
    }
}