using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Forum2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Forum2.Controllers
{
    public class LoginRegController : Controller
    {
        private ForumContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public LoginRegController(
            ForumContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        private Task<User> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
        [HttpGet("")]
        public IActionResult Register()
        {
            if ((HttpContext.User != null) && HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Forum");
            }
            return View();
        }
        [HttpPost("register")]
        public async Task<IActionResult> Registering(Register model)
        {
            if(ModelState.IsValid)
            {
                //Create a new User object, without adding a Password
                User NewUser = new User { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName,  };
                //CreateAsync will attempt to create the User in the database, simultaneously hashing the
                //password
                IdentityResult result = await _userManager.CreateAsync(NewUser, model.Password);
                //If the User was added to the database successfully
                if(result.Succeeded)
                {
                    //Sign In the newly created User
                    //We're using the SignInManager, not the UserManager!
                    await _signInManager.SignInAsync(NewUser, isPersistent: false);
                    if(_context.users.Count() == 1)
                    {
                        await _userManager.AddToRoleAsync(NewUser, "Level2");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(NewUser, "Level1");
                    }
                    TempData["login"] = $"{model.UserName} has succesfully registered.";
                    return RedirectToAction("Index", "Forum");
                }
                //If the creation failed, add the errors to the View Model
                foreach( var error in result.Errors )
                {
                    ModelState.AddModelError("Password", error.Description);
                }
            }
            return View("Register");
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            if ((HttpContext.User != null) && HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Forum");
            }
            return View();
        }
        [HttpPost("logging")]
        public async Task<IActionResult> Logging(Login model)
        {
            if(ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
                if(result.Succeeded)
                {
                    TempData["login"] = $"{model.UserName} has succesfully logged in.";
                    return RedirectToAction("Index", "Forum");
                }
                ModelState.AddModelError("Password", "Your UserName and Password did not match");
            }
            return View("Login", model);
        }
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
