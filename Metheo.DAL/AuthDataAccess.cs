using System.Data;
using Autofac;
using Metheo.DTO;

namespace Metheo.DAL;

public interface IAuthDataAccess
{
    Task<UserLoginResult> GetUserByEmailAsync(string email);
}

public class AuthDataAccess : IAuthDataAccess
{
    private readonly IDbConnection _invitesConnection;
    private readonly IDapperWrapper _dapperWrapper;

    public AuthDataAccess(IComponentContext context, IDapperWrapper dapperWrapper)
    {
        _invitesConnection = context.ResolveNamed<IDbConnection>("InvitesConnection");
        _dapperWrapper = dapperWrapper;

        Console.WriteLine($"Connected to database: {_invitesConnection.Database}");
    }

    public async Task<UserLoginResult> GetUserByEmailAsync(string email)
    {
        var query = @"
                SELECT u.id, u.email, u.password, r.name AS role_name, 
                       COALESCE(STRING_AGG(p.name, ', '), '') AS permission_name
                FROM users u
                LEFT JOIN model_has_roles ur ON u.id = ur.model_id
                LEFT JOIN roles r ON ur.role_id = r.id
                LEFT JOIN role_has_permissions rp ON r.id = rp.role_id
                LEFT JOIN permissions p ON rp.permission_id = p.id
                WHERE u.email = @Email
                GROUP BY u.id, u.email, u.password, r.name;";

        var result = await _dapperWrapper.QueryAsync<UserLoginResult>(_invitesConnection, query, new { Email = email });
        Console.WriteLine(result.FirstOrDefault());

        return result.FirstOrDefault(); // Return the first matching user or null
    }
}