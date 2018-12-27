﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using grow.Data;
using grow.Models;
using grow.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.IO;

namespace grow.Controllers
{
    public class PlantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlantsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; ;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Plants
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            var applicationDbContext = _context.Plant.Include(p => p.PlantType).Include(p => p.User).Where(u => u.User == user);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Plants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DetailsPlantViewModel viewmodel = new DetailsPlantViewModel(_context);
            var plant = await _context.Plant
                .Include(p => p.PlantType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PlantId == id);

            var plantAudit = _context.PlantAudit.Where(pa => pa.PlantId == id).OrderByDescending(o => o.DateCreated).ToList();

            viewmodel.Plant = plant;
            viewmodel.PlantAudit = plantAudit;
            if (plant == null)
            {
                return NotFound();
            }

            return View(viewmodel);
        }

        // GET: Plants/Create
        public IActionResult Create()
        {
            ViewData["PlantTypeId"] = new SelectList(_context.PlantType, "PlantTypeId", "Name");
            return View();
        }

        // POST: Plants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlantId,DateCreated,Name,Notes,InitialImage,UserId,PlantTypeId")] Plant plant)
        {
            // Remove user and userId
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("DateCreated");

            if (ModelState.IsValid)
            {
                // Get the current user
                var user = await GetCurrentUserAsync();
                plant.User = user;
                plant.UserId = user.Id;

                var file = plant.InitialImage;
                var parsedContentDisposition =
                    ContentDispositionHeaderValue.Parse(file.ContentDisposition);
                var filename = Path.Combine(_context.WebRootPath,
                    "Uploads", parsedContentDisposition.FileName.Trim('"'));
                using (var stream = System.IO.File.OpenWrite(filename))
                {
                    await file.CopyToAsync(stream);
                }

                _context.Add(plant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plant);
        }

        // GET: Plants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var plant = await _context.Plant.FindAsync(id);

            if (plant == null)
            {
                return NotFound();
            }

            ViewData["PlantTypeId"] = new SelectList(_context.PlantType, "PlantTypeId", "Name", plant.PlantTypeId);
            return View(plant);
        }

        // POST: Plants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlantId,DateCreated,Name,Notes,InitialImage,UserId,PlantTypeId")] Plant plant)
        {
            if (id != plant.PlantId)
            {
                return NotFound();
            }

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user
                    var user = await GetCurrentUserAsync();
                    plant.User = user;

                    _context.Update(plant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantExists(plant.PlantId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PlantTypeId"] = new SelectList(_context.PlantType, "PlantTypeId", "Name", plant.PlantTypeId);

            return View(plant);
        }

        // GET: Plants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _context.Plant
                .Include(p => p.PlantType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PlantId == id);
            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }

        // POST: Plants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plant = await _context.Plant.FindAsync(id);
            _context.Plant.Remove(plant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlantExists(int id)
        {
            return _context.Plant.Any(e => e.PlantId == id);
        }
    }
}
