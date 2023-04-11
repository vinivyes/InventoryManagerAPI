using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

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

    }
}
