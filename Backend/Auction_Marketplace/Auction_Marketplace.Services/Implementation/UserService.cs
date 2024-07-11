using System.Security.Claims;
using Amazon.S3;
using Amazon.S3.Model;
using Auction_Marketplace.Data;
using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Models;
using Auction_Marketplace.Data.Models.User;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Auction_Marketplace.Services.Constants;
using Auction_Marketplace.Services.Interface;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Services.Implementation
{
    public class UserService : IUserService
	{
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICloudinaryService _cloudinaryService;

        public UserService(IUserRepository userRepository,
            ApplicationDbContext dbContext,
            ICloudinaryService cloudinaryService)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<Response<UserViewModel>> GetUser()
        {
            try
            {
                var email = await _userRepository.GetUserByEmail();
                var userEmail = await _userRepository.GetByEmailAsync(email);          

                UserViewModel user = new UserViewModel()
                {
                    UserId = userEmail.Id,
                    FirstName = userEmail.FirstName,
                    LastName = userEmail.LastName,
                    Email = userEmail.Email,
                    ProfilePicture = userEmail.ProfilePicture
                };

                return new Response<UserViewModel>
                {
                    Succeed = true,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }

        public async Task<Response<UpdateUserViewModel>> UpdateUserInfo(UpdateUserViewModel updatedUser)
        {
            try
            {
                var email = await _userRepository.GetUserByEmail();

                var existingUser = await _userRepository.GetByEmailAsync(email);
                if (existingUser is null)
                {
                    return new Response<UpdateUserViewModel>
                    {
                        Succeed = false,
                        Message = "No such user!"
                    };
                }

                existingUser.FirstName = updatedUser.FirstName ?? existingUser.FirstName;
                existingUser.LastName = updatedUser.LastName ?? existingUser.LastName;

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(updatedUser.ProfilePicture.FileName, updatedUser.ProfilePicture.OpenReadStream())
                };
                var uploadResult = await _cloudinaryService.UploadAsync(updatedUser.ProfilePicture.OpenReadStream(), updatedUser.ProfilePicture.FileName);

                existingUser.ProfilePicture = uploadResult.Url.ToString();

                await _userRepository.UpdateUserInfo(existingUser);

                return new Response<UpdateUserViewModel>
                {
                    Succeed = true,
                    Data = updatedUser
                };
            }
            catch (Exception ex)
            {
                return new Response<UpdateUserViewModel>
                {
                    Succeed = false,
                    Message = $"{ex.Message}"
                };
            }
        }

        public async Task<Response<List<User>>> GetAllUsers()
        {
            try
            {
                List<User> users = await _dbContext.Users.ToListAsync();
                return new Response<List<User>>
                {
                    Succeed = true,
                    Data = users
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all users: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserById(int userId)
        {
            try
            {
                User user = await _userRepository.GetUserById(userId);
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                throw;
            }
        }
    }
}

