pragma solidity ^0.4.17;

contract OracleInterface {
    function testConnection() public pure returns (bool);
    function flightExist(bytes32 _flightId) public view returns (bool);
    function isFlightDelayed(bytes32 _flightId) public view returns (bool);
    function getFlightUniqueId(string _airline, string _airlineFlightId) public returns (bytes32);
    function isFlightFinished(bytes32 _flightId) public view returns (bool);
}