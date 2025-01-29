namespace Metheo.Tools;
using BCrypt.Net;

public interface IPasswordService
{
    bool VerifyPassword(string inputPassword, string hashedPassword);
}

public class PasswordService : IPasswordService
{
    public bool VerifyPassword(string inputPassword, string hashedPassword)
    {
        return BCrypt.Verify(inputPassword, hashedPassword);
    }
}