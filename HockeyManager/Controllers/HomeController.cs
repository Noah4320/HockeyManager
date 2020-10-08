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
using HockeyManager.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

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

            foreach (var team in teams.teams)
            {
                HMTeamInfo hMTeamsInfo = new HMTeamInfo();
                HMTeam hMTeam = new HMTeam();

                var teamStatsUrl = $"https://statsapi.web.nhl.com/api/v1/teams/{team.id}/stats";
                var teamStatsData = await httpClient.GetStringAsync(teamStatsUrl);
                var teamStats = JsonConvert.DeserializeObject<TeamStatRoot>(teamStatsData);

                hMTeamsInfo.Name = team.name;
                hMTeamsInfo.Conference = team.conference.name;
                hMTeamsInfo.Division = team.division.name;
                hMTeamsInfo.Abbreviation = team.abbreviation;                   
                hMTeamsInfo.logoUrl = $"https://www-league.nhlstatic.com/images/logos/teams-current-primary-light/{team.id}.svg";

                await _context.TeamInfo.AddAsync(hMTeamsInfo);
                await _context.SaveChangesAsync();

                hMTeams.Add(new HMTeam
                {
                    GamesPlayed = teamStats.stats[0].splits[0].stat.gamesPlayed,
                    Wins = Convert.ToInt32(teamStats.stats[0].splits[0].stat.wins),
                    Loses = Convert.ToInt32(teamStats.stats[0].splits[0].stat.losses),
                    OvertimeLoses = Convert.ToInt32(teamStats.stats[0].splits[0].stat.ot),
                    Points = Convert.ToInt32(teamStats.stats[0].splits[0].stat.pts),
                    ApiId = team.id,
                    TeamInfoId = hMTeamsInfo.Id
            });

            }

            await _context.Teams.AddRangeAsync(hMTeams);
            await _context.SaveChangesAsync();

            //Fetch Players

            List<HMPlayer> hMPlayers = new List<HMPlayer>();

            foreach (var team in hMTeams)
            {
                var rosterUrl = $"https://statsapi.web.nhl.com/api/v1/teams/{team.ApiId}/roster";
                var rosterData = await httpClient.GetStringAsync(rosterUrl);
                var roster = JsonConvert.DeserializeObject<PersonRoot>(rosterData);

                foreach (var player in roster.roster)
                {
                    HMPlayerInfo hMPlayerInfo = new HMPlayerInfo();
                    HMPlayer hMPlayer = new HMPlayer();
                    try
                    {
                        
                        var playerUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}";
                        var playerData = await httpClient.GetStringAsync(playerUrl);
                        var playerAbout = JsonConvert.DeserializeObject<PeopleRoot>(playerData);

                        var playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}/stats?stats=statsSingleSeason&season=20192020";
                        var playerStatsData = await httpClient.GetStringAsync(playerStatsUrl);
                        var playerStats = JsonConvert.DeserializeObject<StatsRoot>(playerStatsData);


                        hMPlayerInfo.Name = player.person.fullName;
                        hMPlayerInfo.Position = player.position.abbreviation;
                        hMPlayerInfo.Country = playerAbout.People[0].BirthCountry;
                        hMPlayerInfo.DateOfBirth = playerAbout.People[0].BirthDate;
                        hMPlayerInfo.Height = playerAbout.People[0].Height;
                        hMPlayerInfo.Weight = playerAbout.People[0].Weight;
                        hMPlayerInfo.HeadShotUrl = $"https://nhl.bamcontent.com/images/headshots/current/168x168/{player.person.id}.jpg";

                        await _context.PlayerInfo.AddAsync(hMPlayerInfo);
                        await _context.SaveChangesAsync();

                        hMPlayers.Add(new HMPlayer
                        {
                            GamesPlayed = playerStats.stats[0].splits[0].stat.games,
                            Goals = playerStats.stats[0].splits[0].stat.goals,
                            Assists = playerStats.stats[0].splits[0].stat.assists,
                            Points = playerStats.stats[0].splits[0].stat.points,
                            PenalityMinutes = playerStats.stats[0].splits[0].stat.penaltyMinutes,
                            Saves = playerStats.stats[0].splits[0].stat.saves,
                            Shutouts = playerStats.stats[0].splits[0].stat.shutouts,
                            PlusMinus = playerStats.stats[0].splits[0].stat.plusMinus,
                            ApiId = player.person.id,
                            TeamId = team.Id,
                            PlayerInfoId = hMPlayerInfo.Id
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

            //Populate Pool rulesets

            List<RuleSet> ruleSets = new List<RuleSet>();

            ruleSets.Add(new RuleSet
            {
                Name = "Top Scorers",
                Description = "Do you believe that you have what it takes to pick the top goal scorers in the league? Pick your top 5 skaters from any team.",
                maxForwards = 5,
                maxDefensemen = 5,
                maxGoalies = 0,
                maxPlayers = 5
            });

            ruleSets.Add(new RuleSet
            {
                Name = "Point Producers",
                Description = "Pick your top point producers from around the league. Pick 7 forwards and 3 defenceman.",
                maxForwards = 7,
                maxDefensemen = 3,
                maxGoalies = 0,
                maxPlayers = 10
            });

            ruleSets.Add(new RuleSet
            {
                Name = "Assist Machine",
                Description = "Nothing but assists! Pick your top 5 assist getters.",
                maxForwards = 5,
                maxDefensemen = 5,
                maxGoalies = 0,
                maxPlayers = 5
            });

            await _context.RuleSets.AddRangeAsync(ruleSets);
            await _context.SaveChangesAsync();

            return View("Index");
        }
    }
}
