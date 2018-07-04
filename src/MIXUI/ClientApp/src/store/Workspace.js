import { combineReducers } from 'redux';
import { workspaces as client } from '../api';

const workspaceRequestType = 'WORKSPACE_REQUEST';
const workspacesRequestType = 'WORKSPACES_REQUEST';
const receiveWorkspaceType = 'RECEIVE_WORKSPACE';
const receiveWorkspacesType = 'RECEIVE_WORKSPACES';

const createWorkspaceRequestType = 'CREATE_WORKSPACE_REQUEST';
const updateWorkspaceRequestType = 'UPDATE_WORKSPACE_REQUEST';
const deleteWorkspaceRequestType = 'DELETE_WORKSPACE_REQUEST';
const finishCreateWorkspaceType = 'FINISH_CREATE_WORKSPACE'
const finishUpdateWorkspaceType = 'FINISH_UPDATE_WORKSPACE'
const finishDeleteWorkspaceType = 'FINISH_DELETE_WORKSPACE';

const createFileRequestType = 'CREATE_FILE_REQUEST';
const updateFileRequestType = 'UPDATE_FILE_REQUEST';
const deleteFileRequestType = 'DELETE_FILE_REQUEST';
const finishCreateFileType = 'FINISH_CREATE_FILE';
const finishUpdateFileType = 'FINISH_UPDATE_FILE';
const finishDeleteFileType = 'FINISH_DELETE_FILE';

const fileRequestType = 'FILE_REQUEST';
const receiveFileType = 'RECEIVE_FILE';
const clearFileRequestType = 'CLEAR_FILE_REQUEST';

const workspaceRequest = (id) => ({ type: workspaceRequestType, id });
const workspacesRequest = () => ({ type: workspacesRequestType });
const receiveWorkspace = (workspace) => ({ type: receiveWorkspaceType, workspace });
const receiveWorkspaces = (workspaces) => ({ type: receiveWorkspacesType, workspaces });

const createWorkspaceRequest = () => ({ type: createWorkspaceRequestType });
const updateWorkspaceRequest = () => ({ type: updateWorkspaceRequestType });
const deleteWorkspaceRequest = () => ({ type: deleteWorkspaceRequestType });
const finishCreateWorkspace = (workspace) => ({ type: finishCreateWorkspaceType, workspace });
const finishUpdateWorkspace = (workspace) => ({ type: finishUpdateWorkspaceType, workspace });
const finishDeleteWorkspace = (id) => ({ type: finishDeleteWorkspaceType, id });

const createFileRequest = () => ({ type: createFileRequestType });
const updateFileRequest = () => ({ type: updateFileRequestType });
const deleteFileRequest = () => ({ type: deleteFileRequestType });
const finishCreateFile = (file) => ({ type: finishCreateFileType, file });
const finishUpdateFile = (file) => ({ type: finishUpdateFileType, file });
const finishDeleteFile = (workspaceId, fileId) => ({ type: finishDeleteFileType, workspaceId, fileId });

const fileRequest = () => ({ type: fileRequestType });
const receiveFile = (file) => ({ type: receiveFileType, file });
const clearFileRequest = () => ({ type: clearFileRequestType });

export const actions = {
    getWorkspaces: () => (dispatch) => {
        dispatch(workspacesRequest());
        return client.getAllWorkspaces()
            .then((resp) => dispatch(receiveWorkspaces(resp)));
    },
    getWorkspace: (id) => (dispatch) => {
        dispatch(workspaceRequest(id));
        return client.getWorkspace(id)
            .then((resp) => dispatch(receiveWorkspace(resp)));
    },
    createWorkspace: (workspace) => (dispatch) => {
        dispatch(createWorkspaceRequest());
        return client.createWorkspace(workspace)
            .then((resp) => dispatch(finishCreateWorkspace(resp)));
    },
    updateWorkspace: (id, workspace) => (dispatch) => {
        dispatch(updateWorkspaceRequest());
        return client.updateWorkspace(id, workspace)
            .then((resp) => dispatch(finishUpdateWorkspace(resp)));
    },
    deleteWorkspace: (id) => (dispatch) => {
        dispatch(deleteWorkspaceRequest());
        return client.deleteWorkspace(id)
            .then(() => dispatch(finishDeleteWorkspace(id)));
    },

    getFile: (workspaceId, fileId) => (dispatch) => {
        dispatch(fileRequest());
        return client.getFile(workspaceId, fileId)
            .then((resp) => dispatch(receiveFile(resp)));
    },
    clearFile: clearFileRequest,
    createFile: (workspaceId, file) => (dispatch) => {
        dispatch(createFileRequest());
        return client.createFile(workspaceId, file)
            .then((resp) => dispatch(finishCreateFile(resp)));
    },
    updateFile: (workspaceId, fileId, file) => (dispatch) => {
        dispatch(updateFileRequest());
        return client.updateFile(workspaceId, fileId, file)
            .then((resp) => dispatch(finishUpdateFile(resp)));
    },
    deleteFile: (workspaceId, fileId) => (dispatch) => {
        dispatch(deleteFileRequest());
        return client.deleteFile(workspaceId, fileId)
            .then(() => dispatch(finishDeleteFile(workspaceId, fileId)));
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
            const index = state.findIndex(x => x.id === action.workspace.id);
            if (index < 0) {
                return [...state, workspace];
            }
            return state.map((item, idx) => {
                if (idx !== index) {
                    return item;
                } else {
                    return {
                        ...item,
                        ...workspace
                    };
                }
            });

        case finishCreateWorkspaceType:
            return [...state, action.workspace];
        case finishDeleteWorkspaceType:
            return state.filter(x => x.id !== action.id);
        case finishUpdateWorkspaceType:
            return state.map(item => {
                if (item.id !== action.workspace.id) {
                    return item;
                } else {
                    return action.workspace;
                }
            })

        case finishUpdateFileType:
        case finishCreateFileType:
            const file = action.file;
            const workspaceIdx = state.findIndex(x => x.id === file.workspaceId);
            if (workspaceIdx < 0) {
                return state;
            }
            return state.map((item, idx) => {
                if (idx !== workspaceIdx) {
                    return item;
                } else {
                    const fileIdx = item.files.findIndex(x => x.id === file.id);
                    if (fileIdx < 0) {
                        return {
                            ...item,
                            files: [...item.files, file],
                        }
                    } else {
                        return {
                            ...item,
                            files: item.files.map((f, i) => {
                                if (i !== fileIdx) {
                                    return f;
                                } else {
                                    return {
                                        ...f,
                                        ...file
                                    };
                                }
                            }),
                        }
                    }
                }
            });
        case finishDeleteFileType:
            return state.map(workspace => {
                if (workspace.id !== action.workspaceId) {
                    return workspace;
                }
                return {
                    ...workspace,
                    files: workspace.files.filter(x => x.id !== action.fileId),
                }
            })
        default:
            return state;
    }
};

const activeFile = (state = {}, action) => {
    switch (action.type) {
        case clearFileRequestType:
        case fileRequestType:
            return {};
        case finishCreateFileType:
        case finishUpdateFileType:
        case receiveFileType:
            return action.file;
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

        case clearFileRequestType:
            return false;
        case fileRequestType:
            return true;
        case receiveFileType:
            return false;

        default:
            return state;
    }
};

export const reducer = combineReducers({
    list,
    activeFile,
    isFetching,
});