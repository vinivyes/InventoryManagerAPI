using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Text.RegularExpressions;

namespace InventoryManagerAPI.Models
{ 
    /// <summary>
    /// Class to represent a Role
    /// </summary>
    [Table("Role")]
    [Index(nameof(name), IsUnique = true)]
    public class Role
    {

        /// <summary>
        /// Id of this Role
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        /// <summary>
        /// Name of this Role
        /// </summary>
        [Required]
        public string name { get; set; }

        /// <summary>
        /// Is this Role Active
        /// </summary>
        [Required]
        public bool isActive { get; set; }

        /// <summary>
        /// A list of users associated with this role
        /// </summary>
        public virtual ICollection<User> users { get; set; }

        /// <summary>
        /// List of actions that can be performed - can be overriden by notAllowedActions
        /// </summary>
        public string[] allowedActions { get; set; }

        /// <summary>
        /// List of actions that cannot be performed by this role - overrides allowedActions
        /// </summary>
        public string[] notAllowedActions { get; set; }

        /// <summary>
        /// Checks if the input string is valid based on the following conditions:
        /// 1. Ends with either 'read', 'write', 'delete', or '*'.
        /// 2. Has segments divided by '/' without having two consecutive '/' characters.
        /// 3. Uses only alphanumeric characters, '/', and '*' in the string.
        /// 4. Starts with '/' and if does not start with '/', is not '*'.
        /// If the input string meets all of these conditions, the method returns true. Otherwise, it returns false.
        /// </summary>
        /// <param name="action">The string to be validated.</param>
        /// <returns>Returns a boolean value indicating whether the input string is valid or not.</returns>
        public bool IsValidAction(string action)
        {
            // Check if string ends with either 'read', 'write', 'delete' or '*'
            if (!Regex.IsMatch(action, @"(/read|/write|/delete|\*)$"))
            {
                return false;
            }

            // Check if string has segments divided by '/' and no 2 segments character in a row such as '//'
            if (action.Split('/').Length == 0 || action.Contains("//"))
            {
                return false;
            }

            // Check if string only uses alphanumeric characters, '/' and '*'
            if (!Regex.IsMatch(action, @"^[\w/*]+$"))
            {
                return false;
            }

            // Check if string starts with '/' and is not simply '*'
            if (!action.StartsWith('/') && action != "*")
            {
                return false;
            }

            return true;
        }
    }
}
