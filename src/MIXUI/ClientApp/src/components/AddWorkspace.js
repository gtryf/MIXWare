import './Workspaces.css';
import React from 'react';
import { Col, Panel, Modal, Button } from 'react-bootstrap';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faPlus);

class AddWorkspace extends React.Component {
    constructor(props, context) {
        super(props, context);

        this.handleShow = this.handleShow.bind(this);
        this.handleClose = this.handleClose.bind(this);

        this.state = {
            show: false
        };
    }

    handleClose() {
        this.setState({ show: false });
    }

    handleShow() {
        this.setState({ show: true });
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
                        <p>Here will be controls.</p>
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