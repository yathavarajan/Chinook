﻿using Chinook.Models;

namespace Chinook.Services;

public interface IHomeService
{
    Task<List<Artist>> GetArtistsAsync();

    Task<List<Album>> GetAlbumsForArtistAsync(int artistId);
}
