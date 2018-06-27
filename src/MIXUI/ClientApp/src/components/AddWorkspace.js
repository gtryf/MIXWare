import './Workspaces.css';
import React from 'react';
import EditWorkspaceInfo from './EditWorkspaceInfo';
import { connect } from 'react-redux';
import { actions } from '../store/Workspace';
import { Col, Panel } from 'react-bootstrap';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faPlus);

class AddWorkspace extends React.Component {
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
                    <Panel.Title componentClass="h3"><em>New Workspace</em></Panel.Title>
                </Panel.Heading>
                <Panel.Body className='workspace-overview-body workspace-overview-new-body text-center'>
                    <div className='center-vertical'>
                        <FontAwesomeIcon icon={faPlus} size="4x" onClick={this.handleShowModal} />
                    </div>
                </Panel.Body>
            </Panel>

            <EditWorkspaceInfo
                title='Add a new Workspace' 
                onSubmit={this.props.onFormSubmit} 
                visible={this.state.modalVisible} 
            />
        </Col>
    );
};

function mapDispatchToProps(dispatch) {
    return {
        onFormSubmit: (workspace) => {
            dispatch(actions.createWorkspace(workspace));
        }
    }
}

export default connect(null, mapDispatchToProps)(AddWorkspace);