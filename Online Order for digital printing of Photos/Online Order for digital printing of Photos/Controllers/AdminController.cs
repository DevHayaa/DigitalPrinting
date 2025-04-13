using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; // For action filters
using Microsoft.EntityFrameworkCore;
using Online_Order_for_digital_printing_of_Photos.Models;
using System.Security.Cryptography;


namespace Online_Order_for_digital_printing_of_Photos.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyImageDbContext _context;

        // Constructor for dependency injection
        public AdminController(MyImageDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            if (!IsAuthenticated("Admin"))
            {
                return RedirectToAction("Login", "Admin");
            }

            return View();
        }

        private bool IsAuthenticated(string requiredRole)
        {
            var userRole = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(userRole) && userRole == requiredRole;
        }

        // GET: Admin Register
        // GET: Admin Register
        public IActionResult Register()
        {
            return View("~/Views/Admin/Register.cshtml");
        }

        // POST: Admin Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid) return View(user);

            // Ensure the role is Admin
            user.Role = "Admin";

            // Hash the password
            user.Password = HashPassword(user.Password);

            // Save user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: Admin Login
        public IActionResult Login()
        {
            return View("~/Views/Admin/Login.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = HashPassword(model.Password);
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword && u.Role == "Admin");

            if (admin == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Set session values
            HttpContext.Session.SetInt32("UserId", admin.UserId);
            HttpContext.Session.SetString("Role", admin.Role);

            return RedirectToAction(""); // Replace "Admin" with your admin dashboard controller
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
  

        // View Users
        public async Task<IActionResult> ViewUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Edit User (GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            return View(user);
        }

        // Edit User (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            var user = await _context.Users.FindAsync(updatedUser.UserId);
            if (user == null)
                return NotFound();

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Phone = updatedUser.Phone;
            user.Address = updatedUser.Address;

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewUsers");
        }

        // Delete User
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewUsers");
        }

        // View Photos
        public async Task<IActionResult> ViewPhotos()
        {
            var photos = await _context.Photos.Include(p => p.User).ToListAsync();
            return View(photos);
        }

        // View Orders
        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _context.Orders.Include(o => o.User).ToListAsync();
            return View(orders);
        }

        // View Payments
        public async Task<IActionResult> ViewPayments()
        {
            var payments = await _context.Payments.Include(p => p.Order).ToListAsync();
            return View(payments);
        }

        // View Admins
        public async Task<IActionResult> ViewAdmins()
        {
            var admins = await _context.Admins.ToListAsync();
            return View(admins);
        }

        // View Folders
        public async Task<IActionResult> ViewFolders()
        {
            var folders = await _context.Folders.Include(f => f.Order).ToListAsync();
            return View(folders);
        }

        // Create Print Size (GET)
        [HttpGet]
        public IActionResult CreatePrintSize()
        {
            return View(new PrintSize());
        }

        // Create Print Size (POST)
        [HttpPost]
        public async Task<IActionResult> CreatePrintSize(PrintSize model)
        {
            if (ModelState.IsValid)
            {
                _context.PrintSizes.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewPrintSizes");
            }
            return View(model);
        }

        // View Print Sizes
        public async Task<IActionResult> ViewPrintSizes()
        {
            var printSizes = await _context.PrintSizes.ToListAsync();
            return View(printSizes);
        }

        // Edit Print Size (GET)
        [HttpGet]
        public async Task<IActionResult> EditPrintSize(int id)
        {
            var printSize = await _context.PrintSizes.FindAsync(id);
            if (printSize == null)
                return NotFound();
            return View(printSize);
        }

        // Edit Print Size (POST)
        [HttpPost]
        public async Task<IActionResult> EditPrintSize(PrintSize updatedPrintSize)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(updatedPrintSize).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewPrintSizes");
            }
            return View(updatedPrintSize);
        }

        // Delete Print Size
        public async Task<IActionResult> DeletePrintSize(int id)
        {
            var printSize = await _context.PrintSizes.FindAsync(id);
            if (printSize == null)
                return NotFound();

            _context.PrintSizes.Remove(printSize);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewPrintSizes");
        }
      [HttpPost]
        public IActionResult ProcessOrder(int orderId)
        {
            // Code to mark order as processed
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            return RedirectToAction("Index");
        }
    }
}
