import { applyMiddleware, combineReducers, compose, createStore } from 'redux';
import { createLogger } from 'redux-logger';
import thunk from 'redux-thunk';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import * as User from './User';
import * as Workspace from './Workspace';

export default function configureStore(history, initialState) {
    const isDevelopment = process.env.NODE_ENV === 'development';

    const reducers = {
        users: User.reducer,
        workspaces: Workspace.reducer,
    };

    const middleware = [
        thunk,
        routerMiddleware(history),
    ];

    if (isDevelopment) {
        middleware.push(createLogger());
    }

    // In development, use the browser's Redux dev tools extension if installed
    const enhancers = [];
    
    if (isDevelopment && typeof window !== 'undefined' && window.devToolsExtension) {
        enhancers.push(window.devToolsExtension());
    }

    const rootReducer = combineReducers({
        ...reducers,
        routing: routerReducer
    });

    return createStore(
        rootReducer,
        initialState,
        compose(applyMiddleware(...middleware), ...enhancers)
    );
}
