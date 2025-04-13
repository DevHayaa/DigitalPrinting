using System;
using System.Collections.Generic;

namespace Online_Order_for_digital_printing_of_Photos.Models;

public partial class Folder
{
    public int FolderId { get; set; }

    public int? OrderId { get; set; }

    public string? FolderName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }
}
