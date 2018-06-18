import React from 'react';
import { connect } from 'react-redux';
import { Header as H, Image, Segment } from 'semantic-ui-react';

const Header = props => (
    <Segment padded>
        <H as='h2' textAlign='right'>
            <Image circular src='images/avatar.jpg' /> {props.username}
        </H>
    </Segment>
);

function mapStateToProps(state) {
    return {
        username: state.users.currentUser.username,
    };
}

export default connect(mapStateToProps)(Header);