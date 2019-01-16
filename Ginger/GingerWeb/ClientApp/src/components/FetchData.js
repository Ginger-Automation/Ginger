import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor (props) {
    super(props);
    this.state = { forecasts: [], loading: true };

    fetch('api/SampleData/WeatherForecasts')
      .then(response => response.json())
      .then(data => {
        this.setState({ forecasts: data, loading: false });
      });
  }

  static renderForecastsTable (forecasts) {
    return (
      <table className='table table-striped'>
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>filename</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map(forecast =>
            <tr key={forecast.name}>
              <td>{forecast.name}</td>
              <td>{forecast.description}</td>
                        <td>{forecast.fielName}</td>
                        <td>
                            <input type="button" value="Run"/>
                        </td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render () {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderForecastsTable(this.state.forecasts);

      return (
          
          <div>
              <label>Solution folder</label>
              <input type="text" />
              <input type="button" value="Load" />

        <h1>Business Flows</h1>
        <p>Showing all business flows found in solution</p>
        {contents}
      </div>
    );
  }
}
