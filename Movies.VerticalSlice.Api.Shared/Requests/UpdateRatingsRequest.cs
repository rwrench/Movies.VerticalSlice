using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Shared.Requests;

public record 
    UpdateRatingsRequest(
        Guid MovieId,
        float Rating,
        DateTime? DateUpdated,
        string? UserId = ""
    );

