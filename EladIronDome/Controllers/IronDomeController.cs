using EladIronDome.Contexts;
using EladIronDome.Models;
using EladIronDome.Utils;
using EladIronDome.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            List<Threat> threats = _context.Threats.Include((t) => t.Org).Include(t => t.Type).ToList();
            return View(threats);
        }

        public IActionResult CreateThreat()
        {

            ThreatViewModel tvm = new ThreatViewModel
            {
                Types = _context.ThreatAmmunitions.ToList()
         .Select(ta => new SelectListItem { Value = ta.Id.ToString(), Text = ta.Name }).ToList(),
                TerrorOrgs = _context.TerrorOrgs.ToList()
         .Select(ta => new SelectListItem { Value = ta.Id.ToString(), Text = ta.Name }).ToList(),
            };
            return View(tvm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CreateThreat([FromForm] ThreatViewModel tvm)
        {

            TerrorOrg? to = _context.TerrorOrgs.Find(tvm.TerrorOrgId);
            ThreatAmmunition? ta = _context.ThreatAmmunitions.Find(tvm.TypeId);

            if (to == null || ta == null)
            {
                return NotFound();
            }
            Threat t = new Threat
            {
                Org = to,
                Type = ta
            };

            _context.Threats.Add(t);
            _context.SaveChanges();

            return RedirectToAction(nameof(ThreatManagment));
        }

        public IActionResult LaunchDelete(int id)
        {
            // Find the threat by ID
            var threat = _context.Threats.Find(id);

            if (threat == null || threat.Status == THREAT_STATUS.Active)
            {
                // If the threat doesn't exist, return a 404 Not Found
                return NotFound();
            }

            // Remove the threat from the database
            _context.Threats.Remove(threat);

            // Save the changes
            _context.SaveChanges();

            // Redirect back to the management page
            return RedirectToAction(nameof(ThreatManagment));
        }

        public async Task<IActionResult> LaunchThreat(int attackId)
        {
            // 2. Create a new Task/Thread
            var attackActiveId = Guid.NewGuid().ToString();
            Threat? attack = _context.Threats.Include(t => t.Org).Include(t => t.Type).FirstOrDefault(t => t.id == attackId);

            if (attack != null)
            {
                attack.ActiveID = attackActiveId;
                attack.Status = THREAT_STATUS.Active;
                await _context.SaveChangesAsync();
                var cts = new CancellationTokenSource();

                _attacks[attackActiveId] = cts;

                Task.Run(() => RunTask(attackActiveId, cts.Token, attack), cts.Token);

                return RedirectToAction("ThreatManagment");
            }
            else
            {
                return NotFound();
            }
        }

        private async Task RunTask(string attackId, CancellationToken token, Threat Attack)
        {
            try
            {
                int elapsed = 0;
                while (elapsed < Attack.ResponseTime && !token.IsCancellationRequested)
                {
                    await Task.Delay(2000, token); // Wait for 2 seconds or cancel if requested
                    elapsed += 2;
                    var message = $"====>>{Attack.Org.Name} is attacking by using {Attack.Type.Name} - {elapsed}";
                    _logger.LogInformation(message);
                    //await _hubContext.Clients.All.SendAsync("ReceiveProgress", message);
                }

                // Finished
                if (!token.IsCancellationRequested)
                {
                    //await _hubContext.Clients.All.SendAsync("ReceiveProgress", $"Attack {attackId} completed.");
                }
            }
            catch (TaskCanceledException)
            {
                //await _hubContext.Clients.All.SendAsync("ReceiveProgress", $"Attack {attackId} cancelled.");
            }
            finally
            {
                Threat? attack = _context.Threats.Find(attackId);

                if (attack != null)
                {
                    attack.Status = THREAT_STATUS.Inactive;
                    attack.ActiveID = null;
                    await _context.SaveChangesAsync();
                }
            }
        }
    
        public IActionResult CancelAttack(int id)
        {
            Threat? attack = _context.Threats.Find(id);

            if (attack != null && _attacks.TryRemove(attack.ActiveID, out var cts))
            {
                cts.Cancel();
                Threat? a = _context.Threats.Find(id);

                if (a != null)
                {
                    a.Status = THREAT_STATUS.Inactive;
                    a.ActiveID = null;
                    _context.SaveChangesAsync();
                }
                return RedirectToAction("ThreatManagment");
            }

            return NotFound();
        }

    }
}
