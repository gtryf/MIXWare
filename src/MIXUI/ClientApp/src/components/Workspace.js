import React from 'react';
import Header from './Header';

class Workspace extends React.Component {
    render() {
        return (
            <div>
                <Header />
                <h2>Workspace: {this.props.match.params.workspaceId}</h2>
            </div>
        )
    }
};

export default Workspace;