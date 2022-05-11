using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace FlightApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FlightController : ControllerBase
	{
		private readonly string _abi =
			"[\r\n\t{\r\n\t\t\"constant\": false,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_airline\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_airlineFlightId\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"finished\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"delayed\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"addFlight\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"nonpayable\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"uint256\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"flights\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"id\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"finished\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"delayed\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"airline\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"airlineFlightId\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [],\r\n\t\t\"name\": \"testConnection\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_flightId\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"isFlightFinished\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_flightId\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"flightExist\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_airline\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_airlineFlightId\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"getFlightUniqueId\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": false,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"flightNumber\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"airline\",\r\n\t\t\t\t\"type\": \"string\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"finished\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t},\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"delayed\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"updateFlightStatus\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"nonpayable\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [],\r\n\t\t\"name\": \"owner\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"address\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"constant\": true,\r\n\t\t\"inputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"_flightId\",\r\n\t\t\t\t\"type\": \"bytes32\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"name\": \"isFlightDelayed\",\r\n\t\t\"outputs\": [\r\n\t\t\t{\r\n\t\t\t\t\"name\": \"\",\r\n\t\t\t\t\"type\": \"bool\"\r\n\t\t\t}\r\n\t\t],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"view\",\r\n\t\t\"type\": \"function\"\r\n\t},\r\n\t{\r\n\t\t\"inputs\": [],\r\n\t\t\"payable\": false,\r\n\t\t\"stateMutability\": \"nonpayable\",\r\n\t\t\"type\": \"constructor\"\r\n\t}\r\n]";
		private readonly string _privateKey = "8814f6c2ee6d092b3ea1ddf921c1c552b059416f38d925dd40a746fc84aee1f3";

		private readonly string _senderAddress = "0x75e1087d76b8836aaa9ED48b0EAb812512d896ff";
		private readonly string _flightOraclecontractAddress = "0x3018FE4904B7a49740Af63B29e1e3fD3614a59c0";

		private static List<Flight> _flightList = new List<Flight>();

		private readonly Account _account;
		private readonly Web3 _web3;

		public FlightController(ILogger<FlightController> logger)
		{
			_account = new Account(_privateKey, Chain.Private);
			_web3 = new Web3(_account);
			_web3.TransactionManager.UseLegacyAsDefault = true;

		}

		[HttpGet]
		public IEnumerable<Flight> Get()
		{
			return _flightList;
		}

		[HttpGet("{airline}/{flightId}")]
		public Flight Get(string airline, string flightId)
		{
			return _flightList.FirstOrDefault(x => x.FlightNumber == flightId && x.Airline == airline);
		}

		[HttpGet("addTestData")]
		public async Task AddTestData()
		{
			_flightList = new List<Flight>
			{
				new Flight {Airline = "Ryanair", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "RY113", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Ryanair", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "RY242", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Lot", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "L562", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Lot", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "L125", IsDelayed = false, IsFinished = false},           
				new Flight {Airline = "Wizz Air", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "WA754", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Wizz Air", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "WA2135", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Lufthansa", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "LF632", IsDelayed = false, IsFinished = false},
				new Flight {Airline = "Lufthansa", DepartureDate = DateTime.UtcNow, ArrivalDate = DateTime.Now, FlightNumber = "LF743", IsDelayed = false, IsFinished = false}
			};

			foreach (var flight in _flightList)
			{
				//var byteStr = HexByteConvertorExtensions.HexToByteArray(flight.FlightNumber);
				object[] paramsObject = { flight.Airline, flight.FlightNumber, flight.IsFinished, flight.IsDelayed };
				var contract = _web3.Eth.GetContract(_abi, _flightOraclecontractAddress);

				var function = contract.GetFunction("addFlight");
				var gas = await function.EstimateGasAsync(_senderAddress, null, null, paramsObject);
				await function.SendTransactionAsync(_senderAddress, gas: gas, null, paramsObject);
			}
		}

		[HttpPost("update")]
		public async Task Update(UpdateFlightRequest request)
		{
			object[] paramsObject = { request.FlightId, request.Airline, request.Finished, request.Delayed };
			var contract = _web3.Eth.GetContract(_abi, _flightOraclecontractAddress);

			var function = contract.GetFunction("updateFlightStatus");
			var gas = await function.EstimateGasAsync(_senderAddress, null, null, paramsObject);
			await function.SendTransactionAsync(_senderAddress, gas: gas, null, paramsObject);
			_flightList.FirstOrDefault(x => x.FlightNumber == request.FlightId && request.Airline == x.Airline).IsDelayed = request.Delayed;
			_flightList.FirstOrDefault(x => x.FlightNumber == request.FlightId && request.Airline == x.Airline).IsFinished = request.Finished;
		}

		public class UpdateFlightRequest
		{
			public string FlightId { get; set; }
			public string Airline { get; set; }
			public bool Delayed { get; set; }
			public bool Finished { get; set; }
		}
	}
}
