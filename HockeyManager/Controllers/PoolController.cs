using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HockeyManager.Areas.Identity.Data;
using HockeyManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult Index()
        {
            return View();
        }

        // GET: Pool/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Pool/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pool/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

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
    }
}