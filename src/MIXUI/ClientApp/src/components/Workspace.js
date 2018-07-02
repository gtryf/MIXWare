import React from 'react';
import Header from './Header';
import Loader from './Loader';
import FileList from './FileList';
import Editor from './Editor';
import { Grid, Row, Col, PageHeader } from 'react-bootstrap';
import { connect } from 'react-redux';
import { actions } from '../store/Workspace';

class Workspace extends React.Component {
    componentDidMount = () => {
        this.props.loadWorkspace();
    }

    renderWorkspace = () => {
        if (!this.props.workspace) {
            return null;
        }

        return (
            <Grid>
                <PageHeader>
                    {this.props.workspace.name}
                </PageHeader>

                <Grid>
                    <Row>
                        <Col md={4}>
                            <FileList files={this.props.workspace.files} />
                        </Col>
                        <Col md={8}>
                            <Editor />
                        </Col>
                    </Row>

                    <Row>
                        Simulator goes here
                    </Row>
                </Grid>
            </Grid>
        );
    }
    
    render() {
        return (
            <div>
                <Loader show={this.props.isFetching} />
                <Header />
                {this.renderWorkspace()}
            </div>
        )
    }
};

function mapStateToProps(state, ownProps) {
    return {
        isFetching: state.workspaces.isFetching,
        workspace: state.workspaces.list.find(item => item.id === ownProps.match.params.workspaceId),
        activeFile: state.workspaces.activeFile,
    };
}

function mapDispatchToProps(dispatch, ownProps) {
    return {
        loadWorkspace: () => {
            dispatch(actions.getWorkspace(ownProps.match.params.workspaceId));
        },
    };
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Workspace);