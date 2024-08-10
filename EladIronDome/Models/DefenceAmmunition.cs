using System.ComponentModel.DataAnnotations;

namespace EladIronDome.Models
{
    public class DefenceAmmunition
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}
