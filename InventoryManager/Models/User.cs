using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Numerics;

namespace InventoryManagerAPI.Models
{
    [Table("User")]
    [Index(nameof(email), IsUnique = true)]
    public class User
    {

        /// <summary>
        /// Id of this User
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        /// <summary>
        /// First name of this User
        /// </summary>
        [Required]
        public string first_name { get; set; }

        /// <summary>
        /// Last name of this User
        /// </summary>
        [Required]
        public string last_name { get; set; }


        /// <summary>
        /// Email of this User
        /// </summary>
        [Required]
        public string email { get; set; }

        /// <summary>
        /// Hashed version of this password using BCrypt
        /// </summary>
        [Required]
        public string password { get; set; }

        /// <summary>
        /// When was the current password generated - used to invalidate old passwords
        /// </summary>
        public System.DateTime passwordDate { get; set; }

        /// <summary>
        /// A list of roles associated with this user
        /// </summary>
        public virtual ICollection<Role> roles { get; set; }

        /// <summary>
        /// Is the password valid.
        /// It should have a combination of:
        /// - At least 8 characters
        /// - At least 1 uppercase letter
        /// - At least 1 lowercase letter
        /// - At least 1 number
        /// Returns True/False if the password is valid
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < 8)
            {
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            return true;
        }
    }

    public class UserLogin
    {
        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}
