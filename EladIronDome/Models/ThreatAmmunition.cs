using System.ComponentModel.DataAnnotations;

namespace EladIronDome.Models
{
    public class ThreatAmmunition
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Speed { get; set; }
    }
}
