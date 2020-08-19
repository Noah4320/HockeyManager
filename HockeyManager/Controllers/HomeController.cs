using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HockeyManager.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Newtonsoft.Json;
using HockeyManager.Data;

namespace HockeyManager.Controllers
{

    public class HomeController : Controller
    {
        private readonly HockeyContext _context;


        public HomeController(HockeyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> FetchApiData()
        {
            var TeamUrl = "https://statsapi.web.nhl.com/api/v1/teams";
            var httpClient = HttpClientFactory.Create();
            var teamData = await httpClient.GetStringAsync(TeamUrl);

            var teams = JsonConvert.DeserializeObject<TeamRoot>(teamData);

            List<HMTeam> hMTeams = new List<HMTeam>();
            List<HMPlayer> hMPlayers = new List<HMPlayer>();

            foreach (var team in teams.teams)
            {
                var teamStatsUrl = $"https://statsapi.web.nhl.com/api/v1/teams/{team.id}/stats";
                var teamStatsData = await httpClient.GetStringAsync(teamStatsUrl);
                var teamStats = JsonConvert.DeserializeObject<TeamStatRoot>(teamStatsData);

                hMTeams.Add(new HMTeam
                {
                    ApiId = team.id,
                    Name = team.name,
                    Conference = team.conference.name,
                    Division = team.division.name,
                    Abbreviation = team.abbreviation,
                    GamesPlayed = teamStats.stats[0].splits[0].stat.gamesPlayed,
                    Wins = Convert.ToInt32(teamStats.stats[0].splits[0].stat.wins),
                    Loses = Convert.ToInt32(teamStats.stats[0].splits[0].stat.losses),
                    OvertimeLoses = Convert.ToInt32(teamStats.stats[0].splits[0].stat.ot),
                    Points = Convert.ToInt32(teamStats.stats[0].splits[0].stat.pts),
                    logoUrl = $"https://www-league.nhlstatic.com/images/logos/teams-current-primary-light/{team.id}.svg"
                });

            }

            if (_context.Teams.Count() == 0)
            {
                await _context.Teams.AddRangeAsync(hMTeams);
                await _context.SaveChangesAsync();
            }

            //Fetch Players


            foreach (var team in hMTeams)
            {
                var rosterUrl = $"https://statsapi.web.nhl.com/api/v1/teams/{team.ApiId}/roster";
                var rosterData = await httpClient.GetStringAsync(rosterUrl);
                var roster = JsonConvert.DeserializeObject<PersonRoot>(rosterData);

                foreach (var player in roster.roster)
                {
                    try
                    {
                        var playerUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}";
                        var playerData = await httpClient.GetStringAsync(playerUrl);
                        var playerAbout = JsonConvert.DeserializeObject<PeopleRoot>(playerData);

                        var playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}/stats?stats=statsSingleSeason&season=20192020";
                        var playerStatsData = await httpClient.GetStringAsync(playerStatsUrl);
                        var playerStats = JsonConvert.DeserializeObject<StatsRoot>(playerStatsData);

                        hMPlayers.Add(new HMPlayer
                        {
                            Name = player.person.fullName,
                            Position = player.position.abbreviation,
                            Country = playerAbout.People[0].BirthCountry,
                            DateOfBirth = playerAbout.People[0].BirthDate,
                            Height = playerAbout.People[0].Height,
                            Weight = playerAbout.People[0].Weight,
                            GamesPlayed = playerStats.stats[0].splits[0].stat.games,
                            Goals = playerStats.stats[0].splits[0].stat.goals,
                            Assists = playerStats.stats[0].splits[0].stat.assists,
                            Points = playerStats.stats[0].splits[0].stat.points,
                            PenalityMinutes = playerStats.stats[0].splits[0].stat.penaltyMinutes,
                            Saves = playerStats.stats[0].splits[0].stat.saves,
                            Shutouts = playerStats.stats[0].splits[0].stat.shutouts,
                            PlusMinus = playerStats.stats[0].splits[0].stat.plusMinus,
                            ApiId = player.person.id,
                            HeadShotUrl = $"https://nhl.bamcontent.com/images/headshots/current/168x168/{player.person.id}.jpg",
                            TeamId = team.Id
                        });
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        string test = ex.Message;
                    }

                }
            }

            await _context.Players.AddRangeAsync(hMPlayers);
            await _context.SaveChangesAsync();

            return View("Index");
        }
    }
}
