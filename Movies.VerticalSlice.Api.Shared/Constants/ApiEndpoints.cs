namespace Movies.VerticalSlice.Api.Shared.Constants
{
    public static class ApiEndpoints
    {
        public const string ApiBase = "/api";
        public static class Movies
        {
            public const string Base = $"{ApiBase}/movies";
            public const string Create = Base; // or null
            public const string Get = "{idOrSlug}";
            public const string GetAll = Base; // or null
            public const string Update = Base;
            public const string Delete = Base;
            public const string Rate = $"{Base}/{{movieId:guid}}/ratings";
            public const string DeleteRating = Rate;
            public const string Names = Base + "/names";
        }


        public static class Ratings
        {
            public const string Base = $"{ApiBase}/movies/ratings";
            public const string GetUserRatings = $"{Base}/me";
            public const string GetAll = Base; // or n
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
