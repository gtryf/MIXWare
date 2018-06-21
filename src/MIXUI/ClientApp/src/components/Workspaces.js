import React from 'react';
import { connect } from 'react-redux';
import { Grid, Row } from 'react-bootstrap';
import { actions } from '../store/Workspace';
import Header from './Header';
import WorkspaceOverview from './WorkspaceOverview';

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
                        {this.props.workspaces.map(w => (<WorkspaceOverview key={w.id} name={w.name} fileCount={w.fileCount} />))}
                    </Row>
                </Grid>
            </div>
        );
    };
};

export default connect(
    (state) => ({ workspaces: state.workspaces }),
    actions
)(Workspaces);
