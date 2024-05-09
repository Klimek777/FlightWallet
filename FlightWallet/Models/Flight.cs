using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightWallet.Models
{
    public class Flight
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FlightFromName { get; set; }
        public string FlightToName {  get; set; }
        public decimal Price { get; set; }
        public DateTime DepartureDate { get; set; }

        public Flight Clone() => MemberwiseClone() as Flight;
    }
}
