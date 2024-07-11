using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Services.Interface;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Auction_Marketplace.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string subject, string toEmail, string username, string message)
        {
            var apiKey = _configuration["SendGridApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("savina287@gmail.com", "Auction Marketplace");
            var to = new EmailAddress(toEmail, username);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public async Task SendEmailToTheWinner(string subject, string toEmail, string username, string message)
        {
            var apiKey = _configuration["SendGridApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("savina287@gmail.com", "Auction Marketplace");
            var to = new EmailAddress(toEmail, username);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
        public async Task SendEmailToWinnerAndSecondHighestBidder(Auction auction, string winnerEmail, string secondHighestBidderEmail)
        {
            try
            {
                string auctionName = auction.Name;
                decimal winningBidAmount = auction.FinalPrice;

                await SendEmail("Auction Winner Notification", winnerEmail, "Bidder",
                    $"Dear {winnerEmail},\r\n\r\nCongratulations! You've won the auction for {auctionName} with the highest bid of {winningBidAmount} BGN.");

                await SendEmail("Auction Second Highest Bidder Notification", secondHighestBidderEmail, "Bidder",
                    $"Dear {secondHighestBidderEmail},\r\n\r\nUnfortunately, you were not the winner of the auction for {auctionName}. However, you were the second highest bidder with a bid of {winningBidAmount} BGN.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending emails to the winner and the second highest bidder: {ex.Message}");
                throw;
            }
        }
    }
}

