using System;
using System.Collections.Generic;

namespace Online_Order_for_digital_printing_of_Photos.Models;

public partial class PrintSize
{
    public int PrintSizeId { get; set; }

    public string Size { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
