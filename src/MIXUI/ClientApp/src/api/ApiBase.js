const STORAGE_KEY = 'api-token';

class ApiBase {
    constructor() {
        if (typeof sessionStorage === 'undefined') {
            const error = new Error('Cannot handle this kind of browser');
            console.error(error);
            throw error;
        }
    };

    setToken(token) {
        sessionStorage.setItem(STORAGE_KEY, token);
    }

    getToken = () => sessionStorage.getItem(STORAGE_KEY);

    removeToken() {
        sessionStorage.removeItem(STORAGE_KEY);
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
        if (!this.getToken()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        return fetch(url, {
            method: 'post',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
                authorization: `Bearer ${this.getToken()}`,
            },
            body: JSON.stringify(body),
        }).then(checkStatus).then(response => {
            if (response.status === 202) {
                console.log(response.headers);
                return {};
            } else {
                return response.json()
            }
        });
    }

    getApiAuth(url, params = {}) {
        if (!this.getToken()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
        return fetch(url, {
            method: 'get',
            headers: {
                accept: 'application/json',
                authorization: `Bearer ${this.getToken()}`,
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
        if (!this.getToken()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));
        return fetch(url, {
            method: 'delete',
            headers: {
                accept: 'application/json',
                authorization: `Bearer ${this.getToken()}`,
            },
        }).then(checkStatus);
    }

    putApiAuth(url, body) {
        if (!this.getToken()) {
            const error = new Error('Unauthorized');
            console.error(error);
            throw error;
        }
        return fetch(url, {
            method: 'put',
            headers: {
                accept: 'application/json',
                'content-type': 'application/json',
                authorization: `Bearer ${this.getToken()}`,
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