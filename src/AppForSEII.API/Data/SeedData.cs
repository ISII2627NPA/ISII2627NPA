using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AppForSEII.API.Models; 

namespace AppForSEII.API.Data {
    public class SeedData {
        public static void Initialize(ApplicationDbContext dbContext, IServiceProvider serviceProvider, ILogger logger) {
            // 4 Tuplas de Roles
            List<string> rolesNames = new List<string> { "Administrador", "Empleado", "Cliente", "Auditor" };

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            try {
                SeedRoles(roleManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Ocurrió un error al inicializar los roles en la base de datos.");
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            try {
                SeedUsers(userManager, rolesNames);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Ocurrió un error al inicializar los Usuarios en la base de datos.");
            }

            try {
                SeedGenerosEditorialesYLibros(dbContext);
            }
            catch (Exception ex) {
                logger.LogError(ex, "Ocurrió un error al inicializar los Libros, Géneros y Editoriales en la base de datos.");
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager, List<string> roles) {
            foreach (string roleName in roles) {
                if (!roleManager.RoleExistsAsync(roleName).Result) {
                    IdentityRole role = new IdentityRole();
                    role.Name = roleName;
                    role.NormalizedName = roleName.ToUpper();
                    IdentityResult roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }

        public static void SeedUsers(UserManager<ApplicationUser> userManager, List<string> roles) {
            // Tupla 1 (Administrador)
            if (userManager.FindByNameAsync("elena@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("Elena", "Navarro Martínez", "Dirección por defecto", "600000000");
                user.UserName = "elena@uclm.es";
                user.Email = "elena@uclm.es";
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "Password1234%");
                result.Wait();
                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[0]).Wait();
                }
            }

            // Tupla 2 (Empleado)
            if (userManager.FindByNameAsync("gregorio@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("Gregorio", "Diaz Descalzo", "Dirección por defecto", "600000000");
                user.UserName = "gregorio@uclm.es";
                user.Email = "gregorio@uclm.es";
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "APassword1234%");
                result.Wait();
                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[1]).Wait();
                }
            }

            // Tupla 3 (Cliente)
            if (userManager.FindByNameAsync("peter@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("Peter", "Jackson", "Dirección por defecto", "600000000");
                user.UserName = "peter@uclm.es";
                user.Email = "peter@uclm.es";
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "OtherPass12$");
                result.Wait();
                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[2]).Wait();
                }
            }

            // Tupla 4 (Auditor)
            if (userManager.FindByNameAsync("ana@uclm.es").Result == null) {
                ApplicationUser user = new ApplicationUser("Ana", "García", "Dirección por defecto", "600000000");
                user.UserName = "ana@uclm.es";
                user.Email = "ana@uclm.es";
                user.EmailConfirmed = true;

                var result = userManager.CreateAsync(user, "AuditPass12$");
                result.Wait();
                if (result.IsCompletedSuccessfully) {
                    userManager.AddToRoleAsync(user, roles[3]).Wait();
                }
            }
        }

        public static void SeedGenerosEditorialesYLibros(ApplicationDbContext dbcontext) {
            // 5 Tuplas de Géneros
            string[] nombresGeneros = ["Ciencia Ficción", "Drama", "Comedia", "Novela Histórica", "Fantasía"];
            List<Genero> generos = [];
            
            foreach (string nombreGenero in nombresGeneros) {
                var genero = dbcontext.Generos.FirstOrDefault(g => g.Nombre == nombreGenero);
                if (genero == null) {
                    var nuevoGenero = new Genero();
                    nuevoGenero.Nombre = nombreGenero;
                    generos.Add(nuevoGenero);
                    dbcontext.Generos.Add(nuevoGenero);
                }
                else {
                    generos.Add(genero);
                }
            }
            dbcontext.SaveChanges();

            // 4 Tuplas de Editoriales
            string[] nombresEditoriales = ["Planeta", "Minotauro", "Penguin", "Anaya"];
            List<Editorial> editoriales = [];
            
            foreach (string nombreEditorial in nombresEditoriales) {
                var editorial = dbcontext.Editoriales.FirstOrDefault(e => e.Nombre == nombreEditorial);
                if (editorial == null) {
                    var nuevaEditorial = new Editorial();
                    nuevaEditorial.Nombre = nombreEditorial;
                    editoriales.Add(nuevaEditorial);
                    dbcontext.Editoriales.Add(nuevaEditorial);
                }
                else {
                    editoriales.Add(editorial);
                }
            }
            dbcontext.SaveChanges();

            Libro libro;

            // Tupla 1 (Libro) 
            if (dbcontext.Libros.FirstOrDefault(m => m.Titulo == "Dune") == null) {
                libro = new Libro("Dune", "Frank Herbert", new DateTime(1965, 08, 01), 19.99m, 50, editoriales[1].Id, generos[0].Id, "Edición de bolsillo", 4.5m);
                dbcontext.Libros.Add(libro);
            }

            // Tupla 2 (Libro)
            if (dbcontext.Libros.FirstOrDefault(m => m.Titulo == "Los Pilares de la Tierra") == null) {
                libro = new Libro("Los Pilares de la Tierra", "Ken Follett", new DateTime(1989, 10, 01), 29.90m, 15, editoriales[0].Id, generos[3].Id, "Edición en tapa dura", 4.8m);
                dbcontext.Libros.Add(libro);
            }

            // Tupla 3 (Libro)
            if (dbcontext.Libros.FirstOrDefault(m => m.Titulo == "1984") == null) {
                libro = new Libro("1984", "George Orwell", new DateTime(1949, 06, 08), 15.50m, 40, editoriales[2].Id, generos[1].Id, "Edición de bolsillo", 4.6m);
                dbcontext.Libros.Add(libro);
            }

            // Tupla 4 (Libro)
            if (dbcontext.Libros.FirstOrDefault(m => m.Titulo == "El Hobbit") == null) {
                libro = new Libro("El Hobbit", "J.R.R. Tolkien", new DateTime(1937, 09, 21), 22.00m, 25, editoriales[1].Id, generos[4].Id, "Edición tapa blanda", 4.7m);
                dbcontext.Libros.Add(libro);
            }

            dbcontext.SaveChanges();
            
            
            dbcontext.Libros.ExecuteUpdate(s => s.SetProperty(m => m.Stock, 10));
            dbcontext.SaveChanges();
        }
    }
}




    
