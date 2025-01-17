namespace Metheo.Tools;

public interface IPasswordService
{
    bool VerifyPassword(string inputPassword, string hashedPassword);
}

public class PasswordService : IPasswordService
{
    public bool VerifyPassword(string inputPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
    }
}