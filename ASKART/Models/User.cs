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

        public string FullName => $"{Surname} {Name} {Lastname}";
    }
}