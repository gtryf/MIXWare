import React from 'react';
import { connect } from 'react-redux';

const Header = props => (
    <div>
        {props.username}
    </div>
);

function mapStateToProps(state) {
    return {
        username: state.users.currentUser.username,
    };
}

export default connect(mapStateToProps)(Header);