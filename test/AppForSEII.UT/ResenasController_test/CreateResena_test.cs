using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;
using AppForSEII.API.Models;

namespace AppForSEII.UT.ResenasController_test
{
    public class CreateResenas_test : AppForSEII4SqliteUT
    {
        private const string _userName = "elena.navarro@uclm.es";
        private const string _nombre = "Elena";
        private const string _apellidos = "Navarro";
        private const string _direccion = "Avda. España s/n, Albacete";
        private const string _telefono = "666555444";
        private const string _tituloResena = "Resena: Mi opinión sobre estos libros";

        private const string _libro1Title = "Dune";
        private const string _libro1Editorial = "Minotauro";
        private const string _libro1Tipo = "Bolsillo";
        
        public CreateResenas_test()
        {
            var user = new ApplicationUser(_nombre, _apellidos, _direccion, _telefono);
            user.Id = "1";
            user.UserName = _userName;
            user.Email = _userName;

            var genero = new Genero("Ciencia Ficción");
            genero.Id = 1;

            var editorial = new Editorial(_libro1Editorial);
            editorial.Id = 1;
            
            var libro = new Libro(_libro1Title, "Frank Herbert", new DateTime(1965, 8, 1), 20m, 10, editorial.Id, genero.Id, _libro1Tipo, 4.0m);
            libro.Id = 1;
            libro.Genero = genero;
            libro.Editorial = editorial;
            libro.ResenaItems = new List<ResenaItem>();

            var resenaPrevia = new Resena(user.Id, DateTime.Now, "Reseña antigua");
            resenaPrevia.Id = 1;
            resenaPrevia.Usuario = user;

            var resenaItemPrevio = new ResenaItem("Muy buen libro", 4, libro.Id, resenaPrevia.Id);
            resenaItemPrevio.Libro = libro;
            resenaItemPrevio.Resena = resenaPrevia;
            
            libro.ResenaItems.Add(resenaItemPrevio);

            _context.Users.Add(user);
            _context.Generos.Add(genero);
            _context.Editoriales.Add(editorial);
            _context.Libros.Add(libro);
            _context.Resenas.Add(resenaPrevia);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesFor_CreateResena()
        {
            var itemsValidos = new List<ResenaItemDTO> { new ResenaItemDTO(1, _libro1Title, _libro1Editorial, _libro1Tipo, 5, "Me ha encantado.") };

            var resenaSinItems = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, new List<ResenaItemDTO>());
            var resenaUsuarioFalso = new ResenaForCreateDTO("falso@uclm.es", _nombre, _apellidos, _direccion, _telefono, _tituloResena, itemsValidos);
            var resenaLibroFalso = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, 
                new List<ResenaItemDTO> { new ResenaItemDTO(99, "LibroFalso", "Ed", "Tipo", 5, "Genial") });
            
            var resenaTituloFalso = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, "Me gusta el libro", itemsValidos);

            var allTests = new List<object[]>
            { 
                new object[] { resenaSinItems, "¡Error! Debes seleccionar al menos un libro para hacer la reseña." },
                new object[] { resenaUsuarioFalso, "¡Error! El usuario no está registrado." },
                new object[] { resenaLibroFalso, "¡Error! El libro 'LibroFalso' no existe en el sistema." },
                new object[] { resenaTituloFalso, "¡Error! El título de la resena debe empezar por Resena:" }
            };

            return allTests;
        }

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

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task CreateResena_Success_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var itemsNuevos = new List<ResenaItemDTO> { new ResenaItemDTO(1, _libro1Title, _libro1Editorial, _libro1Tipo, 5, "Resena: Obra maestra.") };
            var resenaValida = new ResenaForCreateDTO(_userName, _nombre, _apellidos, _direccion, _telefono, _tituloResena, itemsNuevos);

            var result = await controller.CreateResena(resenaValida);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualResenaDetailDTO = Assert.IsType<ResenaDetailDTO>(createdResult.Value);

            var expectedDetailDTO = new ResenaDetailDTO(
                actualResenaDetailDTO.Id, 
                actualResenaDetailDTO.FechaResena, 
                _userName, 
                _nombre, 
                _apellidos, 
                _direccion, 
                _telefono, 
                _tituloResena, 
                itemsNuevos
            );

            Assert.Equal(expectedDetailDTO, actualResenaDetailDTO);
            
            var libroActualizado = _context.Libros.First(l => l.Titulo == _libro1Title);
            Assert.Equal(4.5m, libroActualizado.CalificacionMedia);
        }
    }
}