import { workspaces } from '../api';

const initialState = [];

const workspacesRequestType = 'WORKSPACES_REQUEST';
const receiveWorkspacesType = 'RECEIVE_WORKSPACES';

const workspacesRequest = () => ({ type: workspacesRequestType });
const receiveWorkspaces = (workspaces) => ({ type: receiveWorkspacesType, workspaces });

export const actions = {
    getWorkspaces: () => (dispatch) => {
        dispatch(workspacesRequest());
        workspaces.getAllWorkspaces()
            .then((resp) => { dispatch(receiveWorkspaces(resp)); });
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

export const reducer = (state, action) => {
    state = state || initialState;

    switch (action.type) {
        case workspacesRequestType:
            return [];
        case receiveWorkspacesType:
            return action.workspaces;
        default:
            return state;
    }
};