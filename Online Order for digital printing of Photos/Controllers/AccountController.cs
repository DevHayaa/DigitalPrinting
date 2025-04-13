using Microsoft.AspNetCore.Mvc;
using Online_Order_for_digital_printing_of_Photos.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Online_Order_for_digital_printing_of_Photos.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyImageDbContext _context;

        public AccountController(MyImageDbContext context)
        {
            _context = context;
        }

        // GET: Register
        // GET: Register
        public IActionResult Register()
        {
            return View("~/Views/Home/Account/Register.cshtml");
        }

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid) return View(user);

            // Hash the password
            user.Password = HashPassword(user.Password);

            // Assign default role if not provided
            user.Role = "User";

            // Save user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: Login
        public IActionResult Login()
        {
            return View("~/Views/Home/Account/Login.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword && u.Role == "User");

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Set session values
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home"); // Replace "User" with your default user dashboard controller
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    

// GET: Logout
public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session
            return RedirectToAction("Login");
        }

      

    }

}
