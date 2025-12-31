using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFood.OrderHub.Application.InfraServices
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string username);
        string GenerateTokenV2(Guid subjectId, string role, string scope, string[] amr, string audience, int expiresMinutes);
    }
}
