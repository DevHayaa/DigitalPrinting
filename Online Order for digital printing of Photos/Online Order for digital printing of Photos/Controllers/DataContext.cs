using Microsoft.EntityFrameworkCore;
using Online_Order_for_digital_printing_of_Photos.Models;

namespace Online_Order_for_digital_printing_of_Photos.Controllers
{
    internal class DataContext
    {
        public object Users { get; internal set; }

        public DbSet<Photo> Photos { get; set; }

    }
}