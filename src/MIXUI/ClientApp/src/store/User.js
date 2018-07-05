import { users } from '../api';

const loginRequestType = 'LOGIN_REQUEST';
const loginSuccessType = 'LOGIN_SUCCESS';
const loginFailureType = 'LOGIN_FAILURE';
const logoutUserType = 'LOGOUT';
const initialState = {
    loginFormData: {
        username: '',
        password: '',
    },
    isLoading: false,
    isFailed: false,
    isLoggedIn: users.isLoggedIn(),
    currentUser: users.getUser(),
}

const loginRequest = () => ({ type: loginRequestType });
const loginSuccess = (userInfo) => ({ type: loginSuccessType });
const loginFailure = (error) => ({ type: loginFailureType, error });

export const actions = {
    login: (username, password) => (dispatch) => {
        dispatch(loginRequest());
        return users.login(username, password)
            .then((resp) => dispatch(loginSuccess(resp)))
            .catch((err) => dispatch(loginFailure(err)));
    },
    logout: () => {
        users.logout();
        return { type: logoutUserType };
    }
};

export const reducer = (state, action) => {
    state = state || initialState;

    switch (action.type) {
        case loginRequestType:
            return { ...state, isLoading: true, isFailed: false, isLoggedIn: false, currentUser: null };
        case loginSuccessType:
            return { ...state, isLoading: false, isFailed: false, isLoggedIn: users.isLoggedIn(), currentUser: users.getUser() };
        case loginFailureType:
            return { ...state, isLoading: false, isFailed: true, isLoggedIn: false, currentUser: null };
        case logoutUserType:
            return {
                ...state,
                isLoading: false,
                isFailed: false,
                isLoggedIn: false,
                currentUser: null,
                loginFormData: {
                    username: '',
                    password: '',
                }
            };
        default:
            return state;
    }
};