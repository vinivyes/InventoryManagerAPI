using Microsoft.AspNetCore.Authorization;

namespace InventoryManagerAPI.Authorization
{
    /// <summary>
    /// Custom authorization requirement for action-based authorization.
    /// </summary>
    public class ActionAuthorizationRequirement : IAuthorizationRequirement
    {
    }
}
