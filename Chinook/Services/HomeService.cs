using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class HomeService(IDbContextFactory<ChinookContext> dbFactory) : IHomeService
{
    public async Task<List<Artist>> GetArtistsAsync()
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        return dbContext.Artists.Include(a => a.Albums).ToList();
    }

    public async Task<List<Album>> GetAlbumsForArtistAsync(int artistId)
    {
        var dbContext = await dbFactory.CreateDbContextAsync();
        return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
    }
}
