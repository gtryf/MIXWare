import ApiBase from './ApiBase';

const STORAGE_KEY = 'api-user';

class User extends ApiBase {
    constructor() {
        super();

        this.useSessionStorage = (typeof sessionStorage !== 'undefined');

        if (this.useSessionStorage) {
            this.user = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
        }
    }

    isLoggedIn() {
        return !!this.user;
    }

    setUser(user) {
        this.user = user;
        this.setToken(user.token);
        if (this.useSessionStorage) {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(user));
        }
    }

    getUser = () => this.user;

    removeUser() {
        this.user = null;
        if (this.useSessionStorage) {
            sessionStorage.removeItem(STORAGE_KEY);
        }
    }

    login(username, password) {
        const url = '/api/auth/login';
        return this.postApi(url, { username, password }).then(user => {
            this.setUser(user);
            return user;
        });
    }

    logout() {
        this.removeUser();
        this.removeToken();
    }
}

export default User;