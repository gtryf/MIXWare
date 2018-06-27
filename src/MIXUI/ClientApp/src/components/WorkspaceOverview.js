import './Workspaces.css';
import React from 'react';
import { Col, Panel, Button, Glyphicon } from 'react-bootstrap';
import { actions } from '../store/Workspace';
import { connect } from 'react-redux';

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
                <Button className="pull-right" bsStyle="danger" bsSize="small" onClick={() => props.onWorkspaceDelete(props.id)}>
                    <Glyphicon glyph="trash" />
                </Button>
                <div className="clearfix"></div>
            </Panel.Footer>
        </Panel>
    </Col>
);

export default connect(
    null,
    (dispatch) => ({
        onWorkspaceDelete: (id) => {
            dispatch(actions.deleteWorkspace(id));
        }
    })
)(WorkspaceOverview);