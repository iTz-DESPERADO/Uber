using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace Uber.Ui.Models;

public sealed class LoginModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
    public string ReturnUrl { get; set; } = string.Empty;
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];
}

public sealed class ForgotPasswordModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password must match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
