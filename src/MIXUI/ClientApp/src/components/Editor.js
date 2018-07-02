import React from 'react';
import { Controlled as CodeMirror } from 'react-codemirror2';
import { Panel, Nav, NavItem } from 'react-bootstrap';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { actions } from '../store/Workspace';

import { library } from '@fortawesome/fontawesome-svg-core';
import { faSave } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/material.css';
import './Editor.css';
import '../codemirror/mode/mixal/mixal';

library.add(faSave);

class Editor extends React.Component {
    state = {
        text: this.props.text || '',
        fileName: this.props.fileName || '[untitled]',
    }

    componentDidMount = () => {
        const { fileId } = this.props.match.params;
        if (fileId) {
            this.props.loadFile(fileId);
        } else {
            this.props.clearFile();
        }
    }

    componentWillReceiveProps = (update) => {
        this.setState({ text: update.text, fileName: update.fileName || '[untitled]' });
    }

    componentDidUpdate = (prevProps) => {
        const { fileId } = this.props.match.params;
        const { fileId: prevFileId } = prevProps.match.params;

        if (fileId && prevFileId !== fileId) {
            this.props.loadFile(fileId);
        } else if (!fileId) {
            this.props.clearFile();
        }
    }

    saveFile = () => {
        const { fileId } = this.props.match.params;
        if (fileId) {
            this.props.updateFile({
                name: this.state.fileName,
                fileContents: this.state.text
            });
        } else {
            let name = prompt('Please enter the file name');
            if (name) {
                this.setState({ fileName: name }, () => {
                    this.props.createFile({
                        name,
                        fileContents: this.state.text
                    });
                });
            }
        }
    }

    renameFile = () => {
        const { fileId } = this.props.match.params;
        let name = prompt('Please enter the file name');
        if (name) {
            this.setState({ fileName: name }, () => {
                if (fileId) {
                    this.props.updateFile({
                        name: this.state.fileName,
                        fileContents: this.state.text
                    });
                }
            });
        }
    }

    render() {
        var options = {
            lineNumbers: true
		};
        return (
            <Panel className='panel-editor'>
                <Panel.Heading>
                    <Nav bsStyle='pills'>
                        <NavItem onClick={this.renameFile}>
                            {this.state.fileName}
                        </NavItem>
                        <NavItem onClick={this.saveFile}>
                            <FontAwesomeIcon icon={faSave} />
                        </NavItem>
                    </Nav>
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
        text: state.workspaces.activeFile.data,
        fileName: state.workspaces.activeFile.name,
    }),
    (dispatch, ownProps) => ({
        loadFile: (id) => {
            dispatch(actions.getFile(ownProps.match.params.workspaceId, id));
        },
        clearFile: () => {
            dispatch(actions.clearFile());
        },
        createFile: (file) => {
            dispatch(actions.createFile(ownProps.match.params.workspaceId, file));
        },
        updateFile: (file) => {
            dispatch(actions.updateFile(ownProps.match.params.workspaceId, ownProps.match.params.fileId, file));
        }
    })
)(Editor));

export default EditorContainer;