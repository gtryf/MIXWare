import { client } from '../Api';

const initialState = [];

const workspacesRequestType = 'WORKSPACES_REQUEST';
const receiveWorkspacesType = 'RECEIVE_WORKSPACES';

const workspacesRequest = () => ({ type: workspacesRequestType });
const receiveWorkspaces = (workspaces) => ({ type: receiveWorkspacesType, workspaces });

export const actions = {
    getWorkspaces: () => (dispatch) => {
        dispatch(workspacesRequest());
        client.getAllWorkspaces()
            .then((resp) => { dispatch(receiveWorkspaces(resp)); });
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