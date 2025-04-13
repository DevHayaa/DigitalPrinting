using System;
using System.Collections.Generic;

namespace Online_Order_for_digital_printing_of_Photos.Models;

public partial class Photo
{
    public int PhotoId { get; set; }

    public int? UserId { get; set; }

    public string PhotoName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
