pragma solidity ^0.4.17;

import "./OracleInterface.sol";
import "./Ownable.sol";

contract Insurance is Ownable {
    address internal flightOracleAddr = 0;
    OracleInterface internal flightOracle = OracleInterface(flightOracleAddr); 

    function setOracleAddress(address _oracleAddress) external onlyOwner returns (bool) {
        flightOracleAddr = _oracleAddress;
        flightOracle = OracleInterface(flightOracleAddr); 
        return flightOracle.testConnection();
    }

    function testOracleConnection() public view returns (bool) {
        return flightOracle.testConnection(); 
    }

    struct InsurancePolicy {
        bytes32 id;
        uint amount;
        address insuree;
        string flightNumber;
        string airline;
        bool payedOut;
        bool userPaidIn;
        bool insureerPaidIn;
        uint userAmount;
        uint airlineAmount;
        bool cancelled;
    }

    struct Airline {
        string name;
        address owner;
    }

    mapping(string => Airline) private airlines;
    mapping(string => bool) private takenAirlineNames;
    mapping(bytes32 => InsurancePolicy) private insurancePolicies;
    mapping(address => bytes32[]) private userInsuranceIds;

    function registerAirline(string memory name, address owner) public onlyOwner {
        require(takenAirlineNames[name] == false, "Airline with given name is already registered in the system");
        takenAirlineNames[name] = true;
        airlines[name] = Airline(name, owner);
    }

    function insure(address insuree, uint userAmount, uint airlineAmount, string memory airline, string flightNumber) public returns (bytes32) {
        require(takenAirlineNames[airline] == true, "Given airline is not registered in the system");
        require(msg.sender == airlines[airline].owner, "Only airline can create insurance");
        require(flightOracle.flightExist(flightOracle.getFlightUniqueId(airline, flightNumber)), "Given flight is not available in Oracle");
        bytes32 id = keccak256(abi.encodePacked(insuree, airline, flightNumber)); 
        require(!insuranceExist(id), "This insurance already exist");
        insurancePolicies[id] = InsurancePolicy(id, 0 , insuree, flightNumber, airline, false, false, false, userAmount, airlineAmount, false);
        userInsuranceIds[insuree].push(id);
        return id;
    }

    function payInAsAirline(bytes32 policyId) public payable returns (bytes32) {
        require(msg.sender ==  airlines[insurancePolicies[policyId].airline].owner, "Only airline owner can call this function");
        require(insurancePolicies[policyId].airlineAmount == msg.value, "Not sufficient amount of Ether");
        insurancePolicies[policyId].insureerPaidIn = true;
        insurancePolicies[policyId].amount = insurancePolicies[policyId].amount + msg.value;
        return policyId;
    }

    function payInAsUser(bytes32 policyId) public payable returns (bytes32) {
        require(msg.sender == insurancePolicies[policyId].insuree, "Only owner of insurance can call this function");
        require(insurancePolicies[policyId].userAmount == msg.value, "Not sufficient amount of Ether");
        insurancePolicies[policyId].userPaidIn = true;
        insurancePolicies[policyId].amount = insurancePolicies[policyId].amount + msg.value;
        return policyId;
    }

    function getUserInsuranceIds(address userAddress) public view returns (bytes32[]) {
        return userInsuranceIds[userAddress];
    }

    function insuranceExist(bytes32 policyId) public view returns (bool) {
        return insurancePolicies[policyId].id  !=  0; 
    }

    function isInsurancePayed(bytes32 policyId) public view returns (bool) {
        return insurancePolicies[policyId].userPaidIn && insurancePolicies[policyId].insureerPaidIn;
    }

    function getInsuranceUser(bytes32 insuranceId) public  {
        require(msg.sender == insurancePolicies[insuranceId].insuree, "Payment can be invoked only by insuree");
        require(insurancePolicies[insuranceId].payedOut == false, "Insurance has been already payed");
        require(insurancePolicies[insuranceId].userPaidIn == true, "User has not paid for the insurance");

        require(flightOracle.isFlightDelayed(flightOracle.getFlightUniqueId(insurancePolicies[insuranceId].airline, insurancePolicies[insuranceId].flightNumber) ), "Flight is not delayed, insurance cannot be paid");
        
        insurancePolicies[insuranceId].insuree.transfer(insurancePolicies[insuranceId].amount);
        insurancePolicies[insuranceId].payedOut = true;
    }

    function getInsuranceBack(bytes32 insuranceId) public  {
        require(msg.sender == airlines[insurancePolicies[insuranceId].airline].owner, "Can be invoked only by owner of the airline");
        require(insurancePolicies[insuranceId].payedOut == false, "Insurance has been already payed");

        require(flightOracle.isFlightFinished(flightOracle.getFlightUniqueId(insurancePolicies[insuranceId].airline, insurancePolicies[insuranceId].flightNumber) ), "Flight is not finished, insurance cannot be paid");
        require(!flightOracle.isFlightDelayed(flightOracle.getFlightUniqueId(insurancePolicies[insuranceId].airline, insurancePolicies[insuranceId].flightNumber) ), "Flight is delayed, insurance cannot be paid");

        airlines[insurancePolicies[insuranceId].airline].owner.transfer(insurancePolicies[insuranceId].amount);

        insurancePolicies[insuranceId].payedOut = true;
    }

    function cancelInusranceIfNotPaid(bytes32 insuranceId) public  {
        require(msg.sender == airlines[insurancePolicies[insuranceId].airline].owner, "Can be invoked only by owner of the airline");
        require(insurancePolicies[insuranceId].payedOut == false, "Insurance has been already payed");
        require(!isInsurancePayed(insuranceId), "Cannot cancel, insurance is already paid by user and insurer");

        if(insurancePolicies[insuranceId].userPaidIn == true) {
            insurancePolicies[insuranceId].insuree.transfer(insurancePolicies[insuranceId].userAmount);
        }

        if(insurancePolicies[insuranceId].insureerPaidIn == true) {
            airlines[insurancePolicies[insuranceId].airline].owner.transfer(insurancePolicies[insuranceId].airlineAmount);
        }

        insurancePolicies[insuranceId].cancelled = true;
    }

    function getPolicy(bytes32 policyId) public view returns (
          bytes32 id,
        uint amount,
        address insuree,
        string flightNumber,
        string airline,
        bool payedOut,
        bool userPaidIn,
        bool insureerPaidIn,
        uint userAmount,
        uint airlineAmount,
        bool cancelled) {
            InsurancePolicy storage policy = insurancePolicies[policyId];
        return (policy.id, policy.amount, policy.insuree, policy.flightNumber, policy.airline, policy.payedOut, policy.userPaidIn, policy.insureerPaidIn, policy.userAmount, policy.airlineAmount, policy.cancelled);
    }
}