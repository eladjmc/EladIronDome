using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EladIronDome.Utils;

namespace EladIronDome.Models
{
    public class Threat
    {

        public Threat()
        {
            Status = THREAT_STATUS.Inactive;
        }


        [Key]
        public int id { get; set; }

        [NotMapped]
        public double ResponseTime
        {
            get
            {
                return ((double)Org.Distance / Type.Speed) * 3600;
            }
        }

        public TerrorOrg Org { get; set; }

        public ThreatAmmunition Type { get; set; }

        public int ThreatAmount {  get; set; }  

        public THREAT_STATUS Status { get; set; } // inActive / active / failed / succeeded

        public DateTime? FiredAt { get; set; }

		public string? ActiveID { get; set; }

	}
}
