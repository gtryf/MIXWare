import React from 'react';
import { connect } from 'react-redux';
import Header from './Header';

const Workspaces = props => (
  <Header />
);

export default connect()(Workspaces);
