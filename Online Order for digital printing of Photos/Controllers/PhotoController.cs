using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For ToListAsync()
using Online_Order_for_digital_printing_of_Photos.Models;

public class PhotoController : Controller
{
    private readonly MyImageDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PhotoController(MyImageDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Display Upload and Print Options
    public async Task<IActionResult> Upload()
    {
        var printSizes = await _context.PrintSizes.ToListAsync();
        ViewBag.PrintSizes = printSizes;

        return View();
    }

    // POST: Upload Photos
    [HttpPost]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var userId = 1; // Replace with logged-in user's ID
        var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            if (file.ContentType == "image/jpeg")
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var photo = new Photo
                {
                    UserId = userId,
                    PhotoName = file.FileName,
                    FilePath = "/uploads/" + fileName,
                    UploadedAt = DateTime.Now
                };

                _context.Photos.Add(photo);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("SelectPhotos");
    }

    // GET: Select Uploaded Photos and Print Sizes
    public async Task<IActionResult> SelectPhotos()
    {
        var userId = 1; // Replace with logged-in user's ID
        var photos = await _context.Photos.Where(p => p.UserId == userId).ToListAsync();
        var printSizes = await _context.PrintSizes.ToListAsync();

        ViewBag.PrintSizes = printSizes;
        return View(photos);
    }

    // POST: Create Order
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(List<int> photoIds, List<int> printSizeIds, List<int> quantities)
    {
        var userId = 1; // Replace with logged-in user's ID
        decimal totalPrice = 0;

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Pending",
            PaymentStatus = "Pending"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        for (int i = 0; i < photoIds.Count; i++)
        {
            var printSize = await _context.PrintSizes.FindAsync(printSizeIds[i]);
            var photo = await _context.Photos.FindAsync(photoIds[i]);
            if (photo != null && printSize != null)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    PhotoId = photoIds[i],
                    PrintSizeId = printSizeIds[i],
                    Quantity = quantities[i],
                    Price = printSize.Price * quantities[i]
                };

                totalPrice += orderDetail.Price;
                _context.OrderDetails.Add(orderDetail);
            }
        }

        order.TotalPrice = totalPrice;
        await _context.SaveChangesAsync();

        return RedirectToAction("OrderSummary", new { orderId = order.OrderId });
    }
}
