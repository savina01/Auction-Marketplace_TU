using Auction_Marketplace.Data.Entities;
using Auction_Marketplace.Data.Models.Review;
using Auction_Marketplace.Services.Implementation;
using Auction_Marketplace.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Permissions;

namespace Auction_Marketplace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromForm] NewReviewModel review)
        {
            try
            {
                var response = await _reviewService.AddReview(review);
                return response.Succeed == true ? Ok(response) : BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.Message}");
            }
        }
    }
}
