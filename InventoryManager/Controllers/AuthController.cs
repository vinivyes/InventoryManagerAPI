using InventoryManagerAPI.Context;
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;
using InventoryManagerAPI.Helpers;
using InventoryManagerAPI.Authorization;
using System.Security.Claims;
using InventoryManagerAPI.Services;
using System.IdentityModel.Tokens.Jwt;

namespace InventoryManagerAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        //Instantiate configuration service
        private IConfiguration _config;
        private readonly InventoryContext _context;
        public AuthController(IConfiguration config, InventoryContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public ActionResult<object> Login(UserLogin login)
        {
            try
            {

                User user = _context.Users
                                    .Include(u => u.roles)
                                    .Select(u => new User()
                                    {
                                        id = u.id,
                                        email = u.email,
                                        first_name = u.first_name,
                                        last_name = u.last_name,
                                        password = u.password,
                                        roles = u.roles
                                    })
                                    .SingleOrDefault((u) => u.email == login.email);

                //Email not found
                if (user is null)
                    return Unauthorized(new
                    {
                        code = "E100001",
                        message = "E-mail is not found."
                    });

                //Password is incorrect
                if (!BCrypt.Net.BCrypt.Verify(login.password, user.password))
                    return Unauthorized(new
                    {
                        code = "E100002",
                        message = "Password is incorrect."
                    });

                //Create Claims list
                List<Claim> claims = new List<Claim>
                {
                    new Claim("userId", user.id.ToString()),
                    new Claim("email", user.email),
                    new Claim("first_name", user.first_name)
                };

                // Add roles as multiple claims if user has any roles
                foreach (Role role in user.roles ?? new List<Role>())
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.name));
                }

                //Create Token
                var token = JwtHelper.GetJwtToken(
                        user.email,
                        _config["Jwt:Key"],
                        _config["Jwt:Issuer"],
                        _config["Jwt:Audience"],
                        TimeSpan.FromHours(24),
                        claims.ToArray()
                    );

                return new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expires = token.ValidTo
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100000",
                    message = Utils.Log(ex)
                });
            }
        }
    }
}
