import ApiService from './ApiService';
import ApiResponseDTO from '../Interfaces/DTOs/ApiResponseDTO';
import CreateAuctionDTO from '../Interfaces/DTOs/CreateAuctionDTO';
import UpdateAuctionDTO from '../Interfaces/DTOs/UpdateAuctionDTO';

class AuctionService {
    private GET_AUCTIONS_ENDPOINT = import.meta.env.VITE_GET_AUCTIONS;
    private CREATE_AUCTION_ENDPOINT = import.meta.env.VITE_CREATE_AUCTION_ENDPOINT;
    private UPDATE_AUCTION_ENDPOINT = import.meta.env.VITE_UPDATE_AUCTION_ENDPOINT;
    private GET_AUCTION_BY_ID_ENDPOINT = import.meta.env.VITE_GET_AUCTION_BY_ID_ENDPOINT;
    private CHECK_FINAL_BID_ENDPOINT = import.meta.env.VITE_CHECK_FINAL_BID_ENDPOINT;
    private DELETE_AUCTION_BY_ID_ENDPOINT = import.meta.env.VITE_DELETE_AUCTION_BY_ID_ENDPOINT;
    private GET_AUCTIONS_BIDDED_ENDPOINT = import.meta.env.VITE_GET_AUCTIONS_BIDDED;
    private GET_REVIEWS_ENDPOINT = import.meta.env.VITE_GET_REVIEWS_ENDPOINT;
    private GET_AVERAGE_RATING_ENDPOINT = import.meta.env.VITE_AVERAGE_RATING_FOR_AUCTION_ENDPOINT;
    private SEND_EMAIL_TO_THE_WINNER_ENDPOINT = import.meta.env.VITE_SEND_EMAIL_TO_THE_WINNER_ENDPOINT;
    private CHECK_AUCTIONS_PAYMENT_STATUS_ENDPOINT = import.meta.env.VITE_CHECK_AUCTIONS_PAYMENT_STATUS_ENDPOINT;

    private apiService: ApiService;

    constructor(apiService: ApiService) {
        this.apiService = apiService;
    }

    async fetchAuctions(): Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(this.GET_AUCTIONS_ENDPOINT);
    }

    async createAuction(data: CreateAuctionDTO): Promise<ApiResponseDTO> {
        const formData = new FormData();
        formData.append('name', data.name);
        formData.append('description', data.description);
        formData.append('startPrice', String(data.startPrice))
        formData.append('existingDays', String(data.existingDays));
        formData.append('isCompleted', String(data.isCompleted));
        if (data.photo) {
            formData.append('photo', data.photo);
        }

        return this.apiService.post<ApiResponseDTO>(this.CREATE_AUCTION_ENDPOINT, formData);
    }

    async updateAuction(auctionId: number, data: UpdateAuctionDTO): Promise<ApiResponseDTO> {
        const formData = new FormData();
        formData.append('name', data.name);
        formData.append('description', data.description);
        if (data.photo) {
            formData.append('photo', data.photo);
        }

        return this.apiService.put<ApiResponseDTO>(`${this.UPDATE_AUCTION_ENDPOINT}${auctionId}`, formData);
    }

    async getAuctionById(auctionId: number): Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.GET_AUCTION_BY_ID_ENDPOINT}${auctionId}`);
    }

    async deleteAuction(auctionId: number): Promise<ApiResponseDTO> {
        return this.apiService.delete<ApiResponseDTO>(`${this.DELETE_AUCTION_BY_ID_ENDPOINT}${auctionId}`);
    }

    async checkFinalBid(auctionId: number) : Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.CHECK_FINAL_BID_ENDPOINT}/${auctionId}`);
    }

    async getAuctionsBidded(): Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(this.GET_AUCTIONS_BIDDED_ENDPOINT);
    }

    async getReviews(auctionId: number): Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.GET_REVIEWS_ENDPOINT}${auctionId}`);
    }

    async averageRating(auctionId: number): Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.GET_AVERAGE_RATING_ENDPOINT}${auctionId}`);
    }

    async sendEmailToTheWinner(auctionId: number) : Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.SEND_EMAIL_TO_THE_WINNER_ENDPOINT}${auctionId}`);
    }

    async checkPaymentForAuctions():  Promise<ApiResponseDTO> {
        return this.apiService.get<ApiResponseDTO>(`${this.CHECK_AUCTIONS_PAYMENT_STATUS_ENDPOINT}`);
    }
}

export default AuctionService;