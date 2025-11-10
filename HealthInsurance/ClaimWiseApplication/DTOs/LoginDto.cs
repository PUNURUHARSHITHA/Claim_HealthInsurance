using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.DTOs
{
    public class LoginDto
    {
        /// <summary>Username of the user</summary>
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }


        /// <summary>Password for authentication</summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } // Later, switch to plain password and hash it


    }

}
