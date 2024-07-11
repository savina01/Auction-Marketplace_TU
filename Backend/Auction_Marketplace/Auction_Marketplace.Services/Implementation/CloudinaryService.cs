using Auction_Marketplace.Services.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Auction_Marketplace.Services.Implementation
{
    internal class CloudinaryService : ICloudinaryService 
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            Account account = new Account(
                "dtww7oous",
                "131455296139648",
                "XqtRNo_dcJ49OiNjA3sFbYvkzrs"
            );

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<UploadResult> UploadAsync(Stream stream, string fileName)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream)
            };

            return await _cloudinary.UploadAsync(uploadParams);
        }
    }
}
