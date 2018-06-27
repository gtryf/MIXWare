const STORAGE_KEY = 'api-token';

class ApiBase {
    constructor() {
        this.useSessionStorage = (typeof sessionStorage !== 'undefined');

        if (this.useSessionStorage) {
            this.token = sessionStorage.getItem(STORAGE_KEY);
        }
    };

    setToken(token) {
        this.token = token;

        if (this.useSessionStorage) {
            sessionStorage.setItem(STORAGE_KEY, token);            
        }
    }

    removeToken() {
        this.token = null;
        
        if (this.useSessionStorage) {
            sessionStorage.removeItem(STORAGE_KEY);
        }
    }

    postApi(url, body) {
        return fetch(url, {
            method: 'post',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
            },
            body: JSON.stringify(body),
        }).then(checkStatus).then(response => response.json());
    }

    postApiAuth(url, body) {
        if (!this.token) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        return fetch(url, {
            method: 'post',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
                authorization: `Bearer ${this.token}`,
            },
            body: JSON.stringify(body),
        }).then(checkStatus).then(response => response.json());
    }

    getApiAuth(url, params = {}) {
        if (!this.token) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
        return fetch(url, {
            method: 'get',
            headers: {
                accept: 'application/json',
                authorization: `Bearer ${this.token}`,
            },
        }).then(checkStatus).then(response => response.json());
    }

    getApi(url, params = {}) {
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
        return fetch(url, {
            method: 'get',
            headers: {
                accept: 'application/json',
            },
        }).then(checkStatus).then(response => response.json());
    }

    deleteApiAuth(url, params = {}) {
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
        return fetch(url, {
            method: 'delete',
            headers: {
                accept: 'application/json',
                authorization: `Bearer ${this.token}`,
            },
        }).then(checkStatus);
    }

    putApiAuth(url, body) {
        if (!this.token) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        return fetch(url, {
            method: 'put',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
                authorization: `Bearer ${this.token}`,
            },
            body: JSON.stringify(body),
        }).then(checkStatus).then(response => response.json());
    }
}

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

export default ApiBase;