using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services;

public interface IArtistService
{
    Task<Artist> GetArtistByIdAsync(long artistId);

    Task<List<PlaylistTrack>> GetTracksByArtistIdAsync(long artistId, string userId);
}
