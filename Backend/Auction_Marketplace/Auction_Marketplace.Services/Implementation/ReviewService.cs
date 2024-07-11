using Auction_Marketplace.Data;
using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Models;
using Auction_Marketplace.Data.Models.Review;
using Auction_Marketplace.Data.Repositories.Implementations;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Auction_Marketplace.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Auction_Marketplace.Services.Implementation
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserService _userService;
        private readonly IAuctionsService _auctionsService;
        private readonly IReviewRepository _reviewRepository;
        private readonly IAuctionRepository _auctionRepository;

        public ReviewService(ApplicationDbContext dbContext,
            IHttpContextAccessor contextAccessor,
            IUserService userService,
            IReviewRepository reviewRepository,
            IAuctionRepository auctionRepository,
            IAuctionsService auctionsService
            )
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _userService = userService;
            _reviewRepository = reviewRepository;
            _auctionRepository = auctionRepository;
            _auctionsService = auctionsService;
        }

        public async Task<Response<Review>> AddReview(NewReviewModel review)
        {
            try
            {
                if (review == null)
                {
                    return new Response<Review>
                    {
                        Succeed = false,
                        Message = "Invalid review data."
                    };
                }
                var email = _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                if (email == null)
                {
                    return new Response<Review>
                    {
                        Succeed = false,
                        Message = "User is not existing."
                    };
                }

                var user = await _userService.GetByEmailAsync(email);

                Review newReview = new Review
                {
                    UserId = user.Id,
                    Comment = review.Comment,
                    Rating = review.Rating,
                    AuctionId = review.AuctionId
                };

                if (newReview == null || string.IsNullOrEmpty(newReview.Comment) || newReview.UserId <=0)
                {
                    return new Response<Review>
                    {
                        Succeed = false,
                        Message = "Invalid review data."
                    };
                }

                await _reviewRepository.AddAsync(newReview);
                await _reviewRepository.SaveChangesAsync();

                return new Response<Review>
                {
                    Succeed = true,
                    Data = newReview
                };
            
            }
            catch (Exception ex)
            {
                return new Response<Review>
                {
                    Succeed = false,
                    Message = "An error occurred while creating the review. See logs for details."
                };
            }
        }

        async Task<Response<string>> IReviewService.DeleteReview(int reviewId)
        {
            try
            {
                var review = _dbContext.Reviews.Find(reviewId);
                if (review != null)
                {
                    await _reviewRepository.DeleteReview(reviewId);
                }

                return new Response<string>
                {
                    Succeed = true,
                    Message = $"Successfully deleted review with Id: {reviewId}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<List<Review>>> GetReviewsForAuction(int auctionId)
        {
            try
            {
                Auction auction = await _auctionRepository.FindAuctionById(auctionId);

                if (auction == null)
                {
                    return new Response<List<Review>>
                    {
                        Succeed = false,
                        Message = "Auction is not found."
                    };
                }

                List<Review> reviews = await _dbContext.Reviews
                    .Include(r => r.User)  
                    .Where(r => r.AuctionId == auctionId)
                    .ToListAsync();

                return new Response<List<Review>>
                {
                    Succeed = true,
                    Data = reviews
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all reviews: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<Review>> UpdateReview(int reviewId, ReviewViewModel review)
        {
            try
            {
                var existingReview = await _reviewRepository.Find(reviewId);

                if (existingReview == null)
                {
                    return new Response<Review>
                    {
                        Succeed = false,
                        Message = $"Review with Id {reviewId} not found."
                    };
                }

                existingReview.Comment = review.Comment;

                await _reviewRepository.UpdateReview(existingReview);

                return new Response<Review>
                {
                    Succeed = true
                };
            }
            catch(Exception ex)
            {
                return new Response<Review>
                {
                    Succeed = false,
                    Message = $"An error occurred while updating the review. See logs for details: {ex.Message}"
                };
            }
        }

        public async Task<double> GetAverageRatingForAuction(int auctionId)
        {
            try
            {
                List<Review> reviews = await _reviewRepository.GetReviewsByAuctionId(auctionId);
                if (reviews == null || reviews.Count == 0)
                {
                    return 0;
                }

                double totalRating = reviews.Sum(r => (double)r.Rating);
                double averageRating = totalRating / reviews.Count;

                return averageRating;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating average rating for auction: {ex.Message}");
                throw; 
            }
        }

    }
}
