using Microsoft.AspNetCore.Mvc;
using Online_Order_for_digital_printing_of_Photos.Models;
using Microsoft.EntityFrameworkCore;


public class OrderController : Controller
{
    private readonly MyImageDbContext _context;

    public OrderController(MyImageDbContext context)
    {
        _context = context;
    }

    public IActionResult Cart()
    {
        var cartItems = _context.OrderDetails
            .Include(od => od.Photo)
            .Include(od => od.PrintSize)
            .ToList();
        return View(cartItems);
    }
}
