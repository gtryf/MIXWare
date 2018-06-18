import React from 'react';
import { connect } from 'react-redux';
import Header from './Header';

const Home = props => (
  <Header />
);

export default connect()(Home);
