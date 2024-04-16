using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Encounters.API.Dtos
{
    public class SocialEncounterDtoDB
    {
        public string Id { get; set; }
        public string EncounterId { get; set; }

        public int TouristsRequiredForCompletion { get; set; }
        public double DistanceTreshold { get; set; }
        public List<long> TouristIDs { get; set; }
    }
}
