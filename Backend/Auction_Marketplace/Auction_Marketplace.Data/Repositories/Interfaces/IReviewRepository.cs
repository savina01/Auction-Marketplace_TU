using Auction_Marketplace.Data.Entities;

namespace Auction_Marketplace.Data.Repositories.Interfaces
{
	public interface IReviewRepository : IRepository<Review>
	{
        public Task AddReview(Review review);
        public Task DeleteReview(int reviewId);
        public Task<Review> Find(int reviewId);
        public Task UpdateReview(Review exsistingReview);
        public Task<List<Review>> GetReviewsByAuctionId(int auctionId);

    }
}