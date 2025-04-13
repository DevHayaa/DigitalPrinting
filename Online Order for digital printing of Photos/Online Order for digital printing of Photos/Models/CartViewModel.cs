namespace Online_Order_for_digital_printing_of_Photos.Models;

public class CartViewModel
{
    public int PhotoId { get; set; }
    public string PhotoName { get; set; }
    public string FilePath { get; set; }
    public string Size { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
