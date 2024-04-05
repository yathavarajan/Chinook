using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services;

public class ArtistService(IDbContextFactory<ChinookContext> dbFactory) : IArtistService
{
    public async Task<Artist> GetArtistByIdAsync(long artistId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();
        return await dbContext.Artists.SingleOrDefaultAsync(a => a.ArtistId == artistId);
    }

    public async Task<List<PlaylistTrack>> GetTracksByArtistIdAsync(long artistId, string userId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();
        var tracks = await dbContext.Tracks
            .Where(a => a.Album.ArtistId == artistId)
            .Include(a => a.Album)
            .Include(a => a.Playlists).ThenInclude(a => a.UserPlaylists)
            .Select(t => new PlaylistTrack
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Any(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks"))
            })
            .ToListAsync();
        return tracks;
    }
}
