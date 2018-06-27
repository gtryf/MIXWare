import React from 'react';
import { connect } from 'react-redux';
import { Grid, Row } from 'react-bootstrap';
import { actions } from '../store/Workspace';
import Header from './Header';
import WorkspaceOverview from './WorkspaceOverview';
import AddWorkspace from './AddWorkspace';

class Workspaces extends React.Component {
    componentDidMount() {
        this.props.getWorkspaces();
    }

    render() {
        return (
            <div>
                <Header />
                <Grid>
                    <Row>
                        <AddWorkspace />
                        {this.props.workspaces.map(w => <WorkspaceOverview key={w.id} id={w.id} name={w.name} description={w.description} fileCount={w.fileCount} />)}
                    </Row>
                </Grid>
            </div>
        );
    }
}

export default connect(
    (state) => ({ workspaces: state.workspaces }),
    actions
)(Workspaces);
