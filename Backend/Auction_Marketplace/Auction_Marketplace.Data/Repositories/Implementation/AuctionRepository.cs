using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Data.Repositories.Implementations
{
    public class AuctionRepository : BaseRepository<Auction>, IAuctionRepository
    {
        private readonly IBidRepository _bidRepository;
        private readonly IPaymentRepository _paymentRepository;

        public AuctionRepository(ApplicationDbContext context, IBidRepository bidRepository, IPaymentRepository paymentRepository)
            : base(context)
        {
            _bidRepository = bidRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task DeleteAuction(int auctionId)
        {
            var auction = await FindAuctionById(auctionId);

            if (auction != null)
            {
                _context.Auctions.Remove(auction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Auction> FindAuctionById(int auctionId)
        {
            return await _context.Auctions.Where(a => a.AuctionId == auctionId).FirstOrDefaultAsync();
        }

        public async Task UpdateAuction(Auction auction)
        {
            _context.Entry(auction).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Auction>> GetAuctionsEndedWithinRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var endedAuctions = await _context.Auctions
                    .Where(a => a.EndDate >= startDate && a.EndDate <= endDate && !a.IsCompleted)
                    .ToListAsync();

                foreach (var auction in endedAuctions)
                {
                    bool isWinnerPaid = await CheckWinnerPaymentStatus(auction);

                    if (!isWinnerPaid)
                    {
                        var secondHighestBidder = await FindSecondHighestBidder(auction);
                        if (secondHighestBidder != null)
                        {
                            auction.UserId = secondHighestBidder.UserId;
                            auction.IsCompleted = true;
                            _context.Auctions.Update(auction);
                        }
                        else
                        {
                            auction.IsCompleted = true;
                            _context.Auctions.Update(auction);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return endedAuctions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting auctions ended within range: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> CheckWinnerPaymentStatus(Auction auction)
        {
            var winnerBid = await _bidRepository.GetWinningBid(auction.AuctionId);
            if (winnerBid == null)
            {
                return false;
            }

            var paymentRecordExists = await _paymentRepository.CheckPaymentExistsForUserAndAuction(winnerBid.UserId, auction.AuctionId);

            return paymentRecordExists;
        }
        public async Task<Bid> FindSecondHighestBidder(Auction auction)
        {
            var bids = await _bidRepository.GetBidsByAuctionId(auction.AuctionId);

            var sortedBids = bids.OrderByDescending(b => b.Amount).ToList();

            if (sortedBids.Count < 2)
            {
                return null;
            }
            return sortedBids[1];
        }
    }
}