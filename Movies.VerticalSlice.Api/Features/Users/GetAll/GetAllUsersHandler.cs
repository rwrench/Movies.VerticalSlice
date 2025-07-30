using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using Movies.VerticalSlice.Api.Features.Users.GetAll;
using Movies.VerticalSlice.Api.Shared.Dtos;
using System.Linq;

namespace Users.VerticalSlice.Api.Features.Users.GetAll;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UsersDto>>
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(
        MoviesDbContext context, 
        ILogger<GetAllUsersHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<UsersDto>> Handle(
        GetAllUsersQuery query, 
        CancellationToken token)
    {
        var UsersQuery = _context.Users.AsQueryable();
        var Users = await UsersQuery.AsNoTracking().ToListAsync(token);

        var result = Users.Select(x => new UsersDto(
              x.Id,
              x.UserName!,
              x.Email!
          )).AsEnumerable();
        _logger.LogInformation("Retrieved {result} Users", result.Count());
        return result;
    }


}
