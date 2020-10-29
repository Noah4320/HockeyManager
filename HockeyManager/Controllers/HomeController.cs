using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HockeyManager.Models;
using HockeyManager.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<ActionResult> ConfigurePlayerRanks()
        {
            var allCenters = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 10 && x.Position == "C").ToListAsync();
            var allLeftWingers = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 10 && x.Position == "LW").ToListAsync();
            var allRightWingers = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 10 && x.Position == "RW").ToListAsync();
            var allDefencemen = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 10 && x.Position == "D").ToListAsync();
            var allGoalies = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed > 20 && x.Position == "G").ToListAsync();

            //These Skaters have 10 or less games played and the goalies have 20 or less
            var allRookieCenters = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed <= 10 && x.Position == "C").ToListAsync();
            var allRookieLeftWingers = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed <= 10 && x.Position == "LW").ToListAsync();
            var allRookieRightWingers = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed <= 10 && x.Position == "RW").ToListAsync();
            var allRookieDefencemen = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed <= 10 && x.Position == "D").ToListAsync();
            var allRookieGoalies = await _context.Players.Where(x => x.ApiId != 0 && x.GamesPlayed <= 20 && x.Position == "G").ToListAsync();


            await UpdateOveralls(allCenters, false);
            await UpdateOveralls(allLeftWingers, false);
            await UpdateOveralls(allRightWingers, false);
            await UpdateOveralls(allDefencemen, false);
            await UpdateOveralls(allGoalies, false);

            await UpdateOveralls(allRookieCenters, true);
            await UpdateOveralls(allRookieLeftWingers, true);
            await UpdateOveralls(allRookieRightWingers, true);
            await UpdateOveralls(allRookieDefencemen, true);
            await UpdateOveralls(allRookieGoalies, true);

            return RedirectToAction("Index");
        }

        public async Task UpdateOveralls(List<HMPlayer> players, bool isRookie) {

            List<decimal> oldRanks = new List<decimal>();

            foreach (var player in players)
            {
                decimal gamesPlayed = Convert.ToDecimal(player.GamesPlayed);
                decimal goals = Convert.ToDecimal(player.Goals);
                decimal assists = Convert.ToDecimal(player.Assists);
                decimal points = Convert.ToDecimal(player.Points);
                decimal Toi = Convert.ToDecimal(player.TimeOnIce);
                decimal hits = Convert.ToDecimal(player.Hits);
                decimal plusMinus = Convert.ToDecimal(player.PlusMinus);
                decimal powerPlayGoals = Convert.ToDecimal(player.PowerPlayGoals);
                decimal penalityMinutes = Convert.ToDecimal(player.PenalityMinutes);
                decimal saves = Convert.ToDecimal(player.Saves);
                decimal goalsAgainst = Convert.ToDecimal(player.GoalsAgainst);
                decimal shutouts = Convert.ToDecimal(player.Shutouts);

                if (player.Position == "C")
                {
                    decimal rank = (0.75m * (goals / gamesPlayed) + 0.70m * (assists / gamesPlayed) + 0.20m * (Toi / gamesPlayed) + 0.05m * (hits / gamesPlayed) + 0.10m * (powerPlayGoals / gamesPlayed) - 0.05m * (penalityMinutes / gamesPlayed) + 0.10m * (plusMinus / gamesPlayed));
                    oldRanks.Add(rank);
                }
                else if (player.Position == "LW" || player.Position == "RW")
                {
                    decimal rank = (0.75m * (goals / gamesPlayed) + 0.70m * (assists / gamesPlayed) + 0.20m * (Toi / gamesPlayed) + 0.05m * (hits / gamesPlayed) + 0.10m * (powerPlayGoals / gamesPlayed) - 0.05m * (penalityMinutes / gamesPlayed) + 0.05m * (plusMinus / gamesPlayed));
                    oldRanks.Add(rank);
                }
                else if (player.Position == "D")
                {
                    decimal rank = (0.75m * (goals / gamesPlayed) + 0.70m * (assists / gamesPlayed) + 0.20m * (Toi / gamesPlayed) + 0.10m * (hits / gamesPlayed) + 0.03m * (powerPlayGoals / gamesPlayed) - 0.03m * (penalityMinutes / gamesPlayed) + 0.05m * (plusMinus / gamesPlayed));
                    oldRanks.Add(rank);
                }
                if (player.Position == "G")
                {
                    decimal rank = (-0.75m * (goalsAgainst / gamesPlayed) + 0.10m * (saves / gamesPlayed) + 0.20m * (shutouts / gamesPlayed));
                    oldRanks.Add(rank);
                }

            }


            decimal oldMin = oldRanks.Min();
            decimal oldMax = oldRanks.Max();
            decimal newMax;
            decimal newMin;

            if (isRookie)
            {
                newMax = 64;
                newMin = 59;
            }
            else
            {
                newMax = 95;
                newMin = 65;
            }
     

            int indexCount = 0;
            foreach (var player in players)
            {
                decimal oldValue = oldRanks[indexCount];
                player.Overall = (int)Math.Round((((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin, 0);
                _context.Players.Attach(player);
                _context.Entry(player).Property(x => x.Overall).IsModified = true;
                indexCount++;
            }

            await _context.SaveChangesAsync();
        }

    }
}
