using Auction_Marketplace.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction_Marketplace.Data.Models.Review
{
    public class NewReviewModel
    {
        public int AuctionId { get; set; }
        public string? Comment { get; set; }
        public RatingStar Rating { get; set; }
    }
}
