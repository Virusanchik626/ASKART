using System.Text.RegularExpressions;

namespace Askart.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Number { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Email { get; set; }
        public byte[] Avatar { get; set; }

        public string FullName => $"{Surname} {Name} {Lastname}".Trim();

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch { return false; }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var regex = new Regex(@"^\+\d{10,15}$");
            return regex.IsMatch(phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
        }
    }
}