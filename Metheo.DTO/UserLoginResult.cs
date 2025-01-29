namespace Metheo.DTO;

public class UserLoginResult
{
    public int id { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string role_name { get; set; }
    public string permission_name { get; set; }
}