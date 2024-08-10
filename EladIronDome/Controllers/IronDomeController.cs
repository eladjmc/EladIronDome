using EladIronDome.Contexts;
using EladIronDome.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace EladIronDome.Controllers
{
    public class IronDomeController : Controller
    {
        private readonly IronDomeDbContext _context; // Holds the DB context
        private readonly ILogger<IronDomeController> _logger; // Holds logger to log stuff
        private static ConcurrentDictionary<string, CancellationTokenSource> _attacks = new ConcurrentDictionary<string, CancellationTokenSource>(); // Holds dictionary to hold the threads and thier cancelation token

        public IronDomeController(IronDomeDbContext context, ILogger<IronDomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: IronDomeController
        public IActionResult ManagmentScreen()
        {
            List<DefenceAmmunition> defenceAmmunitions = _context.DefenceAmmunitions.ToList();
            return View(defenceAmmunitions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateManagmentScreen(List<DefenceAmmunition> defenceAmmunitions)
        {
            foreach (var ammo in defenceAmmunitions)
            {
                // Find the existing record in the database
                var existingAmmo = _context.DefenceAmmunitions.Find(ammo.Id);

                if (existingAmmo != null)
                {
                    // Update the existing record's properties
                    existingAmmo.Amount = ammo.Amount;
                    // If you want to update other fields, do it here
                }
                else
                {
                    TempData["ErrorMessage"] = "Data was not updated successfully!";
                    return RedirectToAction("ManagmentScreen");
                }
            }

            // Save all changes to the database
            _context.SaveChanges();

            // Optionally, you can add a success message to TempData
            TempData["SuccessMessage"] = "Data updated successfully!";

            // log result
            _logger.LogInformation("Updated ammo");

            return RedirectToAction("ManagmentScreen");
        }


        public IActionResult ThreatManagment()
        {
            
            List<Threat> threats = _context.Threats.Include((t)=>t.Org).Include(t=>t.Type).ToList();
            return View(threats);
        }
    }
}
