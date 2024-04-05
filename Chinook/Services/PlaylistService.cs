using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using Playlist = Chinook.ClientModels.Playlist;

namespace Chinook.Services;

public class PlaylistService(IDbContextFactory<ChinookContext> dbFactory) : IPlaylistService
{
    public async Task<List<Playlist>> GetPlayListsAsync(string currentUserId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();
        return dbContext.Playlists
             .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
             .Select(p => new Playlist()
             {
                 Name = p.Name,
                 Tracks = p.Tracks.Select(t => new PlaylistTrack()
                 {
                     AlbumTitle = t.Album.Title,
                     ArtistName = t.Album.Artist.Name,
                     TrackId = t.TrackId,
                     TrackName = t.Name,
                     IsFavorite = t.Playlists.Any(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "My favorite tracks"))
                 }).ToList()
             })
             .ToList();
    }

    public async Task<Playlist> GetPlayListByIdAsync(long playlistId, string currentUserId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();
        return dbContext.Playlists
             .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
             .Where(p => p.PlaylistId == playlistId)
             .Select(p => new Playlist()
             {
                 Name = p.Name,
                 Tracks = p.Tracks.Select(t => new PlaylistTrack()
                 {
                     AlbumTitle = t.Album.Title,
                     ArtistName = t.Album.Artist.Name,
                     TrackId = t.TrackId,
                     TrackName = t.Name,
                     IsFavorite = t.Playlists.Any(p => p.UserPlaylists.Any(up => up.UserId == currentUserId && up.Playlist.Name == "My favorite tracks"))
                 }).ToList()
             })
             .FirstOrDefault();
    }

    public async Task AddTrackToPlaylistAsync(long trackId, string playlistName, string userId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();

        var playlist = await dbContext.Playlists
            .Include(p => p.UserPlaylists)
            .Include(a => a.Tracks)
            .FirstOrDefaultAsync(p => p.Name == playlistName);

        if (playlist == null)
        {
            long maxPlaylistId = await dbContext.Playlists.MaxAsync(p => (long?)p.PlaylistId) ?? 0;

            long newPlaylistId = maxPlaylistId + 1;

            playlist = new Models.Playlist { Name = playlistName, PlaylistId = newPlaylistId };
            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();
        }
        var playlistId = playlist.PlaylistId;

        var track = await dbContext.Tracks.FindAsync(trackId);
        playlist.Tracks.Add(track);

        var userPlaylist = playlist.UserPlaylists?.FirstOrDefault(up => up.UserId == userId);
        if (userPlaylist == null)
        {
            userPlaylist = new UserPlaylist { UserId = userId, PlaylistId = playlistId };
            if (playlist.UserPlaylists == null)
            {
                playlist.UserPlaylists = [];
            }
            playlist.UserPlaylists.Add(userPlaylist);
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveTrackFromPlaylistAsync(long trackId, string playlistName, string userId)
    {
        using var dbContext = await dbFactory.CreateDbContextAsync();

        var track = await dbContext.Tracks.FindAsync(trackId);
        if (track == null) return;

        var playlist = await dbContext.Playlists
            .Include(p => p.UserPlaylists)
            .FirstOrDefaultAsync(p => p.Name == playlistName);

        if (playlist == null) return;

        var userPlaylist = playlist.UserPlaylists.FirstOrDefault(up => up.UserId == userId);
        if (userPlaylist == null) return;

        playlist.UserPlaylists.Remove(userPlaylist);
        playlist.Tracks.Remove(track);

        await dbContext.SaveChangesAsync();
    }
}
