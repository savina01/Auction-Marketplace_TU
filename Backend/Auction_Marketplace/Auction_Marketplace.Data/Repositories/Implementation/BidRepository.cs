using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Data.Repositories.Implementations
{
    public class BidRepository : BaseRepository<Bid>, IBidRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentRepository _paymentRepository;

        public BidRepository(ApplicationDbContext context, IPaymentRepository paymentRepository)
            : base(context)
        {
            _context = context;
            _paymentRepository = paymentRepository;
        }

        public async Task<Auction?> FindAuctionById(int auctionId)
        {
            return await this._context.Auctions.FirstOrDefaultAsync(c => c.AuctionId == auctionId);
        }

        public async Task<List<Bid>> GetBidsByAuctionId(int auctionId)
        {
            return await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .ToListAsync();
        }

        public async Task<List<Bid>> GetBidsMadeByUser(int userId)
        {
            return await _context.Bids
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<Bid?> GetWinningBid(int auctionId)
        {
            try
            {
                var winningBid = await _context.Bids
                    .Where(b => b.AuctionId == auctionId)
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefaultAsync();

                return winningBid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the winning bid: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetSecondHighestBidder(int auctionId)
        {
            try
            {
                var highestBidAmount = await _context.Bids
                    .Where(b => b.AuctionId == auctionId)
                    .MaxAsync(b => (decimal?)b.Amount) ?? 0;

                var secondHighestBidAmount = await _context.Bids
                    .Where(b => b.AuctionId == auctionId && b.Amount < highestBidAmount)
                    .MaxAsync(b => (decimal?)b.Amount) ?? 0;

                var secondHighestBidder = await _context.Bids
                    .Where(b => b.AuctionId == auctionId && b.Amount == secondHighestBidAmount)
                    .Select(b => b.User)
                    .FirstOrDefaultAsync();

                return secondHighestBidder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the second-highest bidder: {ex.Message}");
                throw;
            }
        }

    }
}