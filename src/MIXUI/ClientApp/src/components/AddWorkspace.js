import './Workspaces.css';
import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { actions } from '../store/Workspace';
import { Col, Panel, Modal, Button, FormGroup, FormControl,HelpBlock, ControlLabel } from 'react-bootstrap';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faPlus);

class AddWorkspace extends React.Component {
    static propTypes = {
        fields: PropTypes.object,
        onSubmit: PropTypes.func.isRequired,
    }

    state = {
        show: false,
        fields: this.props.fields || {
            name: '',
            description: ''
        },
    };

    constructor(props) {
        super(props);

        this.handleShow = this.handleShow.bind(this);
        this.handleClose = this.handleSubmit.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.handleHide = this.handleHide.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.getNameValidationState = this.getNameValidationState.bind(this);
        this.getDescriptionValidationState = this.getDescriptionValidationState.bind(this);
        this.validateForm = this.validateForm.bind(this);
    }

    componentWillReceiveProps(update) {
        this.setState({ fields: update.fields });
    }

    handleSubmit() {
        if (!this.validateForm()) return;

        const workspace = this.state.fields;
        this.props.onSubmit(workspace);
        const fields = this.state.fields;
        fields.name = '';
        fields.description = '';
        this.setState({ show: false, fields });
    }

    handleShow() {
        this.setState({ show: true });
    }

    handleHide() {
        const fields = this.state.fields;
        fields.name = '';
        fields.description = '';
        this.setState({ show: false, fields });
    }

    getNameValidationState() {
        const length = this.state.fields.name.length;
        if (length > 100) return 'error';
        if (length < 5) return 'error';
        return null;
    }

    getDescriptionValidationState() {
        const length = this.state.fields.description.length;
        if (length > 1000) return 'error';
        return null;
    }

    validateForm() {
        return this.getNameValidationState() !== 'error' && this.getDescriptionValidationState() !== 'error';
    }

    handleChange(e) {
        const fields = this.state.fields;
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
                    <Panel.Body className='workspace-overview-body workspace-overview-new-body text-center'>
                        <div className='center-vertical'>
                            <FontAwesomeIcon icon={faPlus} size="4x" onClick={this.handleShow} />
                        </div>
                    </Panel.Body>
                </Panel>

                <Modal show={this.state.show} onHide={this.handleHide}>
                    <Modal.Header closeButton>
                        <Modal.Title>Add a new workspace</Modal.Title>
                    </Modal.Header>
                    <Modal.Body>
                        <form>
                            <FormGroup controlId="name" validationState={this.getNameValidationState()}>
                                <ControlLabel>*Name</ControlLabel>
                                <FormControl
                                    type="text"
                                    value={this.state.fields.name}
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
                                    value={this.state.fields.description}
                                    placeholder="(optional)"
                                    onChange={this.handleChange}
                                />
                                <FormControl.Feedback />
                                <HelpBlock>Maximum 1000 characters</HelpBlock>
                            </FormGroup>
                        </form>
                    </Modal.Body>
                    <Modal.Footer>
                        <Button bsStyle="primary" disabled={!this.validateForm()} onClick={this.handleSubmit}>Save</Button>
                    </Modal.Footer>
                </Modal>
            </Col>
        );
    }
};

function mapDispatchToProps(dispatch) {
    return {
        onSubmit: (workspace) => {
            dispatch(actions.createWorkspace(workspace));
        }
    }
}


export default connect(null, mapDispatchToProps)(AddWorkspace);