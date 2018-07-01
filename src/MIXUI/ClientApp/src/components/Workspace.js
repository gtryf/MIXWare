import React from 'react';
import Header from './Header';
import Loader from './Loader';
import FileList from './FileList';
import Editor from './Editor';
import { Grid, Row, Col, PageHeader } from 'react-bootstrap';
import { connect } from 'react-redux';
import { actions } from '../store/Workspace';

class Workspace extends React.Component {
    componentWillMount = () => {
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
                            <FileList workspaceId={this.props.workspace.id} files={this.props.workspace.files} onFileSelected={this.loadFile} />
                        </Col>
                        <Col md={8}>
                            <Editor workspaceId={this.props.match.params.workspaceId} fileId={this.props.match.params.fileId} />
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
        activeFile: state.files.file,
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