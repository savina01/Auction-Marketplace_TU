using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Models.Review;
using Auction_Marketplace.Services.Abstract;
using Auction_Marketplace.Data.Models;

namespace Auction_Marketplace.Services.Interface
{
    public interface IReviewService : IService
    {
        Task<Response<Review>> AddReview(NewReviewModel review);
        Task<Response<List<Review>>> GetReviewsForAuction(int auctionId);
        Task<Response<Review>> UpdateReview(int reviewId, ReviewViewModel review);
        Task<Response<string>> DeleteReview(int reviewId);
        Task<double> GetAverageRatingForAuction(int auctionId);

    }
}
