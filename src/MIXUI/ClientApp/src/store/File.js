import { combineReducers } from 'redux';
import { workspaces as client } from '../api';

const fileRequestType = 'FILE_REQUEST';
const receiveFileType = 'RECEIVE_FILE';
const clearFileRequestType = 'CLEAR_FILE_REQUEST';

const fileRequest = () => ({ type: fileRequestType });
const receiveFile = (file) => ({ type: receiveFileType, file });
const clearFileRequest = () => ({ type: clearFileRequestType });

export const actions = {
    getFile: (workspaceId, fileId) => (dispatch) => {
        dispatch(fileRequest());
        client.getFile(workspaceId, fileId)
            .then((resp) => { dispatch(receiveFile(resp)); });
    },
    clearFile: clearFileRequest,
};

const file = (state = {}, action) => {
    switch (action.type) {
        case clearFileRequestType:
        case fileRequestType:
            return {};
        case receiveFileType:
            return action.file;
        default:
            return state;
    }
};

const isFetching = (state = false, action) => {
    switch (action.type) {
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
    file,
    isFetching,
});