import React from 'react';
import { Row, Col, Nav, NavItem, Modal, Form, FormGroup, ControlLabel, Radio, Checkbox, Button } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { actions } from '../store/Workspace';

import { submissions as client } from '../api';

import { library } from '@fortawesome/fontawesome-svg-core';
import { faSave, faFile, faMicrochip } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faSave, faFile, faMicrochip);

const Menu = (props) => (
    <Nav bsStyle='pills'>
        <NavItem onClick={props.rename} title="Rename">
            {props.activeFile.name || '[untitled]'}
        </NavItem>
        <LinkContainer to={`/workspaces/${props.workspaceId}`}>
            <NavItem title="New">
                <FontAwesomeIcon icon={faFile} />
            </NavItem>
        </LinkContainer>
        <NavItem onClick={props.save} title="Save">
            <FontAwesomeIcon icon={faSave} />
        </NavItem>
        <NavItem title="Assemble" onClick={props.showAssembler}>
            <FontAwesomeIcon icon={faMicrochip} />
        </NavItem>
    </Nav>
);

class AssembleWindow extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            fields: {
                type: 'binary',
                produceSymbolTable: false,
                produceListing: false,
            }
        };

        this.handleSetType = this.handleSetType.bind(this);
        this.handleToggleOutput = this.handleToggleOutput.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleSetType(type) {
        const fields = this.state.fields;
        fields.type = type;
        this.setState(fields);
    }

    handleToggleOutput(type) {
        const fields = this.state.fields;
        fields[type] = !fields[type];
        this.setState(fields);
    }

    handleSubmit() {
        const { visible, handleSubmit } = this.props;
        if (visible) {
            handleSubmit(this.state.fields);
        }
    }

    render() {
        const { visible, handleHide } = this.props;
        return (
            <Modal show={visible} onHide={handleHide}>
                <Modal.Header closeButton>
                    <Modal.Title>Assemble</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Row>
                            <Col sm={6}>
                                <FormGroup>
                                    <ControlLabel>Compile to</ControlLabel>
                                    <Radio name='outputType' checked={this.state.fields.type === 'binary'} onChange={() => this.handleSetType('binary')}>
                                        Binary
                                    </Radio>
                                    <Radio name='outputType' checked={this.state.fields.type === 'card'} onChange={() => this.handleSetType('card')}>
                                        Card
                                    </Radio>
                                </FormGroup>
                            </Col>
                            <Col sm={6}>
                                <FormGroup>
                                    <ControlLabel>Options</ControlLabel>
                                    <Checkbox checked={this.state.fields.produceSymbolTable} onClick={() => this.handleToggleOutput('produceSymbolTable')}>
                                        Symbol file
                                    </Checkbox>
                                    <Checkbox checked={this.state.fields.produceListing} onClick={() => this.handleToggleOutput('produceListing')}>
                                        Program listing
                                    </Checkbox>
                                </FormGroup>
                            </Col>
                        </Row>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button bsStyle="primary" onClick={this.handleSubmit}>Assemble</Button>
                </Modal.Footer>
            </Modal>
        );
    }
}

class FileActions extends React.Component {
    constructor(props) {
        super(props);

        this.state = { showAssembler: false };
        this.handleShowAssembler = this.handleAssemblerVisibility(true).bind(this);
        this.handleHideAssembler = this.handleAssemblerVisibility(false).bind(this);
    }

    handleAssemblerVisibility(visibility) {
        return function () {
            this.setState({ showAssembler: visibility });
        }
    }

    render() {
        return (
            <div>
                <Menu {...this.props} showAssembler={this.handleShowAssembler} />
                <AssembleWindow visible={this.state.showAssembler} handleHide={this.handleHideAssembler} handleSubmit={this.props.assemble} />
            </div>
        );
    }
}

function mapStateToProps(state, ownProps) {
    return {
        activeFile: state.workspaces.activeFile,
        workspaceId: ownProps.match.params.workspaceId,
        fileId: ownProps.match.params.fileId,
        text: ownProps.text,
    }
}

function mapDispatchToProps(dispatch, ownProps) {
    return {
        createFile: (file) => dispatch(actions.createFile(ownProps.match.params.workspaceId, file)),
        updateFile: (file) => dispatch(actions.updateFile(ownProps.match.params.workspaceId, ownProps.match.params.fileId, file)),
    }
}

function mergeProps(stateProps, dispatchProps) {
    return {
        ...stateProps,
        rename: () => {
            if (!stateProps.fileId || !stateProps.workspaceId) { return; }
            const name = prompt('Please enter the file name');
            if (name) {
                const update = {
                    name,
                    fileContents: stateProps.activeFile.data,
                }
                dispatchProps.updateFile(update);
            }
        },
        save: () => {
            if (!stateProps.workspaceId) { return; }
            const fileContents = stateProps.text;
            if (!fileContents) { return; }
            if (stateProps.fileId) {
                dispatchProps.updateFile({ name: stateProps.activeFile.name, fileContents });
            } else {
                const name = prompt('Please enter the file name');
                if (name) {
                    dispatchProps.createFile({ name, fileContents });
                }
            }
        },

        assemble: ({ type, produceSymbolTable, produceListing }) => {
            if (!stateProps.fileId || !stateProps.workspaceId) { return; }
            const submission = {
                fileId: stateProps.fileId,
                type,
                produceSymbolTable,
                produceListing,
                prettyPrinter: 'plain'
            }
            client.createSubmission(submission);
        }
    }
}

const FileActionsContainer = withRouter(connect(
    mapStateToProps,
    mapDispatchToProps,
    mergeProps)(FileActions));

export default FileActionsContainer;