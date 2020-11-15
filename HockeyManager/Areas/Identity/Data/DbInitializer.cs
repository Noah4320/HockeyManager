﻿using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.Models.ApiModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HockeyManager.Areas.Identity.Data
{
    public class DbInitializer
    {
        private readonly HockeyContext _context;

        public DbInitializer(HockeyContext context)
        {
            _context = context;
        }

        public async Task FetchApiData()
        {

            var anyTeams = _context.TeamInfo.Any();

            if (anyTeams)
            {
                return;
            }

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
                hMTeamsInfo.Abbreviation = team.abbreviation;
                hMTeamsInfo.logoUrl = $"https://www-league.nhlstatic.com/images/logos/teams-current-primary-light/{team.id}.svg";

                await _context.TeamInfo.AddAsync(hMTeamsInfo);
                await _context.SaveChangesAsync();

                hMTeams.Add(new HMTeam
                {
                    Division = team.division.name,
                    Conference = team.conference.name,
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
                    try
                    {

                        var playerUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}";
                        var playerData = await httpClient.GetStringAsync(playerUrl);
                        var playerAbout = JsonConvert.DeserializeObject<PeopleRoot>(playerData);

                        var playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.person.id}/stats?stats=statsSingleSeason&season=20192020";
                        var playerStatsData = await httpClient.GetStringAsync(playerStatsUrl);
                        var playerStats = JsonConvert.DeserializeObject<StatsRoot>(playerStatsData);


                        hMPlayerInfo.Name = player.person.fullName;
                        hMPlayerInfo.Country = playerAbout.People[0].BirthCountry;
                        hMPlayerInfo.DateOfBirth = playerAbout.People[0].BirthDate;
                        hMPlayerInfo.Height = playerAbout.People[0].Height;
                        hMPlayerInfo.Weight = playerAbout.People[0].Weight;
                        hMPlayerInfo.HeadShotUrl = $"https://nhl.bamcontent.com/images/headshots/current/168x168/{player.person.id}.jpg";

                        await _context.PlayerInfo.AddAsync(hMPlayerInfo);
                        await _context.SaveChangesAsync();

                        var TOI = playerStats.stats[0].splits[0].stat.timeOnIce.Split(":");
                        int.TryParse(TOI[0], out int parsedTOI);

                        hMPlayers.Add(new HMPlayer
                        {
                            Position = player.position.abbreviation,
                            GamesPlayed = playerStats.stats[0].splits[0].stat.games,
                            TimeOnIce = parsedTOI,
                            Shots = playerStats.stats[0].splits[0].stat.shots,
                            Goals = playerStats.stats[0].splits[0].stat.goals,
                            Assists = playerStats.stats[0].splits[0].stat.assists,
                            Points = playerStats.stats[0].splits[0].stat.points,
                            Hits = playerStats.stats[0].splits[0].stat.hits,
                            PowerPlayGoals = playerStats.stats[0].splits[0].stat.powerPlayGoals,
                            PenalityMinutes = playerStats.stats[0].splits[0].stat.pim,
                            Saves = playerStats.stats[0].splits[0].stat.saves,
                            Shutouts = playerStats.stats[0].splits[0].stat.shutouts,
                            GoalsAgainst = playerStats.stats[0].splits[0].stat.goalsAgainst,
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
        }


        public async Task FetchUpdatedStats()
        {
            //ToDo: change date in url
            var currentDate = DateTime.Now;

            var gamesUrl = $"https://statsapi.web.nhl.com/api/v1/schedule?startDate={currentDate.Year}-{currentDate.Month}-{currentDate.Day}&endDate={currentDate.Year}-{currentDate.Month}-{currentDate.Day}";
            var httpClient = HttpClientFactory.Create();
            var gamesData = await httpClient.GetStringAsync(gamesUrl);
            var games = JsonConvert.DeserializeObject<GameRoot>(gamesData);



            foreach (var game in games.dates[0].games)
            {
                var awayTeam = _context.Teams.Where(x => x.ApiId == game.teams.away.team.id).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).FirstOrDefault();


                foreach (var player in awayTeam.Players)
                {
                    string playerStatsUrl = "";

                    //Is it before September?
                    if (currentDate.Month <= 8)
                    {
                        playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.ApiId}/stats?stats=statsSingleSeason&season={currentDate.Year - 1}{currentDate.Year}";
                    } 
                    else
                    {
                        playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.ApiId}/stats?stats=statsSingleSeason&season={currentDate.Year}{currentDate.Year + 1}";
                    }
                    var playerStatsData = await httpClient.GetStringAsync(playerStatsUrl);
                    var playerStats = JsonConvert.DeserializeObject<StatsRoot>(playerStatsData);

                    var poolRecords = _context.Players.Include(x => x.Team).Include(x => x.PlayerInfo).Where(x => x.Team.PoolId != null && x.PlayerInfo.Name == player.PlayerInfo.Name).ToList();

                    if (poolRecords != null)
                    {
                        foreach (var record in poolRecords)
                        {
                            try
                            {
                                
                                var TOI = playerStats.stats[0].splits[0].stat.timeOnIce.Split(":");
                                int.TryParse(TOI[0], out int parsedTOI);


                                record.GamesPlayed = (playerStats.stats[0].splits[0].stat.games - player.GamesPlayed) + record.GamesPlayed;
                                record.TimeOnIce = (parsedTOI - player.TimeOnIce) + record.TimeOnIce;
                                record.Shots = (playerStats.stats[0].splits[0].stat.shots - player.Shots) + record.Shots;
                                record.Goals = (playerStats.stats[0].splits[0].stat.goals - player.Goals) + record.Goals;
                                record.Assists = (playerStats.stats[0].splits[0].stat.assists - player.Assists) + record.Assists;
                                record.Points = (playerStats.stats[0].splits[0].stat.points - player.Points) + record.Points;
                                record.Hits = (playerStats.stats[0].splits[0].stat.hits - player.Hits) + record.Hits;
                                record.PowerPlayGoals = (playerStats.stats[0].splits[0].stat.powerPlayGoals - player.PowerPlayGoals) + record.PowerPlayGoals;
                                record.PenalityMinutes = (playerStats.stats[0].splits[0].stat.pim - player.PenalityMinutes) + record.PenalityMinutes;
                                record.Saves = (playerStats.stats[0].splits[0].stat.saves - player.Saves) + record.Saves;
                                record.Shutouts = (playerStats.stats[0].splits[0].stat.shutouts - player.Shutouts) + record.Shutouts;
                                record.GoalsAgainst = (playerStats.stats[0].splits[0].stat.goalsAgainst - player.GoalsAgainst) + record.GoalsAgainst;
                                record.PlusMinus = (playerStats.stats[0].splits[0].stat.plusMinus - player.PlusMinus) + record.PlusMinus;

                                _context.Players.Update(record);
                                await _context.SaveChangesAsync();
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                var test = ex.Message;
                            }
                        }
                    }
                    try
                    {
                        var TOI = playerStats.stats[0].splits[0].stat.timeOnIce.Split(":");
                        int.TryParse(TOI[0], out int parsedTOI);

                        player.GamesPlayed = playerStats.stats[0].splits[0].stat.games;
                        player.TimeOnIce = parsedTOI;
                        player.Shots = playerStats.stats[0].splits[0].stat.shots;
                        player.Goals = playerStats.stats[0].splits[0].stat.goals;
                        player.Assists = playerStats.stats[0].splits[0].stat.assists;
                        player.Points = playerStats.stats[0].splits[0].stat.points;
                        player.Hits = playerStats.stats[0].splits[0].stat.hits;
                        player.PowerPlayGoals = playerStats.stats[0].splits[0].stat.powerPlayGoals;
                        player.PenalityMinutes = playerStats.stats[0].splits[0].stat.pim;
                        player.Saves = playerStats.stats[0].splits[0].stat.saves;
                        player.Shutouts = playerStats.stats[0].splits[0].stat.shutouts;
                        player.GoalsAgainst = playerStats.stats[0].splits[0].stat.goalsAgainst;
                        player.PlusMinus = playerStats.stats[0].splits[0].stat.plusMinus;

                        _context.Players.Update(player);
                        await _context.SaveChangesAsync();

                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        var test = ex.Message;
                    }
                }

                var homeTeam = _context.Teams.Where(x => x.ApiId == game.teams.home.team.id).Include(x => x.Players).ThenInclude(x => x.PlayerInfo).FirstOrDefault();


                foreach (var player in homeTeam.Players)
                {
                    string playerStatsUrl = "";

                    //Is it before September?
                    if (currentDate.Month <= 8)
                    {
                        playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.ApiId}/stats?stats=statsSingleSeason&season={currentDate.Year - 1}{currentDate.Year}";
                    }
                    else
                    {
                        playerStatsUrl = $"https://statsapi.web.nhl.com/api/v1/people/{player.ApiId}/stats?stats=statsSingleSeason&season={currentDate.Year}{currentDate.Year + 1}";
                    }
                    var playerStatsData = await httpClient.GetStringAsync(playerStatsUrl);
                    var playerStats = JsonConvert.DeserializeObject<StatsRoot>(playerStatsData);

                    var poolRecords = _context.Players.Include(x => x.Team).Include(x => x.PlayerInfo).Where(x => x.Team.PoolId != null && x.PlayerInfo.Name == player.PlayerInfo.Name).ToList();

                    if (poolRecords != null)
                    {
                        foreach (var record in poolRecords)
                        {
                            try
                            {
                                int.TryParse(playerStats.stats[0].splits[0].stat.penaltyMinutes, out int parsedPM);

                                record.GamesPlayed = (playerStats.stats[0].splits[0].stat.games - player.GamesPlayed) + record.GamesPlayed;
                                record.Shots = (playerStats.stats[0].splits[0].stat.shots - player.Shots) + record.Shots;
                                record.Goals = (playerStats.stats[0].splits[0].stat.goals - player.Goals) + record.Goals;
                                record.Assists = (playerStats.stats[0].splits[0].stat.assists - player.Assists) + record.Assists;
                                record.Points = (playerStats.stats[0].splits[0].stat.points - player.Points) + record.Points;
                                record.PenalityMinutes = (parsedPM - player.PenalityMinutes) + record.PenalityMinutes;
                                record.Saves = (playerStats.stats[0].splits[0].stat.saves - player.Saves) + record.Saves;
                                record.Shutouts = (playerStats.stats[0].splits[0].stat.shutouts - player.Shutouts) + record.Shutouts;
                                record.PlusMinus = (playerStats.stats[0].splits[0].stat.plusMinus - player.PlusMinus) + record.PlusMinus;

                                _context.Players.Update(record);
                                await _context.SaveChangesAsync();
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                var test = ex.Message;
                            }
                        }
                    }
                    try
                    {
                        int.TryParse(playerStats.stats[0].splits[0].stat.penaltyMinutes, out int parsedPM);

                        player.GamesPlayed = playerStats.stats[0].splits[0].stat.games;
                        player.Shots = playerStats.stats[0].splits[0].stat.shots;
                        player.Goals = playerStats.stats[0].splits[0].stat.goals;
                        player.Assists = playerStats.stats[0].splits[0].stat.assists;
                        player.Points = playerStats.stats[0].splits[0].stat.points;
                        player.PenalityMinutes = parsedPM;
                        player.Saves = playerStats.stats[0].splits[0].stat.saves;
                        player.Shutouts = playerStats.stats[0].splits[0].stat.shutouts;
                        player.PlusMinus = playerStats.stats[0].splits[0].stat.plusMinus;

                        _context.Players.Update(player);
                        await _context.SaveChangesAsync();
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        var test = ex.Message;
                    }
                }
            }


        }

    }
}
