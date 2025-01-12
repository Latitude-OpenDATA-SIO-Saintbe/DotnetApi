using Metheo.DTO;
using Dapper;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace Metheo.DAL
{
    public interface IAuthDataAccess
    {
        Task<UserLoginResult> GetUserByEmailAsync(string email);
    }

    public class AuthDataAccess : IAuthDataAccess
    {
        private readonly IDbConnection _connection;

        public AuthDataAccess(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<UserLoginResult> GetUserByEmailAsync(string email)
        {
            var query = @"
                SELECT u.id, u.email, u.password, r.name AS role_name, p.name AS permission_name
                FROM users u
                LEFT JOIN model_has_roles ur ON u.id = ur.model_id
                LEFT JOIN roles r ON ur.role_id = r.id
                LEFT JOIN role_has_permissions rp ON r.id = rp.role_id
                LEFT JOIN permissions p ON rp.permission_id = p.id
                WHERE u.email = @Email";

            var result = await _connection.QueryAsync<UserLoginResult>(query, new { Email = email });

            return result.FirstOrDefault();
        }
    }
}