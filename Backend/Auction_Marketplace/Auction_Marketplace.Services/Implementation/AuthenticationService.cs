using Microsoft.AspNetCore.Identity;
using Auction_Marketplace.Data.Models;
using Auction_Marketplace.Data.Models.Authentication;
using Auction_Marketplace.Data.Entities;
using Newtonsoft.Json;
using Auction_Marketplace.Data.Models.Google;
using Auction_Marketplace.Services.Interface;
using Microsoft.Extensions.Configuration;
using Auction_Marketplace.Services.Constants;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace Auction_Marketplace.Services.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly SignInManager<User> _signInManager;

        public AuthenticationService(IUserService userSevice,
                                        ITokenService tokenService,
                                        IEmailService emailService,
                                        UserManager<User> userManager,
                                        SignInManager<User> signInManager,
                                        IConfiguration configuration,
                                        ICloudinaryService cloudinaryService)
        {
            _userService = userSevice;
            _emailService = emailService;
            _tokenService = tokenService;
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<Response<string>> Register(RegisterViewModel registerUser)
        {
            var userExists = await _userService.GetByEmailAsync(registerUser.Email);

            if (userExists != null)
            {
                return new Response<string>()
                {
                    Succeed = false,
                    Message = "User with this Email Adress already exists!"
                };
            }

            User user = new User();
            var uploadResult = await _cloudinaryService.UploadAsync(registerUser.ProfilePicture.OpenReadStream(), registerUser.ProfilePicture.FileName);
            user.ProfilePicture = uploadResult.Url.ToString();
            var token = await RegisterUser(registerUser, user);

            return token != null ? new Response<string> { Succeed = true, Data = token } : new Response<string> { Succeed = false, Message = "Invalid Registration" };
        }

        public async Task<Response<string>> Login(LoginViewModel loginUser)
        {
            var user = await _userManager.FindByNameAsync(loginUser.Email);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginUser.Password, false);
                if (result.Succeeded)
                {
                    var jwtToken = _tokenService.GenerateJwtToken(user);
                    return new Response<string>()
                    {
                        Succeed = true,
                        Data = jwtToken
                    };
                }
            }
            return new Response<string>()
            {
                Succeed = false,
                Message = "Invalid User"
            };
        }
        public async Task<Response<string>> GoogleLoginAsync(GoogleLoginViewModel googleLogin)
        {
            var validation = await ValidateGoogleTokenAsync(googleLogin.GoogleToken);
            var email = validation.Email;
            var existingUser = await _userService.GetByEmailAsync(email);

            if (existingUser == null)
            {
                var registerUser = new RegisterViewModel
                {
                    FirstName = validation.FirstName,
                    LastName = validation.LastName,
                    Email = email,
                    Password = "Password123"
                };

                User user = new User();
                user.ProfilePicture = validation.ProfilePicture;

                var token = await RegisterUser(registerUser, user);
                return new Response<string> { Succeed = true, Data = token };
            }

            await _signInManager.SignInAsync(existingUser, false);
            var jwtToken = _tokenService.GenerateJwtToken(existingUser);
            return new Response<string> { Succeed = true, Data = jwtToken };
        }

        private async Task<string?> RegisterUser(RegisterViewModel registerUser, User user)
        {
            user.FirstName = registerUser.FirstName;
            user.LastName = registerUser.LastName;
            user.Email = registerUser.Email;
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UserName = registerUser.Email;
            
            var isCreated = await _userManager.CreateAsync(user, registerUser.Password);

            if (!isCreated.Succeeded)
            {
                return null;
            }

            await _userManager.AddToRoleAsync(user, "User");
            var token = _tokenService.GenerateJwtToken(user);
            await _emailService.SendEmail("Register Confirmation Email", registerUser.Email, $"{registerUser.FirstName} {registerUser.LastName}", $"Dear {registerUser.FirstName},\r\n\r\nWelcome to Auction Marketplace! We're delighted to have you on board. Your account has been successfully created.\r\n\r\nIf you have any questions or need assistance, kindly inform us.\r\n\r\nEnjoy exploring and making the most of our services!\r\n\r\nBest regards,\r\n\r\n CEO Savina");

            return token;
        }

        private async Task<GoogleTokenInfo?> ValidateGoogleTokenAsync(string googleToken)
        {
            using (var httpClient = new HttpClient())
            {
                var validationEndpoint = _configuration["ValidationEndpoint"] + googleToken;
                var response = await httpClient.GetAsync(validationEndpoint);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GoogleTokenInfo>(responseContent);
            }
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }
    }

}

