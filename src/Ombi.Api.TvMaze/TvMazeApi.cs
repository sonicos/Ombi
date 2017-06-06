﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.TvMaze.Models;
using Ombi.Helpers;

namespace Ombi.Api.TvMaze
{
    public class TvMazeApi : ITvMazeApi
    {
        public TvMazeApi(ILogger<TvMazeApi> logger)
        {
            Api = new Ombi.Api.Api();
            Logger = logger;
            //Mapper = mapper;
        }
        private string Uri = "http://api.tvmaze.com";
        private Api Api { get; }
        private ILogger<TvMazeApi> Logger { get; }

        public async Task<List<TvMazeSearch>> Search(string searchTerm)
        {
            var request = new Request("search/shows", Uri, HttpMethod.Get);

            request.AddQueryString("q", searchTerm);
            request.ContentHeaders.Add(new KeyValuePair<string, string>("Content-Type","application/json"));

            return await Api.Request<List<TvMazeSearch>>(request);
        }

        public async Task<TvMazeShow> ShowLookup(int showId)
        {
            var request = new Request($"shows/{showId}", Uri, HttpMethod.Get);
            request.AddContentHeader("Content-Type", "application/json");

            return await Api.Request<TvMazeShow>(request);
        }

        public async Task<IEnumerable<TvMazeEpisodes>> EpisodeLookup(int showId)
        {

            var request = new Request($"shows/{showId}/episodes", Uri, HttpMethod.Get);

            request.AddContentHeader("Content-Type", "application/json");

            return await Api.Request<List<TvMazeEpisodes>>(request);
        }

        public async Task<TvMazeShow> ShowLookupByTheTvDbId(int theTvDbId)
        {
            var request = new Request($"lookup/shows?thetvdb={theTvDbId}", Uri, HttpMethod.Get);
            request.AddContentHeader("Content-Type", "application/json");
            try
            {
                var obj = await Api.Request<TvMazeShow>(request);

                var episodes = await EpisodeLookup(obj.id);

                foreach (var e in episodes)
                {
                    // Check if the season exists
                    var currentSeason = obj.Season.FirstOrDefault(x => x.SeasonNumber == e.season);

                    if (currentSeason == null)
                    {
                        // Create the season
                        obj.Season.Add(new TvMazeCustomSeason
                        {
                            SeasonNumber = e.season,
                            EpisodeNumber = new List<int> {e.number}
                        });
                    }
                    else
                    {
                        // Just add a new episode into that season
                        currentSeason.EpisodeNumber.Add(e.number);
                    }


                }

                return obj;
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.ApiException, e, "Exception when calling ShowLookupByTheTvDbId with id:{0}",theTvDbId);
                return null;
            }
        }

        public async Task<List<TvMazeSeasons>> GetSeasons(int id)
        {
            var request = new Request($"shows/{id}/seasons", Uri, HttpMethod.Get);

            request.AddContentHeader("Content-Type", "application/json");

            return await Api.Request<List<TvMazeSeasons>>(request);
        }

    }
}