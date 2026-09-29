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
        builder.Entity<SubastaItem>().HasKey(si => new { si.LibroId, si.SubastaId });   

    }


    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<SubastaItem> SubastaItem { get; set; }     



}