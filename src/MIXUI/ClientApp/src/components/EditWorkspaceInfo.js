import React from 'react';
import PropTypes from 'prop-types';
import { Modal, Button, FormGroup, FormControl,HelpBlock, ControlLabel } from 'react-bootstrap';

class EditWorkspaceInfo extends React.Component {
    static propTypes = {
        fields: PropTypes.object,
        title: PropTypes.string.isRequired,
        onSubmit: PropTypes.func.isRequired,
        visible: PropTypes.bool.isRequired,
    }

    state = {
        show: this.props.visible,
        fields: this.props.fields || {
            name: '',
            description: '',
        }
    };

    componentWillReceiveProps = (update) => {
        this.setState({ 
            show: update.visible,
            fields: update.fields || {
                name: '',
                description: ''
            }
        });
    }

    handleSubmit = () => {
        if (!this.validateForm()) return;

        const workspace = this.state.fields;
        this.props.onSubmit(workspace);
        const fields = this.state.fields;
        fields.name = '';
        fields.description = '';
        this.setState({ show: false, fields });
    }

    handleHide = () => {
        const fields = this.state.fields;
        fields.name = '';
        fields.description = '';
        this.setState({ show: false, fields });
    }

    getNameValidationState = () => {
        const length = this.state.fields.name.length;
        if (length > 100) return 'error';
        if (length < 5) return 'error';
        return null;
    }

    getDescriptionValidationState = () => {
        const length = this.state.fields.description.length;
        if (length > 1000) return 'error';
        return null;
    }

    validateForm = () => 
        this.getNameValidationState() !== 'error' && this.getDescriptionValidationState() !== 'error';

    handleChange = (e) => {
        const fields = this.state.fields;
        fields[e.target.id] = e.target.value;
        this.setState(fields);
    }

    render = () =>
    (
        <Modal show={this.state.show} onHide={this.handleHide}>
            <Modal.Header closeButton>
                <Modal.Title>{this.props.title}</Modal.Title>
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
    );
}

export default EditWorkspaceInfo;