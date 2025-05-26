using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task_Management_System.Helpers;
using Task_Management_System.Models;
using Task_Management_System.Models.Dtos;
using Task_Management_System.Models.DTOs;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "Username already exists.");
            return View(model);
        }

        var existingEmailUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmailUser != null)
        {
            ModelState.AddModelError("Email", "Email already exists.");
            return View(model);
        }

        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            Name = model.Name,
            Gender = model.Gender,
            JoinDate = model.JoinDate,
            Status = "Active"
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = encodedToken }, protocol: Request.Scheme);

            var emailSender = new EmailSender();
            await emailSender.SendEmailAsync(model.Email, "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

            return RedirectToAction("RegisterConfirmation");
        }

        var passwordErrors = result.Errors
              .Where(e => e.Code.Contains("Password"))
              .Select(e => e.Description)
              .ToList();

        foreach (var key in ModelState.Keys.Where(k => k.Contains("Password")).ToList())
        {
            ModelState[key].Errors.Clear();
        }

        foreach (var err in passwordErrors)
        {
            ModelState.AddModelError(string.Empty, err);
        }

        foreach (var error in result.Errors.Where(e => !e.Code.Contains("Password")))
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
            return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        TempData["Message"] = result.Succeeded
            ? "Email confirmed successfully. You can now log in."
            : "Email confirmation failed.";

        return RedirectToAction("Login");
    }


    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "Please confirm your email before logging in.");
            return View(model);
        }

        if (user.Status != "Active")
        {
            ModelState.AddModelError("", "Your account is not active.");
            return View(model);
        }

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        var token = await GenerateJwtToken(user);

        Response.Cookies.Append("JwtToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = model.RememberMe ? DateTimeOffset.Now.AddDays(30) : DateTimeOffset.Now.AddHours(3)
        });

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
            return RedirectToAction("Index", "User");
        else if (roles.Contains("Manager"))
            return RedirectToAction("Index", "UserTasks");

        return RedirectToAction("MyTasks", "UserTasks");
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Cookies.Delete("JwtToken");
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "No user found with this email.");
            return View();
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "Email is not confirmed. Please confirm your email before resetting password.");
            return View();
        }
       
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = encodedToken
            }, protocol: Request.Scheme);

            var emailSender = new EmailSender();
            await emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");
        

        return RedirectToAction("ForgotPasswordConfirmation");
    }


    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }

        var model = new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null || user.Email != model.Email)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your password has been changed.";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ManageProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByIdAsync(id.ToString());

        var model = new ManageProfileViewModel
        {
            UserName = user.UserName,
            Name = user.Name,
            Email = user.Email,
            Gender = user.Gender
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageProfile(ManageProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        user.Name = model.Name;
        user.Gender = model.Gender;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("ManageProfile");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }



    private async Task<string> GenerateJwtToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email) 
    };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ChangeEmail()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login");

        // Verify current password
        if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
        {
            ModelState.AddModelError("CurrentPassword", "The password is incorrect.");
            return View(model);
        }

        // Check if email is already in use
        var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            ModelState.AddModelError("NewEmail", "This email is already in use.");
            return View(model);
        }

        // Generate email change token
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // Create confirmation link
        var callbackUrl = Url.Action(
            "ConfirmEmailChange",
            "Account",
            new { userId = user.Id, newEmail = model.NewEmail, token = encodedToken },
            protocol: Request.Scheme);

        // Send confirmation email
        var emailSender = new EmailSender();
        await emailSender.SendEmailAsync(
            model.NewEmail,
            "Confirm your email change",
            $"Please confirm your email change by <a href='{callbackUrl}'>clicking here</a>.");

        TempData["Message"] = "Confirmation link to change email has been sent. Please check your email.";
        return RedirectToAction("ManageProfile");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string newEmail, string token)
    {
        if (userId == null || newEmail == null || token == null)
            return RedirectToAction("Index", "Home");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);

        if (result.Succeeded)
        {
            // Update the username if it matches the old email
            if (user.UserName == user.Email)
            {
                await _userManager.SetUserNameAsync(user, newEmail);
            }

            // Refresh the sign-in cookie
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Your email has been changed successfully.";
            return RedirectToAction("ManageProfile");
        }

        TempData["Error"] = "Error changing your email.";
        return RedirectToAction("ManageProfile");
    }

}
