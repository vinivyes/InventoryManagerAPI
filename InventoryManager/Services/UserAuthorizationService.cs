using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Helpers;

namespace InventoryManagerAPI.Services
{
    public class UserAuthorizationService
    {
        private readonly InventoryContext _dbContext;

        public UserAuthorizationService(InventoryContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> UserHasActionPermissionAsync(int userId, string action)
        {
            var userRoles = await _dbContext.Users
                .Where(u => u.id == userId)
                .SelectMany(u => u.roles.Select(r => r.name))
                .ToListAsync();

            var roles = await _dbContext.Roles
                .Where(r => userRoles.Contains(r.name) && r.isActive)
                .ToListAsync();

            // Iterate through each role associated with the user
            foreach (var role in roles)
            {
                var allowedActions = role.allowedActions ?? new string[] { };
                var notAllowedActions = role.notAllowedActions ?? new string[] { };

                bool allowed = false;

                // Add a new allowed action based on the userId
                allowedActions = allowedActions.Concat(new string[] { $"/user/{userId}/*/read" }).ToArray();
                allowedActions = allowedActions.Concat(new string[] { $"/user/{userId}/read" }).ToArray();

                // Check if the action is allowed based on the allowedActions patterns
                foreach (var allowedPattern in allowedActions)
                {
                    if (Utils.MatchesPattern(action, allowedPattern))
                    {
                        allowed = true;
                        break;
                    }
                }

                // Check if the action is denied based on the notAllowedActions patterns
                // If the action is both allowed and denied, the denial takes precedence
                foreach (var notAllowedPattern in notAllowedActions)
                {
                    if (Utils.MatchesPattern(action, notAllowedPattern))
                    {
                        allowed = false;
                        break;
                    }
                }

                // If the action is allowed, mark the requirement as successful and exit the loop
                if (allowed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
