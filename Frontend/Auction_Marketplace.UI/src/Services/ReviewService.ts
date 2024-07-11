import NewReviewDTO from "../Interfaces/DTOs/NewReviewDTO";
import ApiService from "./ApiService";
import ApiResponseDTO from '../Interfaces/DTOs/ApiResponseDTO';

class ReviewService {
    private CREATE_REVIEW_ENDPOINT = import.meta.env.VITE_CREATE_REVIEW_ENDPOINT;

    private apiService: ApiService;

    constructor(apiService: ApiService) {
        this.apiService = apiService;
    }

    async createReview(data: NewReviewDTO): Promise<ApiResponseDTO> {
        const formData = new FormData();
        formData.append('auctionId', String(data.auctionId));
        formData.append('comment', data.comment);
        formData.append('rating', data.rating);

        return this.apiService.post<ApiResponseDTO>(this.CREATE_REVIEW_ENDPOINT, formData);
    }
}
export default ReviewService;