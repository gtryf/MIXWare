import React from 'react';
import Header from './Header';
import Loader from './Loader';
import FileList from './FileList';
import { Grid, Row, Col, PageHeader } from 'react-bootstrap';
import { Controlled as CodeMirror } from 'react-codemirror2';
import { connect } from 'react-redux';
import { actions as worskpaceActions } from '../store/Workspace';
import { actions as fileActions } from '../store/File';

import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/material.css';
import '../codemirror/mode/mixal/mixal';

class Workspace extends React.Component {
    state = {
        value: this.props.activeFile.data || ''
    }

    constructor(props) {
        super(props);

        props.loadWorkspace();
    }

    componentWillReceiveProps = (update) => {
        this.setState({ value: update.activeFile.data });
    }

    loadFile = (fileId) => {
        this.props.loadFile(fileId);
    }

    renderWorkspace = () => {
        if (!this.props.workspace) {
            return null;
        }

        var options = {
            lineNumbers: true
		};
        return (
            <Grid>
                <PageHeader>
                    {this.props.workspace.name}
                </PageHeader>

                <Grid>
                    <Row>
                        <Col md={4}>
                            <FileList files={this.props.workspace.files} onFileSelected={this.loadFile} />
                        </Col>
                        <Col md={8}>
                            <CodeMirror
                                value={this.state.value}
                                options={options}
                                onBeforeChange={(editor, data, value) => {
                                    this.setState({ value });
                                }}
                                onChange={(editor, data, value) => {
                                }}
                            />
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
            dispatch(worskpaceActions.getWorkspace(ownProps.match.params.workspaceId));
        },
        loadFile: (id) => {
            dispatch(fileActions.getFile(ownProps.match.params.workspaceId, id));
        }
    };
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Workspace);