using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace ClaimWise.Domain.Entities
{
    public class Dependent
    {
        public int DependentID { get; set; }
        public int PolicyholderID { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }


        [JsonIgnore] // Prevent Swagger from showing nested object
        public Policyholder? Policyholder { get; set; }



        // Navigation: Each dependent belongs to one policyholder
        // public virtual Policyholder Policyholder { get; set; }
    }
}

