// DTOs/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}