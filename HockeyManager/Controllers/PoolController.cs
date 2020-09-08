﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using HockeyManager.Models;
using HockeyManager.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HockeyManager.Controllers
{
    [Authorize]
    public class PoolController : Controller
    {

        private readonly HockeyContext _context;
        private UserManager<User> _userManager;

        public PoolController(HockeyContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pool
        public async Task<ActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var pools = _context.PoolList.Where(x => x.UserId == user.Id).Include(x => x.Pool);

            PoolsViewModel poolsViewModel = new PoolsViewModel();
            poolsViewModel.poolList = pools;

            return View(poolsViewModel);
        }

        // GET: Pool/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Pool/Create
        public ActionResult CreatePool()
        {
            return View();
        }

        // POST: Pool/CreatePool
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePool(Pool pool)
        {
            var user = await _userManager.GetUserAsync(User);
            pool.Status = "Active";
            pool.OwnerId = user.Id;
            try
            {
                // TODO: Add insert logic here

                await _context.Pools.AddAsync(pool);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JoinPool(Pool pool)
        {
            var user = await _userManager.GetUserAsync(User);
            try
            {
                PoolList joinPool = new PoolList();
                var pools = _context.Pools.First(x => x.Name == pool.Name);

                joinPool.PoolId = pools.Id;
                joinPool.UserId = user.Id;

                await _context.PoolList.AddAsync(joinPool);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
           
        }

            // GET: Pool/Edit/5
            public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Pool/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Pool/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Pool/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public string GetRule()
        {
            int id = int.Parse(Request.Form["id"]);

            var result = _context.RuleSets.Where(x => x.Id == id).Select(x => x.Description);

            return result.First();
        }

        [HttpGet]
        public string[] GetPools()
        {
            var pools = _context.Pools.Where(x => x.Private == false).Select(x => x.Name).ToArray();

            return pools;
        }
    }
}