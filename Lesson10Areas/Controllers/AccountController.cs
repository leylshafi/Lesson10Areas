using Lesson10Areas.Models;
using Lesson10Areas.Models.ViewModels;
using Lesson10Areas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Common;

namespace Lesson10Areas.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IMailService mailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IOptions<MailSettings> mailSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            mailService = new MailService(mailSettings);
        }

        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Year = model.Year
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var link = Url.Action("ConfirmEmail", "Account", new { token, email = user.Email }, Request.Scheme);
                    var mailRequest = new MailRequest()
                    {
                        ToEmail = model.Email,
                        Subject = "Confirmation Email",
                        Body = link
                    };
                    bool emailResponse = mailService.SendEmailAsync(mailRequest);

                    if (emailResponse)
                        await signInManager.SignInAsync(user, false);



                    return RedirectToAction("Index", "Home");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return View("Error");

            var result = await userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromForm] MailRequest request)
        {
            try
            {
                mailService.SendEmailAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    if (await userManager.IsEmailConfirmedAsync(user))
                    {
                        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                        if (result.Succeeded)
                        {
                            if (!string.IsNullOrWhiteSpace(returnUrl))
                                return Redirect(returnUrl);
                            return Redirect("/");
                        }
                        else if (result.IsLockedOut)
                            ModelState.AddModelError("All", "Lockout");
                    }
                    else
                        ModelState.AddModelError("All", "Mail has not confired yet!!");

                }
                else
                    ModelState.AddModelError("login", "Incorrect username or password");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
