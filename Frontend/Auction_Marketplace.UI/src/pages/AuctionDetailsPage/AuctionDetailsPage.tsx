import React, { useEffect, useState } from "react";
import Navbar from "../../components/Navbar/Navbar";
import { useParams, Link } from "react-router-dom";
import { clearToken, getToken, isTokenExpired } from "../../utils/GoogleToken";
import ApiService from "../../Services/ApiService";
import AuctionService from "../../Services/AuctionService";
import BidService from "../../Services/BidService";
import ApiResponseDTO from "../../Interfaces/DTOs/ApiResponseDTO";
import "./AuctionDetailsPage.css";
import CountdownTimer from "../../components/CountdownTimer/CountdownTimer";
import ReviewDTO from "../../Interfaces/DTOs/ReviewDTO";
import StarRating from "../../components/StarRating/StarRating";
import NewReviewDTO from "../../Interfaces/DTOs/NewReviewDTO";
import ReviewService from "../../Services/ReviewService";
import StarRatingComponent from 'react-star-rating-component';
import Confetti from 'react-confetti';

const apiService = new ApiService();
const auctionService = new AuctionService(apiService);
const bidService = new BidService(apiService);
const reviewService = new ReviewService(apiService);

const AuctionDetailsPage: React.FC = () => {
  const { auctionId } = useParams<{ auctionId: string }>();
  const [auctionDetails, setAuctionDetails] = useState<any>(null);
  const [bidAmount, setBidAmount] = useState<number>();
  const [bidSuccess, setBidSuccess] = useState<boolean>(false);
  const [finalBid, setFinalBid] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [emailSent, setEmailSent] = useState<boolean>(false);
  const token = getToken();
  const startPrice = auctionDetails?.startPrice;
  const [reviews, setReviews] = useState<ReviewDTO[]>([]);
  const [formData, setFormData] = useState<NewReviewDTO>({
    comment: '',
    rating: "",
    auctionId: auctionId
  });
  const [showReviewForm, setShowReviewForm] = useState<boolean>(false);
  const [showConfetti, setShowConfetti] = useState<boolean>(true);
  const [averageRating, setAverageRating] = useState<number | null>(null);

  useEffect(() => {
    const fetchAuctionDetails = async () => {
      try {
        const response: ApiResponseDTO = await auctionService.getAuctionById(
          Number(auctionId)
        );
        const fetchedAuctionDetails = response.data;
        setAuctionDetails(fetchedAuctionDetails);

        const finalBidResponse: ApiResponseDTO =
          await auctionService.checkFinalBid(Number(auctionId));
        if (finalBidResponse.succeed) {
          setFinalBid(finalBidResponse.data);
          if (finalBidResponse.data && new Date(auctionDetails?.endDate) < new Date()) {
            setShowConfetti(true);
            if (emailSent == false) {
              handleSendEmailToWinner();
            }
            handleCheckWinnersPaid();
          }
       }

      } catch (error) {
        throw error;
      } finally {
        setIsLoading(false);
      }
    };

    const fetchAverageRating = async () => {
      try {
        const response = await auctionService.averageRating(Number(auctionId));
        setAverageRating(response);
      } catch (error) {
        console.error("An error occurred while fetching average rating:", error);
      }
    };

    const handleSendEmailToWinner = async () => {
      try {
        const emailResponse = await auctionService.sendEmailToTheWinner(Number(auctionId));
        if (emailResponse.succeed) {
          setEmailSent(true);
        } else {
          console.error(`Failed to send email to winner: ${emailResponse.message}`);
        }
      } catch (error) {
        console.error(`An error occurred while sending email to winner: ${error}`);
      }
    };

    const handleCheckWinnersPaid = async () =>{
      try {
        const isPaid = await auctionService.checkPaymentForAuctions();
      }
      catch (error) {
        console.error(`An error occurred while checking payment from winner: ${error}`);
      }
    }


    if (token) {
      fetchAuctionDetails();
      fetchReviews();
      fetchAverageRating();
    }
    if (isTokenExpired()) {
      clearToken();
    }
    return () => {
    };
  }, [auctionId, token, emailSent]);

  const fetchReviews = async () => {
    try {
      const response: ApiResponseDTO = await auctionService.getReviews(Number(auctionId));
      const fetchedReviews: ReviewDTO[] = response.data;
      if (response.succeed) {
        setReviews(fetchedReviews);
      }
    }
    catch (error) {
      throw error;
    }
  }

  const handleBidNowClick = async () => {
    try {
      const bidData = { auctionId: Number(auctionId), amount: Number(bidAmount) };
      const response: ApiResponseDTO = await bidService.placeBid(bidData);
      if (response.succeed) {
        setBidSuccess(true);
        setBidAmount(undefined);
        setError(null);
        const finalBidResponse: ApiResponseDTO = await auctionService.checkFinalBid(Number(auctionId));
        if (finalBidResponse.succeed) {
          setFinalBid(finalBidResponse.data);
        }
      } else {
        setError("Error: You can't submit a bid smaller than the current highest bid.");
      }
    } catch (error) {
      setError(`Error creating bid: ${error}`);
    }
  };

  const handleAddReview = async () => {
    try {
      const response = await reviewService.createReview(formData);
      if (response.succeed) {
        alert("Reload page.");
      } else {
        setError("Failed to add review. Please try again.");
      }
    } catch (error) {
      setError(`Error adding review: ${error}`);
    }
  };


  const isBidAmountValid =
    typeof bidAmount === "number" && bidAmount >= startPrice;

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <>
      <Navbar showAuthButtons={false} />
      <div className="auction-details-container">
        <div className="auction-content">
          <div className="auction-photo">
            <img src={auctionDetails?.photo} />
          </div>
          <div className="auction-details">
            <div className="header-auction-detail">
              <h3 className="head-auction-name">{auctionDetails?.name}</h3>
            </div>
            <p className="description">{auctionDetails?.description}</p>
            <p className="start-price">
              Start Price: {auctionDetails?.startPrice} BGN
            </p>
            <p className="time-left">
              Closes in <CountdownTimer endDate={new Date(auctionDetails?.endDate)} />
            </p>
            <div className="average-rating">
              {averageRating !== null ? (
                <StarRatingComponent
                  name="average-rating"
                  editing={false}
                  starCount={5}
                  value={averageRating}
                  renderStarIcon={() => <span>&#9733;</span>}
                />
              ) : (
                <p>Still no ratings yet</p>
              )}
            </div>
            {!auctionDetails ||
              !auctionDetails.endDate ||
              new Date(auctionDetails.endDate) > new Date() ? (
              <div className="bid-section">
                <input
                  className={`input-bid ${bidAmount && bidAmount <= auctionDetails?.startPrice ? "input-bid-invalid" : ""}`}
                  id="bidAmount"
                  value={bidAmount || ""}
                  onChange={(e) => setBidAmount(Number(e.target.value))}
                  placeholder={
                    bidAmount && bidAmount <= auctionDetails?.startPrice
                      ? "Bid must be higher than start price"
                      : ""
                  }
                  style={{
                    padding: "10px",
                    border: "1px solid #ccc",
                    borderRadius: "5px",
                    fontSize: "16px",
                    width: "100%",
                    marginBottom: "10px",
                  }}
                />

                <button
                  className="bid-button"
                  onClick={handleBidNowClick}
                  disabled={!isBidAmountValid}
                >
                  Place Bid
                </button>
              </div>
            ) : null}

            {error && (
              <div className="error-message">{error}</div>
            )}
            <div className="user-container">
              {finalBid && <p>{finalBid}</p>}
            </div>
          </div>
        </div>
      </div>
      <div className="reviews-container">
        {!auctionDetails ||
          !auctionDetails.endDate ||
          new Date(auctionDetails.endDate) > new Date() ? (
          <button className="add-review-button" onClick={() => setShowReviewForm(true)}>
            Add Review
          </button>
        ) : null}      
        {showReviewForm && (
          <form className="add-review-container" onSubmit={handleAddReview}>
            <label>Review</label>
            <input
              type="text"
              className="input-comment"
              value={formData.comment}
              onChange={(e) => setFormData({ ...formData, comment: e.target.value })}
              placeholder="Enter your review comment..."
            />
            <div className="new-star-rating">
              <StarRatingComponent
                name="rate"
                editing={true}
                starCount={5}
                value={formData.rating}
                onStarClick={(rating) => setFormData({ ...formData, rating: rating })}
                renderStarIcon={() => <span>&#9733;</span>}
              />
            </div>
            <button className="add-review-button" type="submit">Submit</button>
          </form>
        )}
        <h3 className="header-reviews">{reviews.length} reviews</h3>
        <ul>
          {reviews.map((review, index) => (
            <li key={index}>
              <p className="reviews-comment">{review.comment}</p>
              <p className="reviews-rating"><StarRating rating={review.rating} />{review.user.firstName}</p>
              <br></br>
            </li>
          ))}
        </ul>
      </div>
      {auctionDetails && new Date(auctionDetails.endDate) < new Date() && <Confetti />}
    </>
  );
};

export default AuctionDetailsPage;
