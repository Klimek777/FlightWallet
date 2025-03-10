﻿using SQLite;
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
        public string AirportFromName { get; set; }
        public string AirportToName { get; set; }

        public decimal Price { get; set; }
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }

        public string ImagePath { get; set; }

        public Flight()
        {
            DepartureDate = DateTime.Now.Date;
            DepartureTime = new TimeSpan(12, 0, 0);
        }

        public Flight Clone() => MemberwiseClone() as Flight;

        

    }
}
