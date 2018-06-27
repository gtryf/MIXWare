const STORAGE_KEY = 'api-user';

class Api {
    constructor() {
        this.useSessionStorage = (typeof sessionStorage !== 'undefined');

        if (this.useSessionStorage) {
            this.user = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
        }
    };

    isLoggedIn() {
        return !!this.user;
    }

    setUser(user) {
        this.user = user;
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
        return fetch(url, {
            method: 'post',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
            },
            body: JSON.stringify({
                username,
                password,
            }),
        }).then(checkStatus).then(response => response.json()).then(user => {
            this.setUser(user);
            return user;
        });
    }

    logout() {
        this.removeUser();
    }

    getAllWorkspaces() {
        if (!this.isLoggedIn()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        const url = '/api/workspaces';
        return fetch(url, {
            method: 'get',
            headers: {
                accept: 'application/json',
                authorization: `Bearer ${this.user.token}`
            },
        }).then(checkStatus).then(response => response.json());
    }

    createWorkspace(workspace) {
        if (!this.isLoggedIn()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        const url = '/api/workspaces';
        return fetch(url, {
            method: 'post',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
                authorization: `Bearer ${this.user.token}`
            },
            body: JSON.stringify(workspace),
        }).then(checkStatus).then(response => response.json());
    }
};

function checkStatus(response) {
    if (response.status >= 200 && response.status < 300) {
        return response;
    } else {
        const error = new Error(`HTTP Error ${response.statusText}`);
        error.status = response.statusText;
        error.response = response;
        console.error(error);
        throw error;
    }
}

export const client = new Api();