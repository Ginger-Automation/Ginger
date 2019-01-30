import React, { Component } from 'react';

export class RunSet extends Component {
    static displayName = RunSet.name;

  constructor (props) {
    super(props);
      this.state = {
                      runsets: [],
                      loading: true,                      
                    };

      fetch('api/RunSet/RunSets')
      .then(response => response.json())
      .then(data => {
        this.setState({ forecasts: data, loading: false });
      });
  }

    static runRunSetClick(runSetName) {
         
        fetch('api/RunSet/RunRunSet', {
           method: 'post',
           headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify({ "name": runSetName })

       });            
    }

    static ShowReport() {
        alert("report");
    }
   

  static renderForecastsTable (forecasts) {
    return (
      <table className='table table-striped'>
        <thead>
          <tr>
                    <th>Name</th>                
                    <th>Description</th>                
          </tr>
        </thead>
        <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.name}>
                        <td>{forecast.name}</td>
                        <td>{forecast.description}</td>
                    
                        <td>
                            <button className="btn btn-primary" onClick={this.runRunSetClick.bind(this, forecast.name)}>Run</button>
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
        : RunSet.renderForecastsTable(this.state.forecasts);


      return (
          
          <div>
              <label>Solution folder</label>
              <input type="text" />
              <input type="button" value="Load" />
              <input type="button" value="Report" onClick="{this.ShowReport.bind()}" />


        <h1>Run Set</h1>
              <p>Showing all Run Sets found in solution</p>
              {contents}              
      </div>
    );
  }
}
