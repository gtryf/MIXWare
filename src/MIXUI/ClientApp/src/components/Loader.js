import React from 'react';
import { Modal } from 'react-bootstrap';

const Loader = (props) => (
    <Modal show={props.show}>
        <Modal.Body>
            <div className="loader center-block" />
        </Modal.Body>
    </Modal>
);

export default Loader;