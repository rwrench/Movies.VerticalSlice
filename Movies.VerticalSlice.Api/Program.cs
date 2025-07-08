using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.Movies.Create;
using Movies.VerticalSlice.Api.Features.Movies.Delete;
using Movies.VerticalSlice.Api.Features.Movies.GetAll;
using Movies.VerticalSlice.Api.Features.Movies.Update;
using Movies.VerticalSlice.Api.Features.Ratings.Create;
using Movies.VerticalSlice.Api.Features.Ratings.GetAll;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MoviesProfile>();
    cfg.AddProfile<RatingsProfile>();    
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Map movie endpoints
app.MapCreateMovie();
app.MapGetAllMovies();  
app.MapUpdateMovie(); // Ensure this is called after Create and GetAll  
app.MapDeleteMovie(); // Ensure this is called after Create and GetAll
app.MapRateMovie(); // Ensure this is called after Create and GetAll    
app.MapGetAllRatings(); // Ensure this is called after Create and GetAll


app.Run();


