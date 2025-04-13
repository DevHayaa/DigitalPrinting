using System;
using System.Collections.Generic;

namespace Online_Order_for_digital_printing_of_Photos.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int PhotoId { get; set; }

    public int PrintSizeId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Photo Photo { get; set; } = null!;

    public virtual PrintSize PrintSize { get; set; } = null!;
}
