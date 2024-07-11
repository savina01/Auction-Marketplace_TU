using Microsoft.AspNetCore.Mvc;
using Auction_Marketplace.Services.Interface;
using Auction_Marketplace.Data.Models.Auction;
using Microsoft.AspNetCore.Authorization;
using MailKit;
using Auction_Marketplace.Services.Implementation;
using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Services.Jobs;

namespace Auction_Marketplace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionsService _auctionsService;
        private readonly IReviewService _reviewService;
        private readonly IEmailService _mailService;
        public AuctionController(IAuctionsService auctionsService, 
            IReviewService reviewService,
            IEmailService mailService)
        {
            _auctionsService = auctionsService;
            _reviewService = reviewService;
            _mailService = mailService;
        }

        [HttpGet]
        [Route("All")]
        [Authorize]
        public async Task<IActionResult> GetAllAuctions()
        {
            try
            {
                var response = await _auctionsService.GetAllAuctions();
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAuctionById([FromRoute] int id)
        {
            try
            {
                var response = await _auctionsService.GetAuctionById(id);
                if (response == null)
                {
                    return NotFound();
                }

                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAuction([FromForm] NewAuctionViewModel auction)
        {
            try
            {
                var response = await _auctionsService.CreateAuction(auction);
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAuction([FromRoute] int id, [FromForm] UpdateAuctionViewModel updatedAuction)
        {
            try
            {
                var response = await _auctionsService.UpdateAuction(id, updatedAuction);
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAuction([FromRoute] int id)
        {
            try
            {
                var response = await _auctionsService.DeleteAuction(id);
                return response.Succeed == true ? Ok(response.Message) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("check-final-bid/{auctionId}")]
        [Authorize]
        public async Task<IActionResult> CheckFinalBid([FromRoute] int auctionId)
        {
            try
            {
                var response = await _auctionsService.CheckFinalBid(auctionId);
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet]
        [Route("Bidded")]
        [Authorize]
        public async Task<IActionResult> GetAllAuctionsUserBidded()
        {
            try
            {
                var response = await _auctionsService.GetAllAuctionsUserBidded();
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("Reviews/{auctionId}")]
        [Authorize]
        public async Task<IActionResult> GetAllReviews([FromRoute]int auctionId)
        {
            try
            {
                var response = await _reviewService.GetReviewsForAuction(auctionId);
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }

        [HttpGet("AverageRating/{auctionId}")]
        public async Task<IActionResult> GetAverageRatingForAuction(int auctionId)
        {
            try
            {
                double averageRating = await _reviewService.GetAverageRatingForAuction(auctionId);
                return Ok(averageRating);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving average rating for auction: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("SendMailToTheWinner/{auctionId}")]
        [Authorize]
        public async Task<IActionResult> SendMail([FromRoute]int auctionId)
        {
            try
            {
                var response = await _auctionsService.SendEmailToWlinner(auctionId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving average rating for auction: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("CheckAuctionsPaymentStatus")]
        [Authorize]
        public async Task<IActionResult> CheckAuctionsPaymentStatus()
        {
            try
            {
                await _auctionsService.CheckAuctionsPaymentStatus();
                return Ok("Auctions payment status checked successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while checking auctions payment status: {ex.Message}");
            }
        }

    }
}

