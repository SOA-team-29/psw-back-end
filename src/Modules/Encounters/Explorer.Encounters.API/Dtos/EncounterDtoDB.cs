using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Encounters.API.Dtos;

public class EncounterDtoDB
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int XpPoints { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool ShouldBeApproved { get; set; }
}
