import { client } from '../Api';

const loginRequestType = 'LOGIN_REQUEST';
const loginSuccessType = 'LOGIN_SUCCESS';
const loginFailureType = 'LOGIN_FAILURE';
const logoutUserType = 'LOGOUT';
const initialState = {
    loginFormData: {
        username: '',
        password: '',
    },
    currentUser: client.getUser(),
    isLoading: false,
    isFailed: false,
    isLoggedIn: client.isLoggedIn()
}

const loginRequest = () => ({ type: loginRequestType });
const loginSuccess = (userInfo) => ({ type: loginSuccessType });
const loginFailure = (error) => ({ type: loginFailureType, error });

export const actionCreators = {
    login: (username, password) => (dispatch) => {
        dispatch(loginRequest());
        client.login(username, password)
            .then((resp) => { dispatch(loginSuccess(resp)); })
            .catch((err) => { dispatch(loginFailure(err)); });
    },
    logout: () => ({ type: logoutUserType }),
};

export const reducer = (state, action) => {
    state = state || initialState;

    switch (action.type) {
        case loginRequestType:
            return { ...state, isLoading: true, isLoggedIn: false, isFailed: false, currentUser: null };
        case loginSuccessType:
            return { ...state, isLoading: false, isLoggedIn: true, isFailed: false, currentUser: client.getUser() };
        case loginFailureType:
            return { ...state, isLoading: false, isFailed: true, isLoggedIn: false, currentUser: null };
        case logoutUserType:
            return { ...state, isLoading: false, isLoggedIn: false, isFailed: false, currentUser: null };
        default:
            return state;
    }
};