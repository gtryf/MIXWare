import React from 'react';
import { Col, Panel } from 'react-bootstrap';

const WorkspaceOverview = (props) => (
    <Col xs={12} sm={4} md={4} lg={4}>
        <Panel>
            <Panel.Heading>
                <Panel.Title componentClass="h3">{props.name}</Panel.Title>
            </Panel.Heading>
            <Panel.Body>
                <p>File count: {props.fileCount}</p>
            </Panel.Body>
        </Panel>
    </Col>
);

export default WorkspaceOverview;