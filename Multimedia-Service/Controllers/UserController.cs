using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultimediaService.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MultimediaService.Models;
using Microsoft.AspNetCore.Identity;

namespace MultimediaService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        public UserController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel registerModel)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerModel.Username))
            {
                return BadRequest("Tên người dùng đã tồn tại.");
            }

            if (registerModel.Password != registerModel.ConfirmPassword)
            { return BadRequest("Mật khẩu xác nhận không đúng!"); }

            var user = new User
            {
                Username = registerModel.Username,
                FullName = registerModel.FullName,
                PasswordHash = HashPassword(registerModel.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            //Login after Register
            AddCookie(user, _configuration);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginModel loginModel)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginModel.Username);
            if (dbUser == null || !VerifyPassword(loginModel.Password, dbUser.PasswordHash))
            {
                return Unauthorized("Thông tin không hợp lệ.");
            }

            AddCookie(dbUser, _configuration);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("loginAsync")]
        public async Task<IActionResult> LoginAsync(string userName, string password)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (dbUser == null || !VerifyPassword(password, dbUser.PasswordHash))
            {
                return Unauthorized("Thông tin không hợp lệ.");
            }

            string token = AddCookie(dbUser, _configuration);
            return Ok(token);
        }

        public string AddCookie(User user, IConfiguration configuration)
        {
            var token = GenerateJwtToken(user, configuration);
            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            });
            return token;
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
            {
                return BadRequest("Token không hợp lệ.");
            }
            /*
            var blacklistToken = new TokenBlacklist
            {
                Token = token,
                Expiration = jwtToken.ValidTo
            };

            _context.TokenBlacklists.Add(blacklistToken);
            await _context.SaveChangesAsync();
            */
            HttpContext.Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return NotFound("Không tìm thấy tài khoản!");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest("Người dùng không hợp lệ.");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản!");
            }

            return Ok(new { user.Id, user.Username, user.FullName });
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromForm] AccountModel registerModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerModel.Username);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            if (registerModel.Password != null && registerModel.ConfirmPassword != null)
            {
                if (registerModel.Password != registerModel.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu xác nhận không đúng!");
                }
                else
                {
                    if (!string.IsNullOrEmpty(registerModel.Password))
                    {
                        user.PasswordHash = HashPassword(registerModel.Password);
                    }
                }
            }

            user.FullName = registerModel.FullName;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Đăng nhập lại
            HttpContext.Response.Cookies.Delete("jwtToken");
            AddCookie(user, _configuration);

            return Ok("Cập nhật thông tin người dùng thành công.");
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateForm([FromForm] AccountModel registerModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerModel.Username);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            if (registerModel.Password != registerModel.ConfirmPassword)
            {
                return BadRequest("Mật khẩu xác nhận không đúng!");
            }

            user.FullName = registerModel.FullName;

            if (!string.IsNullOrEmpty(registerModel.Password))
            {
                user.PasswordHash = HashPassword(registerModel.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Đăng nhập lại
            HttpContext.Response.Cookies.Delete("jwtToken");
            AddCookie(user, _configuration);

            return Ok("Cập nhật thông tin người dùng thành công.");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }

        public string GenerateJwtToken(User user, IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(ClaimTypes.Name, user.FullName), // Thêm tên người dùng
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Định danh thông qua UserID
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetListUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userList = await _context.Users
                .Where(u => u.Id != int.Parse(userIdClaim))
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.CreatedAt
                })
                .ToListAsync();
            return Ok(userList);
        }
    }
}
