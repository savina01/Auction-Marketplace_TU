using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Services.Abstract;

namespace Auction_Marketplace.Services.Interface
{
	public interface IEmailService : IService
	{
		Task SendEmail(string subject, string toEmail, string username, string message);
		Task SendEmailToTheWinner(string subject, string toEmail, string username, string message);
		Task SendEmailToWinnerAndSecondHighestBidder(Auction auction, string winnerEmail, string secondHighestBidderEmail);

    }
}

