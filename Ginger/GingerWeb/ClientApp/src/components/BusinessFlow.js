import React, { Component } from 'react';

export class BusinessFlow extends Component {
    static displayName = BusinessFlow.name;

  constructor (props) {
    super(props);
      this.state = {
                      forecasts: [],
                      loading: true,
                      BFStatus: { status: "zaa" }
                    };

      fetch('api/BusinessFlow/BusinessFlows')
      .then(response => response.json())
      .then(data => {
        this.setState({ forecasts: data, loading: false });
      });
  }

   static runFlowClick(flowName) {
         
       fetch('api/BusinessFlow/RunBusinessFlow', {
           method: 'post',
           headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify({ "name": flowName })

       })
           ;
            //.then(response => response.json())
            //.then(data => {
            //    this.setState({ BFStatus: data });
            //});              
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
                <th>filename</th>
                <th>Status</th>
                <th>run</th>
          </tr>
        </thead>
        <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.name}>
                    <td>{forecast.name}</td>
                    <td>{forecast.description}</td>
                    <td>{forecast.fileName}</td>
                    <td>{forecast.status}</td>
                
                        <td>
                            <button className="btn btn-primary" onClick={this.runFlowClick.bind(this, forecast.name)}>Run</button>
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
        : BusinessFlow.renderForecastsTable(this.state.forecasts);

      let bfstatus = (this.state.BFStatus.status);

      return (
          
          <div>
              <label>Solution folder</label>
              <input type="text" />
              <input type="button" value="Load" />
              <input type="button" value="Report" onClick="{this.ShowReport.bind()}" />
              <label>{bfstatus}</label>

        <h1>Business Flows</h1>
        <p>Showing all business flows found in solution</p>
              {contents}              
      </div>
    );
  }
}
