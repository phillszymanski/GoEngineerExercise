namespace StarshipAPI.Models
{
    public class User
    {
        public int Id {get;set;}
        public string Email {get;set;} = string.Empty;
        public string Username {get;set;} = string.Empty;
        public string PasswordHash {get;set;} = string.Empty;
        public DateTime Created {get;set;}
        public DateTime Edited {get;set;}
    }

    public class UserDto
    {
        public int Id {get;set;}
        public string Email {get;set;} = string.Empty;
        public string Username {get;set;} = string.Empty;
    }

    public class LoginRequest
    {
        public string Email {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email {get;set;} = string.Empty;
        public string Password {get;set;} = string.Empty;
        public string Username {get;set;} = string.Empty;
    }

    public class AuthResponse
    {
        public string Token {get;set;} = string.Empty;
        public UserDto? User {get;set;}
    }
}