import './Workspaces.css';
import React from 'react';
import { Col, Panel, Modal, Button, FormGroup, FormControl,HelpBlock, ControlLabel } from 'react-bootstrap';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faPlus);

class AddWorkspace extends React.Component {
    constructor(props, context) {
        super(props, context);

        this.handleShow = this.handleShow.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.getNameValidationState = this.getNameValidationState.bind(this);
        this.getDescriptionValidationState = this.getDescriptionValidationState.bind(this);

        this.state = {
            show: false,
            form: {
                name: '',
                description: ''
            },
        };
    }

    handleClose() {
        this.setState({ show: false });
    }

    handleShow() {
        this.setState({ show: true });
    }

    getNameValidationState() {
        const length = this.state.form.name.length;
        if (length > 100) return 'error';
        if (length < 5) return 'error';
        return null;
    }

    getDescriptionValidationState() {
        const length = this.state.form.description.length;
        if (length > 1000) return 'error';
        return null;
    }

    handleChange(e) {
        const fields = this.state.form;
        fields[e.target.id] = e.target.value;
        this.setState(fields);
    }

    render() {
        return (
            <Col xs={12} sm={4} md={4} lg={4}>
                <Panel>
                    <Panel.Heading>
                        <Panel.Title componentClass="h3"><em>New Workspace</em></Panel.Title>
                    </Panel.Heading>
                    <Panel.Body className='workspace-overview-body text-center'>
                        <div className='center-vertical'>
                            <FontAwesomeIcon icon={faPlus} size="4x" onClick={this.handleShow} />
                        </div>
                    </Panel.Body>
                    <Panel.Footer>&nbsp;</Panel.Footer>
                </Panel>

                <Modal show={this.state.show} onHide={this.handleClose}>
                    <Modal.Header closeButton>
                        <Modal.Title>Add a new workspace</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <form>
                            <FormGroup controlId="name" validationState={this.getNameValidationState()}>
                                <ControlLabel>*Name</ControlLabel>
                                <FormControl
                                    type="text"
                                    value={this.state.form.name}
                                    placeholder="Enter text"
                                    onChange={this.handleChange}
                                />
                                <FormControl.Feedback />
                                <HelpBlock>Between 5 and 100 characters</HelpBlock>
                            </FormGroup>
                            <FormGroup controlId="description" validationState={this.getDescriptionValidationState()}>
                                <ControlLabel>Description</ControlLabel>
                                <FormControl
                                    componentClass="textarea"
                                    value={this.state.form.description}
                                    placeholder="(optional)"
                                    onChange={this.handleChange}
                                />
                                <FormControl.Feedback />
                                <HelpBlock>Maximum 1000 characters</HelpBlock>
                            </FormGroup>
                        </form>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button onClick={this.handleClose}>Close</Button>
                    </Modal.Footer>
                </Modal>
            </Col>
        );
    }
};

export default AddWorkspace;