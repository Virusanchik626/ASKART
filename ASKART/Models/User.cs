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

        
        public string FullName
        {
            get { return (Surname + " " + Name + " " + Lastname).Trim(); }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            string clean = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return Regex.IsMatch(clean, @"^\+\d{10,15}$");
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            return password.Length >= 8;
        }
    }
}
