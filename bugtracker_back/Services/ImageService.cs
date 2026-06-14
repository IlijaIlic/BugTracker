namespace bugtracker_back.Services
{
    public class ImageService
    {
        private readonly string _uploadPath;
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
            _uploadPath = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fileName}";
        }

        public void DeleteImage(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}