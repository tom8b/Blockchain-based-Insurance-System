pragma solidity ^0.4.17;

contract FlightOracle {
   address public owner;
   
   constructor() public {
        owner = msg.sender;
    }

    modifier onlyOwner() {
        require(msg.sender == owner);
        _;
    }

    function testConnection() public pure returns (bool) {
        return true; 
    }

    struct Flight {
        bytes32 id;
        bool finished;
        bool delayed;
        string airline;
        string airlineFlightId;
    }

    Flight[] public flights;
    mapping(bytes32 => uint) flightIdToIndex; 

    function addFlight(string _airline, string _airlineFlightId, bool finished, bool delayed) onlyOwner public returns (bytes32) {
        bytes32 id = getFlightUniqueId(_airline, _airlineFlightId);

        require(!flightExist(id), "this flight exist");
        
        uint newIndex = flights.push(Flight(id, finished, delayed, _airline, _airlineFlightId))-1; 
        flightIdToIndex[id] = newIndex+1;

        return id;
    }

    function getFlightUniqueId(string _airline, string _airlineFlightId) public view returns (bytes32) {
        return keccak256(abi.encodePacked(_airline, _airlineFlightId)); 
    }

    function updateFlightStatus(string flightNumber, string airline, bool finished, bool delayed) onlyOwner public returns (bytes32) {
        bytes32 id = getFlightUniqueId(airline, flightNumber);
        require(flightExist(id), "There is no flight with given id");

        uint index = flightIdToIndex[id] - 1; 
        flights[index] = Flight(id, finished, delayed, flights[index].airline, flights[index].airlineFlightId);
      
        return id;
    }

    function flightExist(bytes32 _flightId) public view returns (bool) {
        if (flights.length == 0)
            return false;
        uint index = flightIdToIndex[_flightId]; 
        return (index > 0); 
    }

    function getFlight(bytes32 _flightId) private view returns (
        bytes32 id,
        bool finished, 
        bool delayed,
        bool exist,
        string airlineFlightId) {
        
        if (flightExist(_flightId)) {
            Flight storage theFlight = flights[_getFlightIndex(_flightId)]; 
            return (theFlight.id, theFlight.finished, theFlight.delayed, true, theFlight.airlineFlightId); 
        }
        else {
            return (_flightId, false, false, false, "0"); 
        }
    }

    function isFlightDelayed(bytes32 _flightId) public view returns (bool) {
        (bytes32 id, bool finished, bool delayed, bool exist, string memory airlineFlightId) = getFlight(_flightId);
        return delayed;
    }

    function isFlightFinished(bytes32 _flightId) public view returns (bool) {
        (bytes32 id, bool finished, bool delayed, bool exist, string memory airlineFlightId) = getFlight(_flightId);
        return finished;
    }

    function _getFlightIndex(bytes32 _flightId) private view returns (uint) {
        return flightIdToIndex[_flightId]-1; 
    }
}