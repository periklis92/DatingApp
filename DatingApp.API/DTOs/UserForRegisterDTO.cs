using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "You must specify a username between 4 - 20 characters!")]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify a password between 4 - 8 characters!")]
        public string Password { get; set; }
    }
}