using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Shared.Dtos;

public record UsersDto(string id, string UserName, string Email)
{
    public UsersDto() : this(string.Empty, string.Empty, string.Empty)
    {
    }
}
