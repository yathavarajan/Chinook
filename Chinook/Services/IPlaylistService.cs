using Chinook.ClientModels;

namespace Chinook.Services;

public interface IPlaylistService
{
    Task<List<Playlist>> GetPlayListsAsync(string currentUserId);

    Task<Playlist> GetPlayListByIdAsync(long playlistId, string currentUserId);

    Task AddTrackToPlaylistAsync(long trackId, string playlistName, string userId);

    Task RemoveTrackFromPlaylistAsync(long trackId, string playlistName, string userId);
}
