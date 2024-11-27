namespace Metheo.Api.Models;

// A class to hold the result of the query (user, roles, and permissions)
public class UserLoginResult
{
    public int id { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string role_name { get; set; }
    public string permission_name { get; set; }
}
