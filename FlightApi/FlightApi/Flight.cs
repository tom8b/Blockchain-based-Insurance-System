using System;

namespace FlightApi
{
	public class Flight
	{
		public DateTime DepartureDate { get; set; }
		public DateTime ArrivalDate { get; set; }
		public string Airline { get; set; }
		public string FlightNumber { get; set; }
		public bool IsDelayed { get; set; }
		public bool IsFinished { get; set; }
	}
}
