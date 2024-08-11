using EladIronDome.Models;
using EladIronDome.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EladIronDome.ViewModels
{
    public class ThreatViewModel
    {
        public List<SelectListItem> TerrorOrgs { get; set; }
        public List<SelectListItem> Types { get; set; }

        public int TerrorOrgId { get; set; }
        public int TypeId { get; set; }

    }
}
