import React from 'react';
import { Nav, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { actions } from '../store/Workspace';

import { library } from '@fortawesome/fontawesome-svg-core';
import { faSave, faFile, faMicrochip } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faSave, faFile, faMicrochip);

const FileActions = (props) => (
    <Nav bsStyle='pills'>
        <NavItem onClick={props.rename} title="Rename">
            {props.activeFile.name || '[untitled]'}
        </NavItem>
        <LinkContainer to={`/workspaces/${props.workspaceId}`}>
            <NavItem title="New">
                <FontAwesomeIcon icon={faFile} />
            </NavItem>
        </LinkContainer>
        <NavItem onClick={props.save} title="Save">
            <FontAwesomeIcon icon={faSave} />
        </NavItem>
        <NavItem title="Assemble">
            <FontAwesomeIcon icon={faMicrochip} />
        </NavItem>
    </Nav>
);

function mapStateToProps(state, ownProps) {
    return {
        activeFile: state.workspaces.activeFile,
        workspaceId: ownProps.match.params.workspaceId,
        fileId: ownProps.match.params.fileId,
        text: ownProps.text,
    }
}

function mapDispatchToProps(dispatch, ownProps) {
    return {
        createFile: (file) => dispatch(actions.createFile(ownProps.match.params.workspaceId, file)),
        updateFile: (file) => dispatch(actions.updateFile(ownProps.match.params.workspaceId, ownProps.match.params.fileId, file)),
    }
}

function mergeProps(stateProps, dispatchProps) {
    return {
        ...stateProps,
        rename: () => {
            if (!stateProps.fileId || !stateProps.workspaceId) { return; }
            const name = prompt('Please enter the file name');
            if (name) {
                const update = {
                    name,
                    fileContents: stateProps.activeFile.data,
                }
                dispatchProps.updateFile(update);
            }
        },
        save: () => {
            if (!stateProps.workspaceId) { return; }
            const fileContents = stateProps.text;
            if (!fileContents) { return; }
            if (stateProps.fileId) {
                dispatchProps.updateFile({ name: stateProps.activeFile.name, fileContents });
            } else {
                const name = prompt('Please enter the file name');
                if (name) {
                    dispatchProps.createFile({ name, fileContents });
                }
            }
        }
    }
}

const FileActionsContainer = withRouter(connect(
    mapStateToProps,
    mapDispatchToProps,
    mergeProps)(FileActions));

export default FileActionsContainer;