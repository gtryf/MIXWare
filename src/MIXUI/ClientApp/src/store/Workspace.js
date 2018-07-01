import { combineReducers } from 'redux';
import { workspaces } from '../api';

const workspaceRequestType = 'WORKSPACE_REQUEST';
const workspacesRequestType = 'WORKSPACES_REQUEST';
const receiveWorkspaceType = 'RECEIVE_WORKSPACE';
const receiveWorkspacesType = 'RECEIVE_WORKSPACES';

const updateFileRequestType = 'UPDATE_FILE_REQUEST';
const finishUpdateFileType = 'FINISH_UPDATE_FILE';
const createFileRequestType = 'CREATE_FILE_REQUEST';
const finishCreateFileType = 'FINISH_CREATE_FILE';

const workspaceRequest = (id) => ({ type: workspaceRequestType, id });
const workspacesRequest = () => ({ type: workspacesRequestType });
const receiveWorkspace = (workspace) => ({ type: receiveWorkspaceType, workspace });
const receiveWorkspaces = (workspaces) => ({ type: receiveWorkspacesType, workspaces });

const createFileRequest = () => ({ type: createFileRequestType });
const updateFileRequest = () => ({ type: updateFileRequestType });
const finishCreateFile = (file) => ({ type: finishCreateFileType, file });
const finishUpdateFile = (file) => ({ type: finishUpdateFileType, file });

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
    },


    createFile: (workspaceId, file) => (dispatch) => {
        dispatch(createFileRequest());
        workspaces.createFile(workspaceId, file)
            .then((resp) => { dispatch(finishCreateFile(resp)); })
            .then(() => {
                dispatch(workspaceRequest(workspaceId));
                workspaces.getWorkspace(workspaceId)
                    .then((resp) => { dispatch(receiveWorkspace(resp)); });
            });
    },
    updateFile: (workspaceId, fileId, file) => (dispatch) => {
        dispatch(updateFileRequest());
        workspaces.updateFile(workspaceId, fileId, file)
            .then((resp) => { dispatch(finishUpdateFile(resp)); })
            .then(() => {
                dispatch(workspaceRequest(workspaceId));
                workspaces.getWorkspace(workspaceId)
                    .then((resp) => { dispatch(receiveWorkspace(resp)); });
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
            const workspace = action.workspace;
            workspace.files = workspace.files || [];
            return [...state, workspace];
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