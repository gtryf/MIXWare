import { combineReducers } from 'redux';
import { workspaces } from '../api';

const workspaceRequestType = 'WORKSPACE_REQUEST';
const workspacesRequestType = 'WORKSPACES_REQUEST';
const receiveWorkspaceType = 'RECEIVE_WORKSPACE';
const receiveWorkspacesType = 'RECEIVE_WORKSPACES';

const workspaceRequest = (id) => ({ type: workspaceRequestType, id });
const workspacesRequest = () => ({ type: workspacesRequestType });
const receiveWorkspace = (workspace) => ({ type: receiveWorkspaceType, workspace });
const receiveWorkspaces = (workspaces) => ({ type: receiveWorkspacesType, workspaces });

export const actions = {
    getWorkspaces: () => (dispatch) => {
        dispatch(workspacesRequest());
        workspaces.getAllWorkspaces()
            .then((resp) => { dispatch(receiveWorkspaces(resp)); });
    },
    getWorkspace: (id) => (dispatch) => {
        dispatch(workspaceRequest(id));
        workspaces.getWorkspace(id)
            .then((resp) => { dispatch(receiveWorkspace(resp)); });
    },
    createWorkspace: (workspace) => (dispatch) => {
        workspaces.createWorkspace(workspace)
            .then(() => {
                dispatch(workspacesRequest());
                workspaces.getAllWorkspaces()
                    .then((resp) => { dispatch(receiveWorkspaces(resp)); });
            });
    },
    updateWorkspace: (id, workspace) => (dispatch) => {
        workspaces.updateWorkspace(id, workspace)
            .then(() => {
                dispatch(workspacesRequest());
                workspaces.getAllWorkspaces()
                    .then((resp) => { dispatch(receiveWorkspaces(resp)); });
            });
    },
    deleteWorkspace: (id) => (dispatch) => {
        workspaces.deleteWorkspace(id)
        .then(() => {
            dispatch(workspacesRequest());
            workspaces.getAllWorkspaces()
                .then((resp) => { dispatch(receiveWorkspaces(resp)); });
        });
    }
};

const list = (state = [], action) => {
    switch (action.type) {
        case workspacesRequestType:
            return [];
        case workspaceRequestType:
            return state.filter(item => item.id !== action.id);
        case receiveWorkspacesType:
            return action.workspaces;
        case receiveWorkspaceType:
            return [...state, action.workspace];
        default:
            return state;
    }
};

const isFetching = (state = false, action) => {
    switch (action.type) {
        case workspacesRequestType:
        case workspaceRequestType:
            return true;
        case receiveWorkspacesType:
        case receiveWorkspaceType:
            return false;
        default:
            return state;
    }
};

export const reducer = combineReducers({
    list,
    isFetching,
});