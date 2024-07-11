using Auction_Marketplace.Data.Entities;

namespace Auction_Marketplace.Data.Repositories.Interfaces
{
	public interface IAuctionRepository : IRepository<Auction>
	{
        public Task DeleteAuction(int auctionId);
        public Task<Auction> FindAuctionById(int auctionId);
        public Task UpdateAuction(Auction existingAuction);
        public Task<List<Auction>> GetAuctionsEndedWithinRange(DateTime startDate, DateTime endDate);
        public Task<bool> CheckWinnerPaymentStatus(Auction auction);
        public Task<Bid> FindSecondHighestBidder(Auction auction);

    }
}