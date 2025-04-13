namespace Online_Order_for_digital_printing_of_Photos.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Role { get; set; } // Optional for login
    }
}
