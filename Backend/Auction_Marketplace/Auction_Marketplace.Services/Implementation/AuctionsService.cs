
using System.Security.Claims;
using Auction_Marketplace.Data;
using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Models;
using Auction_Marketplace.Data.Models.Auction;
using Auction_Marketplace.Data.Repositories.Interfaces;
using Auction_Marketplace.Services.Interface;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Auction_Marketplace.Services.Implementation
{
    public class AuctionsService : IAuctionsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IStripeService _stripeService;
        private readonly IServiceProvider _serviceProvider;

        public AuctionsService(ApplicationDbContext dbContext,
            IAuctionRepository auctionRepository,
            IHttpContextAccessor contextAccessor,
            IUserService userService, 
            IBidRepository bidRepository,
            IEmailService emailService,
            IStripeService stripeService,
            ICloudinaryService cloudinaryService,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _contextAccessor = contextAccessor;
            _userService = userService;
            _emailService = emailService;
            _stripeService = stripeService;
            _cloudinaryService = cloudinaryService;
            _serviceProvider = serviceProvider;

        }
        public async Task<Response<Auction>> CreateAuction(NewAuctionViewModel auction)
        {
            try
            {
                if(auction == null)
                {
                    return new Response<Auction>
                    {
                        Succeed = false,
                        Message = "Invalid auction data."
                    };
                }

                var email = _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                if (email == null)
                {
                    return new Response<Auction>
                    {
                        Succeed = false,
                        Message = "User is not existing."
                    };
                }

                var user = await _userService.GetByEmailAsync(email);

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(auction.Photo.FileName, auction.Photo.OpenReadStream())
                };
                var uploadResult = await _cloudinaryService.UploadAsync(auction.Photo.OpenReadStream(), auction.Photo.FileName);

                Auction newAuction = new Auction
                {
                    UserId = user.Id,
                    Name = auction.Name,
                    Description = auction.Description,
                    StartPrice = auction.StartPrice,
                    EndDate = DateTime.Now.AddDays(auction.ExistingDays),
                    IsCompleted = false,
                    Photo = uploadResult.Url.ToString()
                };

                if (newAuction == null || string.IsNullOrEmpty(newAuction.Name) || newAuction.UserId <= 0)
                {
                    return new Response<Auction>
                    {
                        Succeed = false,
                        Message = "Invalid auction data."
                    };
                }

                await _auctionRepository.AddAsync(newAuction);
                await _auctionRepository.SaveChangesAsync();

                return new Response<Auction>
                {
                    Succeed = true,
                    Data = newAuction
                };
            }
            catch (Exception ex)
            {
                return new Response<Auction>
                {
                    Succeed = false,
                    Message = "An error occurred while creating the auction. See logs for details."
                };
            }
        }

        public async Task<Response<string>> DeleteAuction(int auctionId)
        {
            try
            {
                Auction auction = await _auctionRepository.FindAuctionById(auctionId);

                if(auction != null)
                {
                    await _auctionRepository.DeleteAuction(auctionId);
                }

                return new Response<string>
                {
                    Succeed = true,
                    Message = $"Successfully deleted auction with Id: {auctionId}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<List<Auction>>> GetAllAuctions()
        {
            try
            {
                List<Auction> auctions = await _dbContext.Auctions.ToListAsync();
                return new Response<List<Auction>>
                {
                    Succeed = true,
                    Data = auctions
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all auctions: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<Auction>> GetAuctionById(int auctionId)
        {
            try
            {
                Auction auction = await _auctionRepository.FindAuctionById(auctionId);
                return new Response<Auction>
                {
                    Succeed = true,
                    Data = auction
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all auctions: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<Auction>> UpdateAuction(int auctionId, UpdateAuctionViewModel updatedAuction)
        {
            try
            {
                var existingAuction = await _auctionRepository.FindAuctionById(auctionId);

                if (existingAuction == null)
                {
                    return new Response<Auction>
                    {
                        Succeed = false,
                        Message = $"Auction with ID {auctionId} not found."
                    };
                }

                existingAuction.Name = updatedAuction.Name;
                existingAuction.Description = updatedAuction.Description;

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(updatedAuction.Photo.FileName, updatedAuction.Photo.OpenReadStream())
                };
                var uploadResult = await _cloudinaryService.UploadAsync(updatedAuction.Photo.OpenReadStream(), updatedAuction.Photo.FileName);

                existingAuction.Photo = uploadResult.Url.ToString();

                await _auctionRepository.UpdateAuction(existingAuction);

                return new Response<Auction>
                {
                    Succeed = true
                };
            }
            catch (Exception ex)
            {
                return new Response<Auction>
                {
                    Succeed = false,
                    Message = $"An error occurred while updating the auction. See logs for details: {ex.Message}"
                };
            }
        }

        public async Task<Response<string>> CheckFinalBid(int auctionId)
        {
            try
            {
                await Task.Delay(1000);

                Auction auction = await _auctionRepository.FindAuctionById(auctionId);

                if (auction == null)
                {
                    return new Response<string>
                    {
                        Succeed = false,
                        Message = "Auction is not found."
                    };
                }

                if (auction.EndDate <= DateTime.Now)
                {
                    auction.IsCompleted = true;
                }

                List<Bid> bids = await _bidRepository.GetBidsByAuctionId(auctionId);

                if (bids == null || bids.Count == 0)
                {
                    return new Response<string>
                    {
                        Succeed = false,
                        Message = "No bids found for this auction."
                    };
                }

                decimal highestBidAmount = bids.Max(b => b.Amount);
                auction.FinalPrice = highestBidAmount;

                Bid finalBid = bids.FirstOrDefault(b => b.Amount == highestBidAmount);

                if (finalBid == null)
                {
                    return new Response<string>
                    {
                        Succeed = false,
                        Message = "Non bid found."
                    };
                }

                User user = await _userService.GetUserById(finalBid.UserId);

                await _auctionRepository.UpdateAuction(auction);

                return new Response<string>
                {
                    Succeed = true,
                    Data = $"{user.Email} has final bid {finalBid.Amount} BGN"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Succeed = false,
                    Message = $"An error occurred while checking the finalbid: {ex.Message}"
                };
            }
        }

        public async Task<Response<string>> SendEmailToWlinner(int auctionId)
        {
            try
            {
                var auctionResponse = await GetAuctionById(auctionId);
                if (!auctionResponse.Succeed || auctionResponse.Data == null)
                {
                    return new Response<string>
                    {
                        Succeed = false,
                        Message = "Auction not found or invalid.",
                    };
                }

                var finalBidResponse = await CheckFinalBid(auctionId);
                if (!finalBidResponse.Succeed)
                {
                    return new Response<string>
                    {
                        Succeed = false,
                        Message = finalBidResponse.Message
                    };
                }

                var messageParts = finalBidResponse.Data.Split(" ");
                string winningUserEmail = messageParts[0];
                decimal winningBidAmount = decimal.Parse(messageParts[4]);

                string auctionName = auctionResponse.Data.Name;
                long amount = Convert.ToInt64(auctionResponse.Data.FinalPrice);
                var session = await _stripeService.CreateCheckoutSessionAuctions(amount, auctionId, winningUserEmail);
                string stripeLink = session.Url;

                await _emailService.SendEmail("Auction Winner Notification", winningUserEmail, "Bidder",
                    $"Dear {winningUserEmail},\r\n\r\nCongratulations! You've won the auction for {auctionName} with the highest bid of {winningBidAmount} BGN. " +
                    $"Proceed to make your payment: {stripeLink}");

                return new Response<string>
                {
                    Succeed = true,
                    Message = $"Email sent successfully to {winningUserEmail}"
                };
            }
            catch (Exception ex)
            {
                return new Response<string>
                {
                    Succeed = false,
                    Message = $"An error occurred while sending the email to the winner: {ex.Message}"
                };
            }
        }

        public async Task CheckAuctionsPaymentStatus()
        {
            try
            {
                DateTime sevenDaysAgo = DateTime.Now.AddDays(-7);
                var auctions = await _auctionRepository.GetAuctionsEndedWithinRange(sevenDaysAgo, DateTime.Now);

                foreach (var auction in auctions)
                {
                    var finalBidResponse = await CheckFinalBid(auction.AuctionId);
                    if (finalBidResponse == null)
                    {
                        Console.WriteLine($"Error checking final bid for auction {auction.AuctionId}: No response received");
                        continue;
                    }

                    if (!finalBidResponse.Succeed)
                    {
                        Console.WriteLine($"Failed to retrieve final bid for auction {auction.AuctionId}: {finalBidResponse.Message}");
                        continue;
                    }

                    var messageParts = finalBidResponse.Data.Split(" ");
                    if (messageParts.Length < 5)
                    {
                        Console.WriteLine($"Error parsing final bid response for auction {auction.AuctionId}");
                        continue;
                    }

                    string winningUserEmail = messageParts[0];
                    decimal winningBidAmount = decimal.Parse(messageParts[4]);

                    bool isWinnerPaid = await _stripeService.IsPaymentReceived(winningUserEmail, winningBidAmount);

                    if (!isWinnerPaid)
                    {
                        var secondHighestBidderResponse = await _bidRepository.GetSecondHighestBidder(auction.AuctionId);
                        if (secondHighestBidderResponse == null)
                        {
                            Console.WriteLine($"No second highest bidder found for auction {auction.AuctionId}");
                            continue; 
                        }

                        await _emailService.SendEmailToWinnerAndSecondHighestBidder(auction, winningUserEmail, secondHighestBidderResponse.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking auctions payment status: {ex.Message}");
                throw;
            }
        }


        public async Task<Response<List<Auction>>> GetAllAuctionsUserBidded()
        {
           var email = _contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
           if (email == null)
           {
               return new Response<List<Auction>>
               {
                   Succeed = false,
                   Message = "Invalid auction data.",
               };
           }

           var user = await _userService.GetByEmailAsync(email);
           List<Bid> bids = await _bidRepository.GetBidsMadeByUser(user.Id);
           List<Auction> auctions = new List<Auction>();
           foreach (var bid in bids)
           {
               Auction auction = await _auctionRepository.FindAuctionById(bid.AuctionId);
               auctions.Add(auction);
           }

           auctions = auctions.DistinctBy(a => a.AuctionId).ToList();
           if (auctions.Count() == 0 || auctions == null)
           {
               return new Response<List<Auction>>
               {
                   Succeed = false,
                   Message = "Invalid auction data.",
               };
           }

           return new Response<List<Auction>>
           {
               Succeed = true,
               Data = auctions
           };
        }

        public async Task<bool> IsPaymentReceived(string userEmail, decimal amount)
        {
            return !string.IsNullOrEmpty(userEmail) && amount > 0;
        }

    }
}