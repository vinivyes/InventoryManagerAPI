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
        /// <summary>
        /// Initializes a new instance of the AuthorizeActionAttribute class with the specified action.
        /// </summary>
        /// <param name="action">The action required for authorization.</param>
        public AuthorizeActionAttribute(string action) : base()
        {
            Action = action;
        }

        /// <summary>
        /// Gets the required action for authorization.
        /// </summary>
        public string Action { get; }
    }
}
