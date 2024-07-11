using Auction_Marketplace.Services.Abstract;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Auction_Marketplace.Services.Interface
{
    public interface ICloudinaryService : IService
    {
        Task<UploadResult> UploadAsync(Stream stream, string fileName);

    }
}
