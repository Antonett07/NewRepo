using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRaceDayApiClient _apiClient;
        private readonly IUserSessionService _sessionService;

        public AccountController(IRaceDayApiClient apiClient, IUserSessionService sessionService)
        {
            _apiClient = apiClient;
            _sessionService = sessionService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_sessionService.IsAuthenticated)
            {
                return RedirectBasedOnRole(_sessionService.Role);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authResponse = await _apiClient.LoginAsync(model);
            if (authResponse == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password. Please try again.");
                return View(model);
            }

            var fullName = $"{authResponse.FirstName} {authResponse.LastName}".Trim();
            if (string.IsNullOrEmpty(fullName)) fullName = authResponse.Email;

            _sessionService.SetUserSession(
                authResponse.UserId,
                authResponse.Email,
                fullName,
                authResponse.Role,
                authResponse.ProfilePictureUrl,
                authResponse.Token);

            TempData["SuccessMessage"] = $"Welcome back, {fullName}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectBasedOnRole(authResponse.Role);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_sessionService.IsAuthenticated)
            {
                return RedirectBasedOnRole(_sessionService.Role);
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiClient.RegisterAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. The email may already be in use or the service is temporarily unavailable.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! Please log in with your credentials.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _sessionService.ClearSession();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutPost()
        {
            _sessionService.ClearSession();
            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!_sessionService.IsAuthenticated || !_sessionService.UserId.HasValue)
            {
                return RedirectToAction(nameof(Login), new { returnUrl = "/Account/Profile" });
            }

            var profile = await _apiClient.GetProfileAsync(_sessionService.UserId.Value, _sessionService.Token);
            if (profile == null)
            {
                // Fallback to session values
                profile = new UserProfileViewModel
                {
                    UserId = _sessionService.UserId.Value,
                    Email = _sessionService.Email ?? string.Empty,
                    FirstName = _sessionService.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
                    LastName = _sessionService.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                    Role = _sessionService.Role ?? "Participant",
                    ProfilePictureUrl = _sessionService.ProfilePictureUrl
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!_sessionService.IsAuthenticated || !_sessionService.UserId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            model.UserId = _sessionService.UserId.Value;
            model.Role = _sessionService.Role ?? "Participant";

            // If a new profile picture is uploaded, upload via Part 2 API to Azure Blob Storage
            if (model.NewProfilePicture != null && model.NewProfilePicture.Length > 0)
            {
                var uploadedUrl = await _apiClient.UploadProfilePictureAsync(model.NewProfilePicture, _sessionService.Token);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    model.ProfilePictureUrl = uploadedUrl;
                }
            }

            var success = await _apiClient.UpdateProfileAsync(model, _sessionService.Token);
            if (success)
            {
                _sessionService.UpdateProfile($"{model.FirstName} {model.LastName}".Trim(), model.ProfilePictureUrl);
                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError(string.Empty, "Unable to save profile updates. Please try again.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectBasedOnRole(string? role)
        {
            if (string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Organiser");
            }
            return RedirectToAction("Index", "Events");
        }
    }
}
