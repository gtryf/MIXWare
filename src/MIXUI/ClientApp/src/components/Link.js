import React from 'react';
import { withRouter, NavLink } from 'react-router-dom';

const Link = withRouter(function Link(props) {
    const { children, history, to, staticContext, ...rest } = props;
    return (
        history.location.pathname === to ?
            <span>{children}</span>
            :
            <NavLink {...{to, ...rest}}>{children}</NavLink>
    );
});

export default Link;