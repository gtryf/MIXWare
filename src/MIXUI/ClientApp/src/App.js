import React from 'react';
import { Route } from 'react-router';
import Home from './components/Home';
import Login from './components/Login';
import Counter from './components/Counter';
import FetchData from './components/FetchData';
import PrivateRoute from './components/PrivateRoute';

export default () => (
  <div>
    <PrivateRoute exact path='/' component={Home} />
    <Route path='/login' component={Login} />
    <Route path='/counter' component={Counter} />
    <Route path='/fetchdata/:startDateIndex?' component={FetchData} />
  </div>
);
