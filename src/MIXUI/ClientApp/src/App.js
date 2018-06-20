import React from 'react';
import { Route, Switch } from 'react-router';
import Home from './components/Home';
import Login from './components/Login';
import Logout from './components/Logout';
import Counter from './components/Counter';
import FetchData from './components/FetchData';
import PrivateRoute from './components/PrivateRoute';

export default () => (
    <Switch>
        <PrivateRoute exact path='/' component={Home} />
        <Route path='/login' component={Login} />
        <Route path='/logout' component={Logout} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetchdata/:startDateIndex?' component={FetchData} />
    </Switch>
);
