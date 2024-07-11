using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Data.Repositories.Implementations
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddReview(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Review>> GetReviewsByAuctionId(int auctionId)
        {
            return await _context.Reviews
                .Where(b => b.AuctionId == auctionId)
                .ToListAsync();
        }

        public async Task DeleteReview(int reviewId)
        {
            var review = FindReviewById(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(await review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Review> Find(int reviewId)
        {
            return await _context.Reviews.Where(a => a.ReviewId == reviewId).FirstOrDefaultAsync();
        }

        public async Task<Review?> FindReviewById(int reviewId)
        {
            return await this._context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task UpdateReview(Review exsistingReview)
        {
            _context.Entry(exsistingReview).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}