using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Shared.Dtos
{
    public class MovieNameDto
    {
        public Guid Id { get; init; } = Guid.Empty;
        public string Name { get; set; } = "";
        public MovieNameDto()
        {
          
        }

        public MovieNameDto(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
