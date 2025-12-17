namespace Movies.VerticalSlice.Api.Shared.Constants
{
    public static class ApiEndpoints
    {
        public const string ApiBase = "/api";
        public static class Movies
        {
            public const string Base = $"{ApiBase}/movies";
            public const string Create = Base;
            public const string Get = "{idOrSlug}";
            public const string GetAll = Base;
            public const string Update = Base;
            public const string Delete = Base + "/{id}";
            public const string Rate = $"{Base}/{{movieId:guid}}/ratings";
            public const string DeleteRating = Rate;
            public const string Names = Base + "/names";

            /// <summary>
            /// Constructs the URL for updating a movie with query string parameter
            /// </summary>
            public static string UpdateWithId(Guid id) => $"{Update}?id={id}";

            /// <summary>
            /// Constructs the URL for deleting a movie by replacing the route parameter
            /// </summary>
            public static string DeleteWithId(Guid id) => $"{Base}/{id}";
        }


        public static class Ratings
        {
            public const string Base = $"{ApiBase}/movies/ratings";
            public const string GetUserRatings = $"{Base}/me";
            public const string GetAll = Base;
        }

        public static class Users
        {
            const string Base = $"{ApiBase}/users";
            public const string Register = Base;
            public const string Login = $"{Base}/login";
            public const string Update = $"{Base}/{{userName}}";
            public const string GetAll = Base; 
        }
    }

}
