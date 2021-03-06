﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UADAPI
{

    /// <summary>
    /// Tell the program how to search anime
    /// </summary>
    public interface IQueryAnimeSeries
    {
        /// <summary>
        /// The host name for example http://google.com/
        /// </summary>
        string RemoteHost { get; set; }

        /// <summary>
        /// This will define do this mod support gt popular anime. If false, <see cref="GetFeaturedAnime"/> will not be called 
        /// </summary>
        bool SupportGetPopularSeries { get; set; }

        /// <summary>
        /// Your modification such as Name, Version, Description,...
        /// </summary>
        ModificatorInformation ModInfo { get; set; }

        ModificatorInformation RelativeManagerInfo { get; set; }

        /// <summary>
        /// Tell the program how to get anime series.
        /// </summary>
        /// <param name="offset">The offset of this anime at the remote host</param>
        /// <param name="count">How many anime series will be requested</param>
        /// <param name="animeGenres">Optional filter: Search anime satisfy the requested genres</param>
        /// <param name="season">Optional filter: Search anime in the requested season</param>
        /// <returns>Search result: Contain list of requested anime series information</returns>
        Task<List<AnimeSeriesInfo>> GetAnime(int offset, int count, string search = null, string animeGenres = null, string season = null);

        /// <summary>
        /// Get the anime that popular at the moment
        /// </summary>
        /// <param name="offset">The offset of this anime at the remote host</param>
        /// <param name="count">How many anime series will be requested</param>
        /// <returns></returns>
        Task<List<AnimeSeriesInfo>> GetFeaturedAnime(int offset, int count);

        /// <summary>
        /// Get all the anime genres define at the remote host
        /// </summary>
        /// <returns>A list of string contain all the genres</returns>
        Task<List<GenreItem>> GetAnimeGenres();

        /// <summary>
        /// Get all the anime season defined at the remote host
        /// </summary>
        /// <returns>A list of anime seasons</returns>
        Task<List<SeasonItem>> GetAnimeSeasons();

        /// <summary>
        /// Get the anime with the specified AnimeID
        /// </summary>
        /// <param name="id">the id of the anime</param>
        /// <returns></returns>
        Task<AnimeSeriesInfo> GetAnimeByID(int id);


        /// <summary>
        /// Get the search limit that <c>IQueryAnimeSeries.GetAnimes()</c> can satisfy
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="isSeason"></param>
        /// <returns></returns>
        Task<int> GetSearchLimit(string arg, bool isSeason);
    }

    public interface IAnimeSeriesManager
    {
        /// <summary>
        /// The Anime Series to work with
        /// </summary>
        AnimeSeriesInfo AttachedAnimeSeriesInfo { get; set; }

        ModificatorInformation ModInfo { get; set; }

        ModificatorInformation RelativeQueryInfo { get; set; }

        /// <summary>
        /// Back-stored for GetQualities()
        /// </summary>
        List<string> Qualities { get; set; }

        /// <summary>
        /// Get the episode information with only 1 request. If the remote host don't implement this. return value of GetEpisodes()
        /// <para>API Developer: Becareful not to overwrite the avaibleoffline property when re-requesting information. Because alot of Anime Control use to check for new episdes, download missing
        /// epsides,...
        /// </para>
        /// </summary>
        /// <param name="quality">For remote host don't implement this.</param>
        /// <returns></returns>
        Task<IEnumerable<EpisodeInfo>> GetPrototypeEpisodes();

        /// <summary>
        /// Tell the program how to get anime episodes
        /// <para>API Developer: Becareful not to overwrite the avaibleoffline property when re-requesting information. Because alot of Anime Control use to check for new episdes, download missing
        /// epsides,...
        /// </para>
        /// </summary>
        /// <param name="quality"></param>
        /// <param name="id">Specify with episodes to download, null: get all episode information</param>
        /// <returns></returns>
        Task<IEnumerable<EpisodeInfo>> GetEpisodes(List<int> id = null);

        /// <summary>
        /// Get detail by episodes, Use in watch online
        /// </summary>
        /// <param name="id">The ID of this episode</param>
        /// <returns></returns>
        Task<EpisodeInfo> GetEpisodeByID(int id);

        /// <summary>
        /// Get all the quality of this anime series
        /// </summary>
        /// <returns></returns>
        Task<List<VideoQuality>> GetQualities();

        /// <summary>
        /// Get the stardrandize quaility for example "m480p" => "480p" or "something720xyzp" => "720p".
        /// </summary>
        /// <returns> Object represent the source with the specified quality. Return null if no filmSources is suitable</returns>
        Task<MediaSourceInfo> GetCommonQuality(Dictionary<VideoQuality, MediaSourceInfo> filmSources, string requestedQuality);
    }


}
