﻿using Graduation_Project.BL;
using GraduationProject.Data.Models;
using GraduationProject_Api.TRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GraduationProject_Api;

    [Route("api/[controller]")]
    [ApiController]
    public class TUserController : ControllerBase
    {
   
      
            private readonly IConfiguration _configuration;
            //private readonly ITIUserRepository _userRepository;
           private readonly UserManager<User> _userManager;


        public TUserController(IConfiguration configuration , UserManager<User> userManager)
            {
                _configuration = configuration;
            _userManager = userManager;
            }

        // Your controller actions using _userRepository


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<TokenDTO>> Login(LoginDTO credentials)


        {
            //Using GetUserByNameAsync:to retrieve the user with the provided username from the database. 
            var user = await _userManager.FindByNameAsync(credentials.UserName);

            if (user == null)
            {
                return BadRequest("User not found");
            }


            //Using IsLockedOutAsync:checks if the user account is locked out due to too many failed login attempts.
            bool isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
            {
                return BadRequest("Try again");
            }


  


            bool isAuthenticated = await _userManager.CheckPasswordAsync(user, credentials.Password);
            if (!isAuthenticated)
            {
                // to count the AccessFailed if >3 "go to locked "
                await _userManager.AccessFailedAsync(user);
                return Unauthorized("Wrong Credentials");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                return Unauthorized("User has no role assigned");
            }

            // Add role claims
            //var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));



            var secretKey = _configuration.GetValue<string>("SecretKey")!;
            var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(secretKeyInBytes);
            var methodUsedInGeneratingToken = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var exp = DateTime.Now.AddMinutes(15);

            var jwt = new JwtSecurityToken(
               //claims: userClaims.Concat(roleClaims).ToList(),
                 claims: userClaims,
                notBefore: DateTime.Now,
                issuer: "backendApplication",
                audience: "weather",
                expires: exp,
                signingCredentials: methodUsedInGeneratingToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(jwt);

            return new TokenDTO
            {
                Token = tokenString,
                ExpiryDate = exp
            };
        }

    #region register 
    //[HttpPost]
    //[Route("register")]
    //public async Task<ActionResult>Register(UserRegistrationDto registrationDto)
    //{
    //    var user = new User
    //    {
    //        FirstName = registrationDto.FirstName,
    //        LastName = registrationDto.LastName,
    //        Email = registrationDto.Email
    //    };
    //    var creationresult = await _userManager.CreateAsync(user, registrationDto.Password);
    //    if (!creationresult.Succeeded)
    //    {
    //        return BadRequest(creationresult.Errors);
    //    }
    //    var claimList = new List<Claim> {
    //        new Claim(ClaimTypes.NameIdentifier , user.Id.ToString()),
    //        new Claim(ClaimTypes.Role , "Admin"),
    //    };
    //    var userClaims = await _userManager.GetClaimsAsync(user);

    //    await _userManager.AddClaimAsync(user, claimList);
    //    return NoContent();
    //}


    #endregion
    #region admin register

    [HttpPost]
    [Route("register/admin")]
    public async Task<ActionResult> AdminRegister(UserRegistrationDto registerDto)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return BadRequest("Password and Confirm Password do not match.");
        }
        var user = new User
        {

            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,

        };
        if (registerDto.Password == null)
        {
            return BadRequest("Password cannot be null.");
        }
        var creationResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!creationResult.Succeeded)
        {
            return BadRequest("Failed to create user: " + string.Join("; ", creationResult.Errors.Select(e => e.Description)));
        }
       

        var claimsList = new List<Claim>
           {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role, "Admin"),
    
           };
        await _userManager.AddClaimsAsync(user, claimsList);

        return Ok();
    }

    #endregion

    #region Host Register
    [HttpPost]
    [Route("register/host")]
    public async Task<ActionResult> HostRegister(UserRegistrationDto registerDto)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return BadRequest("Password and Confirm Password do not match.");
        }
        var user = new User
        {

            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,

        };

        if (registerDto.Password == null)
        {
            return BadRequest("Password cannot be null.");
        }

        var creationResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!creationResult.Succeeded)
        {
            return BadRequest("Failed to create user: " + string.Join("; ", creationResult.Errors.Select(e => e.Description)));
        }


        var claimsList = new List<Claim>
           {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role, "Host"),

           };
        await _userManager.AddClaimsAsync(user, claimsList);

        return Ok();
    }
    #endregion
    #region Client Region
    [HttpPost]
    [Route("register/client")]
    public async Task<ActionResult> ClientRegister(UserRegistrationDto registerDto)
    {
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return BadRequest("Password and Confirm Password do not match.");
        }
        var user = new User
        {

            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,

        };

        if (registerDto.Password == null)
        {
            return BadRequest("Password cannot be null.");
        }
        var creationResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!creationResult.Succeeded)
        {
            return BadRequest("Failed to create user: " + string.Join("; ", creationResult.Errors.Select(e => e.Description)));
        }


        var claimsList = new List<Claim>
           {
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Role, "Client"),

           };
        await _userManager.AddClaimsAsync(user, claimsList);

        return Ok();
    }
    #endregion

}



