using System.Threading.Tasks;
using Memester.Database;
using Memester.FileStorage;
using Memester.Models;
using Microsoft.EntityFrameworkCore;

namespace Memester.Services
{
    public class MemeService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly FileStorageService _fileStorageService;

        public MemeService(DatabaseContext databaseContext, FileStorageService fileStorageService)
        {
            _databaseContext = databaseContext;
            _fileStorageService = fileStorageService;
        }
        public async Task DeleteMeme(long memeId)
        {
            var meme = await _databaseContext.Memes.SingleAsync(m => m.Id == memeId);
            _databaseContext.Remove(meme);
            await _fileStorageService.Delete($"meme{meme.Id}.webm");
            await _fileStorageService.Delete($"meme{meme.Id}.jpeg");
            await _databaseContext.SaveChangesAsync();
        }
    }
}