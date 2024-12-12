using System.Security.Claims;
using System.Text.RegularExpressions;
using Blog.API.Data;
using Blog.API.Infrastracture;
using Blog.API.Middleware;
using Blog.API.Models.DB;
using Blog.API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Services
{
    public interface IAccountService
    {
        Task<TokenResponse> RegisterAsync(UserRegisterModel model);
        Task<TokenResponse> LoginAsync(LoginCredentials model);
        
        Task<ActionResult> Logout(string token, ClaimsPrincipal user);

        Task<UserDTO> GetProfile(ClaimsPrincipal user);
        
        Task<ActionResult> EditProfile(UserEditModel userEdit, ClaimsPrincipal user);
    }

    public class AccountService : IAccountService
    {
        private readonly BlogDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly ITokenBlackListService _tokenBlackListService;

        public AccountService(
            BlogDbContext context,
            IPasswordHasher passwordHasher, 
            IJwtProvider jwtProvider, 
            ITokenBlackListService tokenBlackListService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _tokenBlackListService = tokenBlackListService;
        }
        
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;
            
            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
            return passwordRegex.IsMatch(password);
        }
        
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }

        public async Task<TokenResponse> RegisterAsync(UserRegisterModel model)
        {
            //Проверка email
            if (!IsValidEmail(model.email))
            {
                throw new ValidationAccessException("Invalid email format");
            }
            
            //Проверка даты рождения
            if (model.birthDate < new DateTime(1900, 1, 1) || model.birthDate > DateTime.UtcNow)
            {
                throw new ValidationAccessException("Birth date must be between 1900 and the current date.");
            }
            
            //Проверка пароля: длина минимум 6, минимум одна строчная, одна заглавная буква и одна цифра
            if (!IsValidPassword(model.password))
            {
                throw new ValidationAccessException("Password must be at least 6 characters long, include at least one lowercase letter, one uppercase letter, and one digit.");
            }
            var hashedPassword = _passwordHasher.Generate(model.password);
            
            //Проверка существует ли аккаунты с таким же email и phoneNumber в бд
            var exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.email == model.email || u.phoneNumber == model.phoneNumber);

            if (exists)
                throw new ValidationAccessException("Email Or Phone Already Registered");

            var newUser = new User
            {
                fullName = model.fullName,
                birthDate = model.birthDate,
                createTime = DateTime.UtcNow,
                gender = model.gender,
                email = model.email,
                phoneNumber = model.phoneNumber,
                passwordHash = hashedPassword

            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return _jwtProvider.GenerateToken(newUser);
        }
        
        public async Task<TokenResponse> LoginAsync(LoginCredentials model)
        {
            //ПРоверка существует ли аккаунт с таким email
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.email == model.email);

            if (user == null)
            {
                throw new ValidationAccessException("Wrong Email Or Password");
            }

            //Проверяем верный ли пароль
            var result = _passwordHasher.Verify(model.password, user.passwordHash);

            if (!result)
            {
                throw new ValidationAccessException("Wrong Email Or Password");
            }
            else
            {
                return _jwtProvider.GenerateToken(user);
            }
        }
        
        public async Task<ActionResult> Logout(string token, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !Guid.TryParse(userId, out var parsedId))
            {
                throw new UnauthorizedAccessException();
            }
            
            //Добавляет токен в блэк лист
            if (!string.IsNullOrEmpty(token))
            { 
                await _tokenBlackListService.AddTokenToBlackList(token);
                return null;
            }
            else
            {
                throw new UnauthorizedAccessException(); 
            }
        }

        public async Task<UserDTO> GetProfile(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !Guid.TryParse(userId, out var parsedId))
            {
                throw new UnauthorizedAccessException();
            }

            var userdb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.id == parsedId);
            
            if (userdb == null)
            {
                throw new KeyNotFoundException("user not found"); 
            }

            return new UserDTO
            {
                id = userdb.id,
                createTime = userdb.createTime,
                fullName = userdb.fullName,
                birthDate = userdb.birthDate,
                gender = userdb.gender,
                email = userdb.email,
                phoneNumber = userdb.phoneNumber
            };
        }
        
        public async Task<ActionResult> EditProfile(UserEditModel userEdit, ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !Guid.TryParse(userId, out var parsedId))
            {
                throw new UnauthorizedAccessException(); 
            }

            var userdb = await _context.Users
                .FirstOrDefaultAsync(u => u.id == parsedId);
            
            if (userdb == null)
                throw new KeyNotFoundException("user not found"); 
            
            //Проверка email
            if (!IsValidEmail(userEdit.email))
            {
                throw new ValidationAccessException("Invalid email format");
            }
            
            //Проверка даты рождения
            if (userEdit.birthDate < new DateTime(1900, 1, 1) || userEdit.birthDate > DateTime.UtcNow)
            {
                throw new ValidationAccessException("Birth date must be between 1900 and the current date.");
            }
            
            var exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.email == userEdit.email || u.phoneNumber == userEdit.phoneNumber);

            if (exists && userEdit.email != userdb.email && userEdit.phoneNumber != userdb.phoneNumber)
            {
                throw new ValidationAccessException("this email or phone already exists");
            }

            userdb.email = userEdit.email;
            userdb.fullName = userEdit.fullName;
            userdb.birthDate = userEdit.birthDate;
            userdb.gender = userEdit.gender;
            userdb.phoneNumber = userEdit.phoneNumber;

            await _context.SaveChangesAsync();
            return null;
        }
        
    }
}