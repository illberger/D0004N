using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D0004N
{

    /// <summary>
    /// <b>Kolla SQL-skripten för det aktuella schemat.</b>
    /// </summary>
    public class Schema
    {
        public class Station
        {
            public int StationId { get; set; } // Pk
            public string Adress { get; set; } = "";
        }


        public class Bokning
        {
            public int BokningsId { get; set; }
            public List<string> RegNr { get; set; }
            public DateTime StartDatum { get; set; }
            public DateTime? SlutDatum { get; set; }
        }

        public class BiltypDto
        {
            public int BilTypId { get; set; }
            public decimal KrDygn { get; set; }
            public decimal KrHelg { get; set; }
        }

        public class BokningBilDto
        {
            public string RegNr { get; set; }
            public DateTime StartDatum { get; set; }
            public DateTime? SlutDatum { get; set; }
        }


        public enum BilType { Stadsbil, Liten, Mellan, Kombi, Minibuss, Transportbil };

        /// <summary>
        /// Exempel på koppling till minimal deskription.
        /// </summary>
        /// <returns></returns>
        public static string[] GetBilType ()
        {
            return Enum.GetNames(typeof(BilType));
        }
    }
}
