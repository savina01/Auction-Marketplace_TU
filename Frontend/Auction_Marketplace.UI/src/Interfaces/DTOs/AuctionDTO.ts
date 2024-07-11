interface AuctionDTO {
    auctionId: number;
    userId: number;
    user: any;
    name: string;
    photo: File | null;
    description: string;
    endDate: any;
    isCompleted: boolean;
    createdAt: string;
    updatedAt: string;
    deletedOn: string | null;
}

export default AuctionDTO;