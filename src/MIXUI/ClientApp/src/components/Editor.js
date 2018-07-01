import React from 'react';
import { Controlled as CodeMirror } from 'react-codemirror2';
import { Panel, Nav, NavItem } from 'react-bootstrap';
import { connect } from 'react-redux';
import { actions as workspaceActions } from '../store/Workspace';
import { actions as fileActions } from '../store/File';

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
        text: this.props.text || ''
    }

    componentDidMount = () => {
        if (this.props.fileId) {
            this.props.loadFile(this.props.fileId);
        } else {
            this.props.clearFile();
        }
    }

    componentWillReceiveProps = (update) => {
        this.setState({ text: update.text });
        if (update.fileId && this.props.fileId !== update.fileId) {
            this.props.loadFile(update.fileId);
        } else if (!update.fileId) {
            this.props.clearFile();
        }
    }

    saveFile = () => {
        if (this.props.fileId) {
            this.props.updateFile(this.props.fileId, {
                name: this.props.fileName,
                fileContents: this.state.text
            });
        } else {
            let name = prompt('Please enter the file name');
            if (name) {
                this.props.createFile({
                    name,
                    fileContents: this.state.text
                });
            }
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
                        <NavItem>
                            {this.props.fileName ? this.props.fileName : '[untitled]'}
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

const EditorContainer = connect(
    (state) => ({
        text: state.files.file.data,
        fileName: state.files.file.name,
    }),
    (dispatch, ownProps) => ({
        loadFile: (id) => {
            dispatch(fileActions.getFile(ownProps.workspaceId, id));
        },
        clearFile: () => {
            dispatch(fileActions.clearFile());
        },
        createFile: (file) => {
            dispatch(workspaceActions.createFile(ownProps.workspaceId, file));
        },
        updateFile: (fileId, file) => {
            dispatch(workspaceActions.updateFile(ownProps.workspaceId, fileId, file));
        }
    })
)(Editor);

export default EditorContainer;