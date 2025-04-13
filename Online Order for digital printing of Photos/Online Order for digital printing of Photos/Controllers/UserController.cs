using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Online_Order_for_digital_printing_of_Photos.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class UserController : Controller
{
    private readonly MyImageDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UserController(MyImageDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Upload Photo
    public async Task<IActionResult> UploadPhoto()
    {
        // Fetch available print sizes from the database
        var printSizes = await _context.PrintSizes.ToListAsync();
        ViewBag.PrintSizes = printSizes;

        return View("~/Views/Home/UploadPhoto.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(PhotoUploadViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "You need to be logged in to upload photos.";
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid form data.";
            return RedirectToAction("UploadPhoto");
        }

        try
        {
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                // Save the photo file
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PhotoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(stream);
                }

                // Create a Photo entry in the database
                var photo = new Photo
                {
                    UserId = userId.Value,
                    PhotoName = model.PhotoFile.FileName,
                    FilePath = $"/uploads/{fileName}",
                    UploadedAt = DateTime.Now
                };

                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync();

                // Add photo to cart (OrderDetails)
                if (model.PrintSizeId.HasValue && model.Quantity > 0)
                {
                    var printSize = await _context.PrintSizes.FindAsync(model.PrintSizeId.Value);
                    if (printSize == null)
                    {
                        TempData["Error"] = "Invalid print size selected.";
                        return RedirectToAction("UploadPhoto");
                    }

                    // Get or create a pending order
                    var order = await _context.Orders
                        .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

                    if (order == null)
                    {
                        order = new Order
                        {
                            UserId = userId.Value,
                            OrderDate = DateTime.Now,
                            Status = "Pending",
                            TotalPrice = 0,
                            PaymentStatus = "Unpaid"
                        };

                        await _context.Orders.AddAsync(order);
                        await _context.SaveChangesAsync(); // Save to generate OrderId
                    }

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        PhotoId = photo.PhotoId,
                        PrintSizeId = model.PrintSizeId.Value,
                        Quantity = model.Quantity,
                        Price = model.Quantity * printSize.Price
                    };

                    await _context.OrderDetails.AddAsync(orderDetail);
                    await _context.SaveChangesAsync();

                    // Update the order's total price
                    order.TotalPrice += orderDetail.Price;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Photo uploaded successfully!";
                return RedirectToAction("Cart");
            }
            else
            {
                TempData["Error"] = "Please select a valid photo file.";
                return RedirectToAction("UploadPhoto");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while uploading the photo: {ex.InnerException?.Message ?? ex.Message}";
            return RedirectToAction("UploadPhoto");
        }
    }

    // View Cart
    public async Task<IActionResult> Cart()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "You need to be logged in to view your cart.";
            return RedirectToAction("Login", "Account");
        }

        // Fetch items in the cart
        var cartItems = await _context.OrderDetails
            .Include(o => o.PrintSize)
            .Include(o => o.Photo)
            .Where(o => o.Order.UserId == userId && o.Order.Status == "Pending")
            .ToListAsync();

        // Calculate total price
        var totalPrice = cartItems.Sum(item => item.Price);
        ViewBag.TotalPrice = totalPrice;

        return View(cartItems);
    }

    // Checkout
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "You need to be logged in to checkout.";
            return RedirectToAction("Login", "Account");
        }

        // Fetch items from the cart
        var cartItems = await _context.OrderDetails
            .Include(o => o.PrintSize)
            .Include(o => o.Photo)
            .Where(o => o.Order.UserId == userId && o.Order.Status == "Pending")
            .ToListAsync();

        // Calculate the total amount
        var totalAmount = cartItems.Sum(item => item.Price);

        // Populate the CheckoutViewModel
        var checkoutViewModel = new CheckoutViewModel
        {
            CartItems = cartItems.Select(o => new CartItem
            {
                Photo = o.Photo.PhotoName,
                Quantity = o.Quantity,
                Price = o.Price,
            }).ToList(),
            TotalAmount = totalAmount
        };

        return View(checkoutViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "You need to be logged in to checkout.";
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return View(model);
        }

        try
        {
            // Fetch the user's pending order
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == "Pending");

            if (order == null)
            {
                TempData["Error"] = "No items found in your cart.";
                return RedirectToAction("Cart");
            }

            // Update the order status
            order.Status = "Completed";
            order.PaymentStatus = model.PaymentMethod == "Credit Card" ? "Paid" : "Unpaid";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Checkout completed successfully!";
            return RedirectToAction("CompleteOrder");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred during checkout: {ex.Message}";
            return RedirectToAction("Checkout");
        }
    }

    // Order Confirmation Page
    public IActionResult CompleteOrder()
    {
        return View("CompleteOrder");
    }

    public async Task<IActionResult> About()
    {
        return View("~/Views/Home/About.cshtml");
    }
}
