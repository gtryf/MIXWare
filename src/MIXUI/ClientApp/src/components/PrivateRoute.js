import React from 'react';
import { Route, Redirect } from 'react-router-dom';

import { users } from '../api';

const PrivateRoute = ({ component, ...rest }) => (
  <Route {...rest} render={(props) => (
    users.isLoggedIn() ? (
      React.createElement(component, props)
    ) : (
      <Redirect to={{
        pathname: '/login',
      }} />
    )
  )} />
);

export default PrivateRoute;
