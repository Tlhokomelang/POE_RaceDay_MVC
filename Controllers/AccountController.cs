using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Helpers;
using POE_RaceDay_MVC.Models;
using POE_RaceDay_MVC.Services;

namespace POE_RaceDay_MVC.Controllers
{
    public class AccountController : AppController
    {
        private readonly IRaceDayApi _api;
        private readonly ISessionUser _user;

        public AccountController(IRaceDayApi api, ISessionUser user)
        {
            _api = api;
            _user = user;
        }

        // ---------- Login ----------

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_user.IsAuthenticated) return RedirectToRoleHome(_user);

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await SignInAsync(model.Email, model.Password);

                SetSuccess($"Welcome back, {UiHelpers.FirstName(_user.Name)}.");

                if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToRoleHome(_user);
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty,
                    ex.StatusCode == 401
                        ? "Invalid email or password."
                        : ex.Message);

                model.Password = string.Empty;

                return View(model);
            }
        }

        // ---------- Register ----------

        [HttpGet]
        public IActionResult Register()
        {
            if (_user.IsAuthenticated) return RedirectToRoleHome(_user);

            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _api.RegisterAsync(new RegisterApiRequest
                {
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    Role = model.Role,
                    PhoneNumber = model.PhoneNumber
                });
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 409)
                    ModelState.AddModelError(nameof(model.Email), ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);

                model.Password = string.Empty;
                model.ConfirmPassword = string.Empty;

                return View(model);
            }

            // Account created: sign the new user straight in.
            try
            {
                await SignInAsync(model.Email, model.Password);

                SetSuccess($"Welcome to RaceDay, {UiHelpers.FirstName(_user.Name)}!");

                return RedirectToRoleHome(_user);
            }
            catch (ApiException)
            {
                SetSuccess("Your account has been created. Please log in.");

                return RedirectToAction(nameof(Login));
            }
        }

        // ---------- Logout ----------

        [HttpPost]
        public IActionResult Logout()
        {
            _user.SignOut();

            SetSuccess("You have been logged out.");

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ---------- Profile (both roles) ----------

        [HttpGet]
        [RoleAuthorize]
        public async Task<IActionResult> Profile()
        {
            ApiUser profile = await _api.GetProfileAsync();

            _user.ProfilePicture = profile.ProfilePicture;

            return View(ToViewModel(profile));
        }

        [HttpPost]
        [RoleAuthorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillReadOnlyAsync(model);
                return View(model);
            }

            try
            {
                await _api.UpdateProfileAsync(
                    model.FullName.Trim(), model.PhoneNumber);

                _user.UpdateName(model.FullName.Trim());

                SetSuccess("Your profile has been updated.");

                return RedirectToAction(nameof(Profile));
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillReadOnlyAsync(model);

                return View(model);
            }
        }

        [HttpPost]
        [RoleAuthorize]
        public async Task<IActionResult> ProfilePicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                SetError("Please choose a picture to upload.");
                return RedirectToAction(nameof(Profile));
            }

            string? problem = UploadValidator.Validate(picture);

            if (problem != null)
            {
                SetError(problem);
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                string? blob = await _api.UploadProfilePictureAsync(picture);

                _user.ProfilePicture = blob;

                SetSuccess("Your profile picture has been updated.");
            }
            catch (ApiException ex) when (IsRecoverable(ex))
            {
                SetError(ex.Message);
            }

            return RedirectToAction(nameof(Profile));
        }

        // ---------- helpers ----------

        private async Task SignInAsync(string email, string password)
        {
            LoginResponse login = await _api.LoginAsync(email.Trim(), password);

            _user.SignIn(login);

            // Cache the avatar for the navigation bar (best effort).
            try
            {
                ApiUser profile = await _api.GetProfileAsync();
                _user.ProfilePicture = profile.ProfilePicture;
            }
            catch (ApiException)
            {
            }
        }

        private static ProfileViewModel ToViewModel(ApiUser profile) => new()
        {
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Role = profile.Role,
            MemberSince = profile.CreatedAt,
            ProfilePicture = profile.ProfilePicture
        };

        // Read-only fields are not posted back, so reload them for redisplay.
        private async Task FillReadOnlyAsync(ProfileViewModel model)
        {
            try
            {
                ApiUser profile = await _api.GetProfileAsync();

                model.Email = profile.Email;
                model.Role = profile.Role;
                model.MemberSince = profile.CreatedAt;
                model.ProfilePicture = profile.ProfilePicture;
            }
            catch (ApiException)
            {
                model.Email = _user.Email;
                model.Role = _user.Role;
            }
        }
    }
}
