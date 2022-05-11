import React, { Component } from "react";
import SimpleStorageContract from "./contracts/SimpleStorage.json";
import getWeb3 from "./getWeb3";

import "./App.css";

class App extends Component {
  state = {
    storageValue: 0,
    web3: null,
    accounts: null,
    contract: null,
    insurancePolicyId: null,
    policyOwner: null,
    flightNumber: null,
    airline: null,
    isPayed: null,
    isAirlineSet: false,
    isError: false,
    flightDelayed: false,
    flightFinished: false,
    insureerPaidIn: false,
    userPaidIn: false,
  };
  contractAddress = "0xb4F47Ea4082035EfAf682dA5a3a88216A6b34B76";

  componentDidMount = async () => {
    try {
      // Get network provider and web3 instance.
      const web3 = await getWeb3();

      // Use web3 to get the user's accounts.
      const accounts = await web3.eth.getAccounts();

      // Get the contract instance.
      const networkId = await web3.eth.net.getId();
      const deployedNetwork = SimpleStorageContract.networks[networkId];
      console.log(deployedNetwork);

      const instance = new web3.eth.Contract(
        SimpleStorageContract.abi,
        this.contractAddress
      );

      // Set web3, accounts, and contract to the state, and then proceed with an
      // example of interacting with the contract's methods.
      this.setState({ web3, accounts, contract: instance });
    } catch (error) {
      // Catch any errors for any of the above operations.
      alert(
        `Failed to load web3, accounts, or contract. Check console for details.`
      );
      console.error(error);
    }
  };

  handleChange = (event) => {
    this.setState({ insurancePolicyId: event.target.value });
  };

  handleSubmit = async (event) => {
    event.preventDefault();
    this.setState({ isAirlineSet: false, isError: false });
    const { accounts, contract } = this.state;
    console.log(this.state.accounts[0]);
    console.log(this.state.policyOwner);

    try {
      // Stores a given value, 5 by default.
      const res = await contract.methods
        .getPolicy(this.state.insurancePolicyId)
        .call();

      if (res[4] == "") throw Error();
      this.setState({
        airline: res[4],
        flightNumber: res[3],
        policyOwner: res[2],
        isPayed: res[5],
        insureerPaidIn: res[7],
        userPaidIn: res[6],
      });
      this.setState({ isAirlineSet: true });
      event.preventDefault();

      fetch(`https://localhost:44349/flight/${res[4]}/${res[3]}`)
        .then((response) => {
          console.log(response);
          return response.json();
        })
        .then((data) =>
          this.setState({
            flightDelayed: data.isDelayed,
            flightFinished: data.isFinished,
          })
        );
    } catch (error) {
      console.log(error);

      alert("Wprowadzono niepoprawne ID");
    }
  };

  pay = async () => {
    const { accounts, contract } = this.state;
    if (!this.state.flightFinished) {
      alert("Lot się jeszcze nie zakonczyl, ubezpieczenie nie przysluguje.");
      return;
    }

    if (!this.state.flightDelayed) {
      alert("Lot nie byl opozniony. Ubezpieczenie nie przysluguje.");
      return;
    }

    try {
      const res = await contract.methods
        .getInsuranceUser(this.state.insurancePolicyId)
        .send({ from: this.state.accounts[0] });
    } catch (error) {
      alert("Ubezpieczenie nie przysługuje" + error);
    }
  };

  render() {
    if (!this.state.web3) {
      return <div>Loading Web3, accounts, and contract...</div>;
    }
    return (
      <div className="App">
        <h1>Przeglądarka ubezpieczeń</h1>
        <p>Wprowadź ID polisy</p>
        <form onSubmit={this.handleSubmit}>
          <label>
            <input
              class="form-control"
              type="text"
              value={this.state.value}
              onChange={this.handleChange}
            />
          </label>
          <input class="btn btn-primary" type="submit" value="Sprawdź" />
        </form>
        {this.state.isAirlineSet ? (
          <div>
            {" "}
            <h2>Linia lotniczna: {this.state.airline}</h2>
            <h2>Numer lotu: {this.state.flightNumber}</h2>
            <h2>Wlasciciel polisy: {this.state.policyOwner}</h2>
            <h2>
              {this.state.isPayed ? (
                <a>Polisa wypłacona</a>
              ) : (
                <a>Polisa niewypłacona</a>
              )}
              <br></br>
              {this.state.flightFinished ? (
                <a>
                  Lot się odbył i{" "}
                  {this.state.flightDelayed ? (
                    <a> się opóźnił </a>
                  ) : (
                    <a>się nie opóźnił</a>
                  )}
                </a>
              ) : (
                <a>Lot się jeszcze nie zakończył</a>
              )}
            </h2>
            {this.state.accounts[0] == this.state.policyOwner &&
            !this.state.isPayed ? (
              <button type="button" class="btn btn-primary" onClick={this.pay}>
                Wypłać
              </button>
            ) : (
              ""
            )}
          </div>
        ) : (
          ""
        )}
      </div>
    );
  }
}

export default App;
