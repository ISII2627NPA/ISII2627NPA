using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// Referencias a tu proyecto principal de la API
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.UT.ResenasController_test
{
    public class CreateResenas_test : AppForSEII4SqliteUT
    {
        // --- CONSTANTES PARA LOS DATOS DE PRUEBA ---
        private const string _userName = "elena.navarro@uclm.es";
        private const string _nombre = "Elena";
        private const string _apellidos = "Navarro";
        private const string _direccion = "Avda. España s/n, Albacete";
        private const string _telefono = "666555444";
        private const string _tituloResena = "Mi opinión sobre estos libros";

        private const string _libro1Title = "Dune";
        private const string _libro1Editorial = "Minotauro";
        private const string _libro1Tipo = "Bolsillo";
        
        // CONSTRUCTOR: Prepara la BD en memoria antes de CADA test
        public CreateResenas_test()
        {
            var user = new ApplicationUser { Id = "1", UserName = _userName, Email = _userName, Nombre = _nombre, Apellidos = _apellidos, Direccion = _direccion, Telefono = _telefono };
            var genero = new Genero { Nombre = "Ciencia Ficción" };
            var editorial = new Editorial { Nombre = _libro1Editorial };
            
            // Creamos el libro con una nota media inicial de 4.0
            var libro = new Libro { Id = 1, Titulo = _libro1Title, Autor = "Frank Herbert", TipoLibro = _libro1Tipo, CalificacionMedia = 4.0m, FechaLanzamiento = new DateTime(1965, 8, 1), Genero = genero, Editorial = editorial };

            // Añadimos la reseña antigua al historial para simular ese 4.0 previo
            var resenaPrevia = new Resena { Id = 1, Usuario = user, FechaResena = DateTime.Now, Titulo = "Reseña antigua" };
            var resenaItemPrevio = new ResenaItem { LibroId = 1, ResenaId = 1, Calificacion = 4, Descripcion = "Muy buen libro" };
            libro.ResenaItems = new List<ResenaItem> { resenaItemPrevio };

            _context.Users.Add(user);
            _context.Generos.Add(genero);
            _context.Editoriales.Add(editorial);
            _context.Libros.Add(libro);
            _context.Resenas.Add(resenaPrevia);
            _context.SaveChanges();
        }

        // --- ORIGEN DE DATOS PARA LOS CASOS DE ERROR ---
        public static IEnumerable<object[]> TestCasesFor_CreateResena()
        {
            var itemsValidos = new List<ResenaItemDTO> { new ResenaItemDTO(1, _libro1Title, _libro1Editorial, _libro1Tipo, 5, "Me ha encantado.") };

            var resenaSinItems = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, new List<ResenaItemDTO>());
            var resenaUsuarioFalso = new ResenaForCreateDTO("falso@uclm.es", _nombre, _apellidos, _direccion, _telefono, _tituloResena, itemsValidos);
            var resenaLibroFalso = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, 
                new List<ResenaItemDTO> { new ResenaItemDTO(99, "LibroFalso", "Ed", "Tipo", 5, "Genial") });

            var allTests = new List<object[]>
            { 
                new object[] { resenaSinItems, "¡Error! Debes seleccionar al menos un libro para hacer la reseña." },
                new object[] { resenaUsuarioFalso, "¡Error! El usuario no está registrado." },
                new object[] { resenaLibroFalso, "¡Error! El libro 'LibroFalso' no existe en el sistema." }
            };

            return allTests;
        }

        // --- TEST DE ERRORES ---
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [MemberData(nameof(TestCasesFor_CreateResena))]
        public async Task CreateResena_Error_test(ResenaForCreateDTO resenaDTO, string errorExpected)
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var result = await controller.CreateResena(resenaDTO);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.StartsWith(errorExpected, errorActual);
        }

        // --- TEST DE ÉXITO ---
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Success_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var itemsNuevos = new List<ResenaItemDTO> { new ResenaItemDTO(1, _libro1Title, _libro1Editorial, _libro1Tipo, 5, "Obra maestra.") };
            var resenaValida = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, itemsNuevos);

            var result = await controller.CreateResena(resenaValida);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualResenaDetailDTO = Assert.IsType<ResenaDetailDTO>(createdResult.Value);

            Assert.Equal(_tituloResena, actualResenaDetailDTO.Titulo);
            
            // Verificamos el cálculo de la nota media (4.0 previo + 5.0 nuevo = 4.5)
            var libroActualizado = _context.Libros.First(l => l.Titulo == _libro1Title);
            Assert.Equal(4.5m, libroActualizado.CalificacionMedia);
        }
    }
}