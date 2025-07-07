using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Data;

public class MoviesDbContext : DbContext
{
    public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options) { }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieRating> Ratings { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Movie 1 - * MovieRating
        modelBuilder.Entity<MovieRating>()
            .HasOne<Movie>()
            .WithMany()
            .HasForeignKey(r => r.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        // Add indexes for Movies table
        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.Title)
            .HasDatabaseName("IX_Movies_Title");

        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.YearOfRelease)
            .HasDatabaseName("IX_Movies_YearOfRelease");

        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.UserId)
            .HasDatabaseName("IX_Movies_UserId");

        // Composite index for common queries
        modelBuilder.Entity<Movie>()
            .HasIndex(m => new { m.YearOfRelease, m.Title })
            .HasDatabaseName("IX_Movies_Year_Title");
    }
}