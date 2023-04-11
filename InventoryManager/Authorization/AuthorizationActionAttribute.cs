using Microsoft.AspNetCore.Authorization;
using System;

namespace InventoryManagerAPI.Authorization
{    
    /// <summary>
    /// Custom authorization attribute to specify required actions for authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeActionAttribute : AuthorizeAttribute, IAuthorizeData
    {
        public AuthorizeActionAttribute(string action) : base("ActionPolicy")
        {
            Action = action;
        }
        public string Action { get; }
    }
}
