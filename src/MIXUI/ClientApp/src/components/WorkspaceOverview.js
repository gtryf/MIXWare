import './Workspaces.css';
import React from 'react';
import { Col, Panel } from 'react-bootstrap';

const WorkspaceOverview = (props) => (
    <Col xs={12} sm={4} md={4} lg={4}>
        <Panel>
            <Panel.Heading>
                <Panel.Title componentClass="h3">{props.name}</Panel.Title>
            </Panel.Heading>
            <Panel.Body className='workspace-overview-body'>
                <p>{props.description && props.description.length ? props.description : '[No description available]'}</p>
            </Panel.Body>
            <Panel.Footer>
                File count: {props.fileCount}
            </Panel.Footer>
        </Panel>
    </Col>
);

export default WorkspaceOverview;