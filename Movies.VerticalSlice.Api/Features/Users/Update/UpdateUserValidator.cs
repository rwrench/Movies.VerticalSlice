using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Movies.VerticalSlice.Api.Data.Database;
using System.Text.RegularExpressions;

namespace Movies.VerticalSlice.Api.Features.Users.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    readonly MoviesDbContext _context;
    public UpdateUserValidator(MoviesDbContext context)
    {
        _context = context;
        
        // Rule to ensure at least email or password is provided
        RuleFor(x => x)
            .Must(HaveAtLeastEmailOrPassword)
            .WithMessage("Either email or password must be provided for update");

        // Setup validation rules
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters");
            
      
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(100)
            .WithMessage("Email must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));
           
        RuleFor(x => x.Password)
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters")
            .Must(BeAStrongPassword)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }

    private static bool HaveAtLeastEmailOrPassword(UpdateUserCommand command)
    {
        return !string.IsNullOrEmpty(command.Email) || 
            !string.IsNullOrEmpty(command.Password);
    }

    static bool BeAStrongPassword(string? password)
    {
        if (string.IsNullOrEmpty(password)) return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

}