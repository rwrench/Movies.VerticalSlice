using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Shared.Dtos
{
    public class MovieNameDto
    {
        public Guid MovieId { get; init; } = Guid.Empty;
        public string MovieName { get; set; } = "";
        public MovieNameDto()
        {
          
        }

        public MovieNameDto(Guid id, string name)
        {
            MovieId = id;
            MovieName = name;
        }
    }
}
