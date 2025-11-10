using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Domain.Entities
{
    public class Hospital
    {
        public int HospitalID { get; set; }
        public string HospitalName { get; set; }
        public string Location { get; set; }
        public bool IsNetworkHospital { get; set; }

        public virtual ICollection<Claim> Claims { get; set; }
    }

}
