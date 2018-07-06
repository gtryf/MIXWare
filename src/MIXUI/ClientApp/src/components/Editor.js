import React from 'react';
import { Controlled as CodeMirror } from 'react-codemirror2';
import { Panel } from 'react-bootstrap';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { actions } from '../store/Workspace';
import FileActions from './FileActions';

import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/material.css';
import './Editor.css';
import '../codemirror/mode/mixal/mixal';

class Editor extends React.Component {
    state = {
        text: this.props.text || '',
    }

    clearState = () => {
        this.setState({
            text: '', 
        });
    }

    componentDidMount = () => {
        const { fileId } = this.props.match.params;
        if (fileId) {
            this.props.loadFile(fileId);
        } else {
            this.clearState();
        }
    }

    componentWillReceiveProps = (update) => {
        this.setState({
            text: update.text, 
        });

        const { fileId: prevFileId } = this.props.match.params;
        const { fileId } = update.match.params;
        if (fileId && prevFileId !== fileId) {
            this.props.loadFile(fileId);
        } else if (!fileId) {
            this.clearState();
        }
    }

    render() {
        var options = {
            lineNumbers: true
		};
        return (
            <Panel className='panel-editor'>
                <Panel.Heading>
                    <FileActions text={this.state.text} />
                </Panel.Heading>
                <Panel.Body>
                    <CodeMirror
                        value={this.state.text}
                        options={options}
                        onBeforeChange={(editor, data, value) => {
                            this.setState({ text: value });
                        }}
                        onChange={(editor, data, value) => {
                        }}
                    />
                </Panel.Body>
            </Panel>
        );
    }
}

const EditorContainer = withRouter(connect(
    (state) => ({
        activeFileId: state.workspaces.activeFile.id,
        text: state.workspaces.activeFile.data,
        fileName: state.workspaces.activeFile.name,
    }),
    (dispatch, ownProps) => ({
        loadFile: (id) => dispatch(actions.getFile(ownProps.match.params.workspaceId, id)),
        createFile: (file) => dispatch(actions.createFile(ownProps.match.params.workspaceId, file)),
        updateFile: (file) => dispatch(actions.updateFile(ownProps.match.params.workspaceId, ownProps.match.params.fileId, file)),
    })
)(Editor));

export default EditorContainer;