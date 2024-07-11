using Auction_Marketplace.Data.Repositories.Interfaces;
using Auction_Marketplace.Services.Constants;
using Auction_Marketplace.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Auction_Marketplace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        private async Task<string> WriteFile(IFormFile file)
        {
            string fileName = "";

            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            fileName = DateTime.Now.Ticks.ToString() + extension;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            var exactPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
            using (var stream = new FileStream(exactPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        [HttpPost]
        [Route("UploadFile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var result = await WriteFile(file);
                return Ok();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("{document}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
            var provider = new FileExtensionContentTypeProvider();
            if(!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);  
            return File(bytes, contentType, Path.GetFileName(filePath));
        }

        [HttpGet("GetFileList")]
        public IActionResult GetFileList()
        {
            var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "Files"))
                                 .Select(Path.GetFileName)
                                 .ToList();
            return Ok(files);
        }
    }
}

