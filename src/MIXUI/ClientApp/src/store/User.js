import { client } from '../Api';

const loginUserType = 'LOGIN';
const logoutUserType = 'LOGOUT';
const initialState = { user: null }

export const actionCreators = {
    login: (username, password) => ({ type: loginUserType, user: { username, password } }),
    logout: () => ({ type: logoutUserType }),
};

export const reducer = (state, action) => {
    state = state || initialState;

    switch (action.type) {
        case loginUserType:
            client.login(action.user.username, action.user.password);
            if (client.isLoggedIn()) {
                return { ...state, user: action.user };
            } else {
                return { ...state, user: null };
            }
        case logoutUserType:
            client.logout();
            return { ...state, user: null };
        default:
            return state;
    }
};