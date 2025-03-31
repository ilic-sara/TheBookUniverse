using AutoMapper;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Users;

namespace TheBookUniverse.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class HomeController : Controller
    {
        private readonly UserManager<Data.User> userManager;
        private readonly SignInManager<Data.User> signInManager;
        private readonly RoleManager<Data.Role> roleManager;
        private readonly IMapper mapper;
        private readonly ILogger<HomeController> logger;

        public HomeController(UserManager<Data.User> userManager,
                                  SignInManager<Data.User> signInManager,
                                  RoleManager<Data.Role> roleManager,
                                  IMapper mapper,
                                  ILogger<HomeController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
            this.logger = logger;

            //CreateRoles().Wait();
            //CreateAdminUser().Wait();
        }

        /*
        private async Task CreateRoles()
        {
            try
            {
                string[] roles = { "Admin", "User" };
                foreach (var roleName in roles)
                {
                    bool roleExists = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        var role = new Data.Role();
                        role.Name = roleName;
                        await roleManager.CreateAsync(role);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] CreateRoles :: An error occured while trying to create roles.\n{ex}");
            }

        }

        private async Task CreateAdminUser()
        {
            try
            {
                string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? throw new Exception("Environment variables missing.");
                string adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? throw new Exception("Environment variables missing.");
                string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? throw new Exception("Environment variables missing.");

                Data.User? adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser is null)
                {
                    var admin = new Data.User
                    {
                        UserName = adminUsername,
                        Email = adminEmail
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] CreateAdminUser :: An error occured while trying to create admin user.\n{ex}");
            }

        }*/

        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registration(RegistrationModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Registration failed.";
                    return View(model);
                }

                Data.User user = mapper.Map<Data.User>(model);
                Role? userRole = await roleManager.FindByNameAsync("User");
                if (userRole is null)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Registration failed.";
                    return View(model);
                }
                user.Roles.Add(userRole.Id);
                var result = await userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errorString = "";
                    if (result.Errors is { } && result.Errors.Any())
                    {
                        foreach (var identityError in result.Errors)
                        {
                            errorString += "\n" + identityError.Description;
                        }
                    }

                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = errorString;
                    return View(model);
                }


                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("ViewStartPage", "Home", new { area = "Common" });
            }
            catch (Exception ex)
            {
                TempData["Response"] = false;
                TempData["ResponseMessage"] = "Registration failed."; 

                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] Registration :: An error occured while trying to add new user.\n{ex}");
                return View(model);
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Login failed.";
                    return View(model);
                }

                var result = signInManager.PasswordSignInAsync(model.UserName, model.Password, true, false);

                if (result.Result.Succeeded)
                {
                    var returnUrl = "/Common/Home/ViewStartPage";
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    TempData["Response"] = false;
                    TempData["ResponseMessage"] = "Login failed.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] Login :: An error occured while trying to log in user with username {model.UserName}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }

        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        public async Task<ActionResult> Logout()
        {
            try
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("ViewStartPage", "Home", new { area = "Common" });
            }
            catch (Exception ex)
            {
                var user = await userManager.GetUserAsync(User);
                string userName = user?.UserName ?? "unknown" ;
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                                    $"[ERROR] Logout :: An error occured while trying to logout user {userName}.\n{ex}");

                return StatusCode(500, "Internal server error");
            }

        }
    }
}
