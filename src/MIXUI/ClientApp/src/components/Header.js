import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { Nav, Navbar, NavDropdown, MenuItem, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

const Header = props => (
    <header>
        <Navbar inverse collapseOnSelect>
            <Navbar.Header>
                <Navbar.Brand>
                    <Link to='/'>MIXWare</Link>
                </Navbar.Brand>
                <Navbar.Toggle />
            </Navbar.Header>
            <Navbar.Collapse>
                <Nav>
                    <LinkContainer to='about'>
                        <NavItem eventKey={1}>About</NavItem>
                    </LinkContainer>
                    <LinkContainer to='workspaces'>
                        <NavItem eventKey={2}>Workspaces</NavItem>
                    </LinkContainer>
                </Nav>
                <Nav pullRight>
                    <NavDropdown eventKey={1} title={'Hello ' + props.username} id="basic-nav-dropdown">
                        <LinkContainer to="/logout">
                            <MenuItem eventKey={1.1}>Logout</MenuItem>
                        </LinkContainer> 
                    </NavDropdown>
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    </header>
);

function mapStateToProps(state) {
    return {
        username: state.users.currentUser.username,
    };
}

export default connect(mapStateToProps)(Header);