using AppForSEII.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.DTOs.ApplicationUserDTO;

namespace AppForSEII.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }



    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<CompraItem> CompraItems { get; set; }


    public DbSet<SubastaItem> SubastaItems { get; set; } 



    public DbSet<Genero> Generos { get; set; }

    public DbSet<Libro> Libros { get; set; }


    public DbSet<Editorial> Editoriales { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }

    public DbSet<Resena> Resenas { get; set; }



    public DbSet<Subasta> Subastas { get; set; }
}