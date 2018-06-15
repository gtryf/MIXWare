import * as React from 'react';
import { Route, Redirect, RouteProps } from 'react-router-dom';

const PrivateRoute: React.SFC<RouteProps> = ({ component, ...rest }) => (
    <Route {...rest} render={props => (
        //fakeAuth.isAuthenticated ? React.createElement(component! as React.SFC<any>, props) : (
            <Redirect to={{
                pathname: '/login',
                state: { from: props.location }
            }} />
        //)
    )} />
);

export { PrivateRoute };