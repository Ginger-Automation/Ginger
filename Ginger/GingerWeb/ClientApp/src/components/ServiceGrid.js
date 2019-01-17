import React, { Component } from 'react';

export class ServiceGrid extends Component {
    static displayName = ServiceGrid.name;

    constructor(props) {
        super(props);
        this.state = { gingerNodes: [], loading: true };

        fetch('api/ServiceGrid/NodeList')
            .then(response => response.json())
            .then(data => {
                this.setState({ gingerNodes: data, loading: false });
            });
    }



    static renderNodesTable(gingerNodes) {
        return (
            <table className='table table-striped'>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Service ID</th>
                        <th>Session ID</th>                        
                        <th>Actions count</th>                        
                    </tr>
                </thead>
                <tbody>
                    {gingerNodes.map(gingerNodeInfo =>
                        <tr key={gingerNodeInfo.name}>
                            <td>{gingerNodeInfo.name}</td>
                            <td>{gingerNodeInfo.serviceId}</td>
                            <td>{gingerNodeInfo.sessionID}</td>                            
                            <td>{gingerNodeInfo.actionCount}</td>                            
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : ServiceGrid.renderNodesTable(this.state.gingerNodes);

        return (

            <div>
                <label>Service Grid</label>

                <input type="button" value="Refresh" />

                <h1>Services Grid</h1>
                <p>Showing all Services connect to local Ginger Grid</p>
                {contents}
            </div>
        );
    }
}
