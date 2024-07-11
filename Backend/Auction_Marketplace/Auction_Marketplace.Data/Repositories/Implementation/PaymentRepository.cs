using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Data.Repositories.Implementations
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddPayment(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetPaymentsByUserId(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentByUserIdAndAuctionId(int userId, int auctionId)
        {
            try
            {
                var paymentRecord = await _context.Payments
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.AuctionId == auctionId);

                return paymentRecord;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting the payment record: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CheckPaymentExistsForUserAndAuction(int userId, int auctionId)
        {
            try
            {
                // Query the database to check if a payment record exists for the specified user and auction
                var paymentRecordExists = await _context.Payments
                    .AnyAsync(p => p.UserId == userId && p.AuctionId == auctionId);

                return paymentRecordExists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking payment existence: {ex.Message}");
                throw;
            }
        }
    }
}