//#region admin login
//[HttpPost]
//[Route("Admins/login")]
////=> /api/users/static-login
//public async Task<ActionResult<TokenDto>> AdminLogin(LoginDto credentials)
//{
//    #region Username and Password verification

//    IdentityUser? user = await _userManager.FindByNameAsync(credentials.PhoneNumber);

//    if (user is null)
//    {
//        return NotFound("User not found");
//    }

//    bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, credentials.Password);
//    if (!isPasswordCorrect)
//    {
//        //return Unauthorized();
//        return Unauthorized("Invalid password");
//    }

//    #endregion

//    #region Generate Token

//    var claimsList = await _userManager.GetClaimsAsync(user);

//    var roleClaim = claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Role);

//    if (roleClaim.Value != "Admin")
//    {
//        return Unauthorized("You are not an Admin");
//    }

//    string secretKey = _configuration.GetValue<string>("SecretKey")!;
//    var algorithm = SecurityAlgorithms.HmacSha256Signature;

//    var keyInBytes = Encoding.ASCII.GetBytes(secretKey);
//    var key = new SymmetricSecurityKey(keyInBytes);
//    var signingCredentials = new SigningCredentials(key, algorithm);

//    var token = new JwtSecurityToken(
//        claims: claimsList,
//        signingCredentials: signingCredentials,
//        expires: DateTime.Now.AddMinutes(720));
//    var tokenHandler = new JwtSecurityTokenHandler();

//    return new TokenDto
//    {
//        Token = tokenHandler.WriteToken(token),
//    };

//    #endregion

//}
//#endregion

/*

public async Task<ActionResult<TokenDto>> AdminLogin(LoginDto credentials)
{
    #region Username and Password verification

    IdentityUser? user = await _userManager.FindByNameAsync(credentials.PhoneNumber);

    if (user is null)
    {
        return NotFound("User not found");
    }

    bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, credentials.Password);
    if (!isPasswordCorrect)
    {
        //return Unauthorized();
        return Unauthorized("Invalid password");
    }

    var claimsList = await _userManager.GetClaimsAsync(user);

    var roleClaim = claimsList.FirstOrDefault(c => c.Type == ClaimTypes.Role);

    if (roleClaim.Value != "Admin")
    {
        return Unauthorized("You are not an Admin");
    }

    string secretKey = _configuration.GetValue<string>("SecretKey")!;
    var algorithm = SecurityAlgorithms.HmacSha256Signature;

    var keyInBytes = Encoding.ASCII.GetBytes(secretKey);
    var key = new SymmetricSecurityKey(keyInBytes);
    var signingCredentials = new SigningCredentials(key, algorithm);

    var token = new JwtSecurityToken(
        claims: claimsList,
        signingCredentials: signingCredentials,
        expires: DateTime.Now.AddMinutes(720));
    var tokenHandler = new JwtSecurityTokenHandler();

    return new TokenDto
    {
        Token = tokenHandler.WriteToken(token),
    };





 */