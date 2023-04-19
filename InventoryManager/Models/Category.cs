using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace InventoryManagerAPI.Models
{ 
    /// <summary>
    /// Class to represent a Category
    /// </summary>
    [Table("Category")]
    [Index(nameof(name), IsUnique = true)]
    public class Category
    {

        /// <summary>
        /// Id of this Category
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        /// <summary>
        /// Name of this Category
        /// </summary>
        [Required]
        public string name { get; set; }

        /// <summary>
        /// Is this Category Active
        /// </summary>
        [Required]
        public bool isActive { get; set; }

        /// <summary>
        /// A description of the Category
        /// </summary>
        public string description { get; set; }

    }
}
