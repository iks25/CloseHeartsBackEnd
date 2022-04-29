using CloseHeartsAPI.DataAccess;
using CloseHeartsAPI.DTO;
using CloseHeartsAPI.Entities;
using CloseHeartsAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloseHeartsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataBaseContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataBaseContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await IsUserExist(registerDTO.UserName))
                return BadRequest($"user with name {registerDTO.UserName} exist");


            using var hmac = new HMACSHA512();

            var user = new AppUser()
            {
                UserName = registerDTO.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO
            {
                UserName = registerDTO.UserName,
                Token = _tokenService.CreateToken(user),
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName == loginDTO.UserName);

            if (user == null)
                return Unauthorized("Invalid User Name");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i = 0; i < computerHash.Length; i++)
            {
                if (computerHash[i] != user.PasswordHash[i])
                    return Unauthorized("bad Password");
            }
            return Ok(new UserDTO
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
            });

        }

        private async Task<bool> IsUserExist(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
        }


    }
}
