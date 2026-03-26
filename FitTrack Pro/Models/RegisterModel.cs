using System.ComponentModel.DataAnnotations;

namespace FitTrack_Pro.Models
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }

        public string FullName { get; set; } = string.Empty;
    }
}
