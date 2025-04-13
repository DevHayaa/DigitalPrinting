using System.ComponentModel.DataAnnotations;
namespace Online_Order_for_digital_printing_of_Photos.Models;


public class CheckoutViewModel
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Shipping address is required.")]
    public string ShippingAddress { get; set; }

    public string Notes { get; set; }

    public string HearAboutUs { get; set; }

    [Required(ErrorMessage = "Payment method is required.")]
    public string PaymentMethod { get; set; }

    public string CreditCardNumber { get; set; }

    public string CreditCardExpiry { get; set; }

    public string CreditCardCVV { get; set; }

    public List<CartItem> CartItems { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CartItem
{
    public string Photo { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal SubTotal => Quantity * Price;
}

