using System.Linq;
using System.Threading.Tasks;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagerAPI.Authorization
{
    /// <summary>
    /// Custom authorization handler to check if the user has a role that allows the required action.
    /// </summary>
    public class ActionAuthorizationHandler : AuthorizationHandler<ActionAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InventoryContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the ActionAuthorizationHandler class.
        /// </summary>
        /// <param name="httpContextAccessor">Provides access to the current HttpContext.</param>
        /// <param name="dbContext">The database context for accessing role information.</param>
        public ActionAuthorizationHandler(IHttpContextAccessor httpContextAccessor, InventoryContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Handles the authorization requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The authorization requirement.</param>
        /// <returns>A task representing the authorization result.</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActionAuthorizationRequirement requirement)
        {
            // Get the action values from the custom attributes.
            var endpoint = _httpContextAccessor.HttpContext.GetEndpoint();
            var attributes = endpoint.Metadata.OfType<AuthorizeActionAttribute>();
            var actions = attributes.Select(a => ReplacePlaceholders(a.Action, _httpContextAccessor.HttpContext)).ToList();

            // Get user roles from claims
            var userRoles = context.User.Claims.Where(c => c.Type == "Role").Select(c => c.Value).ToList();

            // Retrieve role entities from the database
            var roles = await _dbContext.Roles
                                        .Where(r => userRoles.Contains(r.name) && r.isActive)
                                        .ToListAsync();

            // Iterate through each role associated with the user
            foreach (var role in roles)
            {
                var allowedActions = role.allowedActions;
                var notAllowedActions = role.notAllowedActions;

                // Iterate through each action required by the endpoint
                foreach (var action in actions)
                {
                    bool allowed = false;

                    // Check if the action is allowed based on the allowedActions patterns
                    foreach (var allowedPattern in allowedActions ?? new string[] { })
                    {
                        if (Utils.MatchesPattern(action, allowedPattern))
                        {
                            allowed = true;
                            break;
                        }
                    }

                    // Check if the action is denied based on the notAllowedActions patterns
                    // If the action is both allowed and denied, the denial takes precedence
                    foreach (var notAllowedPattern in notAllowedActions ?? new string[] { })
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
                        context.Succeed(requirement);
                        break;
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Replaces placeholders in the action string with their corresponding values from the route data.
        /// </summary>
        /// <param name="action">The action string containing placeholders.</param>
        /// <param name="httpContext">The current HttpContext.</param>
        /// <returns>A new action string with placeholders replaced with their actual values.</returns>
        private string ReplacePlaceholders(string action, HttpContext httpContext)
        {
            foreach (var routeValue in httpContext.Request.RouteValues)
            {
                action = action.Replace("{" + routeValue.Key + "}", routeValue.Value.ToString());
            }
            return action;
        }
    }
}
