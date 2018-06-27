import './Workspaces.css';
import React from 'react';
import EditWorkspaceInfo from './EditWorkspaceInfo';
import { Row, Col, Panel, Button, Glyphicon } from 'react-bootstrap';
import { actions } from '../store/Workspace';
import { connect } from 'react-redux';
import { LinkContainer } from 'react-router-bootstrap';

class WorkspaceOverview extends React.Component {
    state = {
        modalVisible: false,
    };

    handleShowModal = () => {
        this.setState({ modalVisible: true });
    }

    render = () =>
    (
        <Col xs={12} sm={4} md={4} lg={4}>
            <Panel>
                <Panel.Heading>
                    <Panel.Title componentClass="h3">{this.props.name}</Panel.Title>
                </Panel.Heading>
                <Panel.Body className='workspace-overview-body'>
                    <div className="description">
                        <p>{this.props.description && this.props.description.length ? this.props.description : '[No description available]'}</p>
                    </div>
                    <div className="action-open">
                        <LinkContainer to={`/workspaces/${this.props.id}`}>
                            <Button bsStyle="primary" bsSize="large">Open</Button>
                        </LinkContainer>
                    </div>
                </Panel.Body>
                <Panel.Footer>
                    <Row>
                        <Col xs={8}>
                            File count: {this.props.fileCount}
                        </Col>
                        <Col xs={4}>
                            <div className="btn-toolbar pull-right">
                                <Button bsStyle="primary" bsSize="small" onClick={this.handleShowModal}>
                                    <Glyphicon glyph="edit" />
                                </Button>
                                <Button bsStyle="danger" bsSize="small" onClick={() => this.props.onWorkspaceDelete(this.props.id)}>
                                    <Glyphicon glyph="trash" />
                                </Button>
                            </div>
                        </Col>
                    </Row>
                </Panel.Footer>
            </Panel>

            <EditWorkspaceInfo 
                title='Edit Workspace' 
                onSubmit={this.props.onFormSubmit(this.props.id)} 
                visible={this.state.modalVisible} 
                fields={{ name: this.props.name, description: this.props.description }}
            />
        </Col>
    );
};

export default connect(
    null,
    (dispatch) => ({
        onWorkspaceDelete: (id) => {
            dispatch(actions.deleteWorkspace(id));
        },
        onFormSubmit: (id) => (workspace) => {
            dispatch(actions.updateWorkspace(id, workspace));
        }
    })
)(WorkspaceOverview);