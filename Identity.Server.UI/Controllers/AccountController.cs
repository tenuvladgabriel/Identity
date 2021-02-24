using System;
using System.Threading.Tasks;
using Identity.Server.UI.Models;
using IdentityServer.Core.Entites;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.Server.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IEventService events,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var loginModel = new LoginInputModel
            {
                ReturnUrl = returnUrl
            };

            return View(loginModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, true);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName,
                        clientId: context?.Client.ClientId));

                    if (context != null)
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect("redirect to client url");
                    }

                    throw new Exception("Invalid return URL");
                }
                
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Invalid credentials",
                    clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, "Your credentials is invalid.");
            }

            var loginModelWithErrors = new LoginInputModel
            {
                Username = model.Username,
                ReturnUrl = model.ReturnUrl
            };

            return View(loginModelWithErrors);
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterInputModel());
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = ApplicationUser.Create(model.Email, model.FirstName, model.LastName, true);

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"The user with id {newUser.Id} was created");
                }
            }

            return View();
        }
    }
}