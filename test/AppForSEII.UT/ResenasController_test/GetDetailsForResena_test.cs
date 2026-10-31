using System;
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
    public class GetDetailsForResena_test : AppForSEII4SqliteUT
    {
        public GetDetailsForResena_test()
        {
            var user = new ApplicationUser("Peter", "Jackson", "Calle", "600") { Id = "1", UserName = "peter@uclm.es", Email = "peter@uclm.es" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var resena = new Resena(user.Id, DateTime.Now, "Reseña de prueba") { Id = 1, Usuario = user };
            _context.Resenas.Add(resena);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_Found_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var result = await controller.GetDetailsForResena(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var detail = Assert.IsType<ResenaDetailDTO>(okResult.Value);
            Assert.Equal(1, detail.Id);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForResena_NotFound_test()
        {
            var mock = new Mock<ILogger<ResenasController>>();
            var controller = new ResenasController(_context, mock.Object);

            var result = await controller.GetDetailsForResena(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}