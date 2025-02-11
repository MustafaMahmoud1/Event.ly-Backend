﻿using GraduationProject.API.Services;
using GraduationProject.Data.Models;
using GraduationProject.BL.Dtos.SignDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GraduationProject.API.Controllers.Register_Login_controllers.Login
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public LoginController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        #region Login 
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new Response { Status = "Error", Message = "User not found!" });

            // Retrieve the user's plain-text password from the database
            string userPassword = user.Password;

            var Password = PasswordHasherService.HashPassword(model.Password);

            // Compare the provided password with the hashed password stored in the database
            if (userPassword == Password)
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    Status = "Success",
                    Message = "User Login successfully!" , 
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });

            }
            // Passwords don't match, return Unauthorized
            return Unauthorized();
        }
        #endregion



        #region Email Confirmation
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest(new Response { Status = "Error", Message = "User ID or token is missing." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new Response { Status = "Error", Message = "User not found." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok(new Response { Status = "Success", Message = "Email confirmed successfully!" });
            }
            else
            {
                return BadRequest(new Response { Status = "Error", Message = "Email confirmation failed!" });
            }
        }
        #endregion



    }
}




