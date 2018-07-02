import React from 'react';
import { Route, Switch, Redirect } from 'react-router';
import Workspaces from './components/Workspaces';
import Workspace from './components/Workspace';
import Login from './components/Login';
import Logout from './components/Logout';
import PrivateRoute from './components/PrivateRoute';

export default () => ( 
    <Switch>
        <PrivateRoute exact path='/workspaces' component={Workspaces} />
        <PrivateRoute path='/workspaces/:workspaceId/:fileId?' component={Workspace} />
        <Route path='/login' component={Login} />
        <Route path='/logout' component={Logout} />
        <Route exact path='/' render={() => (
            <Redirect
                to='/workspaces'
            />
        )} />
    </Switch>
);
