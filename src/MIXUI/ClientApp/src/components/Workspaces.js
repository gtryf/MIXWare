import React from 'react';
import { Grid, Row } from 'react-bootstrap';
import Header from './Header';
import WorkspaceOverview from './WorkspaceOverview';

import { client } from '../Api';

class Workspaces extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            workspaces: []
        };
        client.getAllWorkspaces().then(workspaces => {
            console.log(workspaces);
            this.setState({ workspaces });
        });
    }

    render() {
        return (
            <div>
                <Header />
                <Grid>
                    <Row>
                        {this.state.workspaces.map(w => (<WorkspaceOverview key={w.id} name={w.name} fileCount={w.fileCount} />))}
                    </Row>
                </Grid>
            </div>
        );
    };
};

export default Workspaces;
