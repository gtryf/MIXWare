import * as React from 'react';
import { Route } from 'react-router-dom';
import { PrivateRoute } from './_components';
import { Layout } from './components/Layout';
import Home from './components/Home';

export const routes = <Layout>
    <PrivateRoute exact path='/' component={Home} />
    <Route path='/login' component={Home} />
</Layout>;
