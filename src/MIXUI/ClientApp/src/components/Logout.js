import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Redirect } from 'react-router-dom';
import { connect } from 'react-redux';
import { actionCreators } from '../store/User';

class Logout extends Component {
    static propTypes = {
        handleLogout: PropTypes.func.isRequired,
    };

    constructor(props) {
        super(props);

        this.props.handleLogout();
    }

    render() {
        return (
            <Redirect
                to='/login'
            />
        );
    }
}

function mapStateToProps(state) {
    return {};
}

function mapDispatchToProps(dispatch) {
    return {
        handleLogout: () => {
            dispatch(actionCreators.logout());
        }
    }
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Logout);