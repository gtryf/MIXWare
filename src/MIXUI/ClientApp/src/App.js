﻿import React from 'react';
import { Route, Switch, Redirect } from 'react-router';
import Workspaces from './components/Workspaces';
import Login from './components/Login';
import Logout from './components/Logout';
import PrivateRoute from './components/PrivateRoute';

export default () => ( 
    <Switch>
        <PrivateRoute path='/workspaces' component={Workspaces} />
        <Route path='/login' component={Login} />
        <Route path='/logout' component={Logout} />
        <Route exact path='/' render={() => (
            <Redirect
                to='/workspaces'
            />
        )} />
    </Switch>
);
