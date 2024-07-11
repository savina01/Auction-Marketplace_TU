import UserDTO from "./UserDTO";

interface ReviewDTO {
    auctionId: number;
    userId: number;
    user: UserDTO;
    comment: string;
    rating: any;
  }

  export default ReviewDTO;