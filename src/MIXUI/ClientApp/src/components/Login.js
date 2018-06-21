import './Login.css';
import PropTypes from 'prop-types';
import React from 'react';
import { Button, FormGroup, FormControl, ControlLabel, Alert, Row, Grid, Col } from 'react-bootstrap';
import { Redirect } from 'react-router';
import { connect } from 'react-redux';
import { actions } from '../store/User';

class Login extends React.Component {
    static propTypes = {
        isLoading: PropTypes.bool.isRequired,
        errorMessage: PropTypes.string,
        fields: PropTypes.object,
        isLoggedIn: PropTypes.bool.isRequired,
        onSubmit: PropTypes.func.isRequired,
    };

    state = {
        fields: this.props.fields || {
            username: '',
            password: '',
        },
        isLoggedIn: this.props.isLoggedIn,
    };

    componentWillReceiveProps(update) {
        this.setState({ fields: update.fields });
    }

    validateForm = () => {
        const credentials = this.state.fields;

        if (!credentials.username) return false;
        if (!credentials.password) return false;

        return true;
    };

    onFormSubmit = (evt) => {
        const credentials = this.state.fields;
        evt.preventDefault();

        if (!this.validateForm()) return;

        this.props.onSubmit(credentials);
    };

    onInputChange = (evt) => {
        const fields = this.state.fields;

        fields[evt.target.id] = evt.target.value;

        this.setState({ fields });
    };

    render() {
        if (this.props.isLoggedIn) {
            return <Redirect to='/' />;
        }
        return (
            <Grid>
                <Row>
                    <Col mdOffset={5} md={3}>
                        <form className="form-login" onSubmit={this.onFormSubmit}>
                            <h4>Log in to your account</h4>
                            <FormGroup controlId="username" bsSize="large">
                                <ControlLabel>Username</ControlLabel>
                                <FormControl
                                    autoFocus
                                    type="text"
                                    value={this.state.fields.username}
                                    onChange={this.onInputChange}
                                />
                            </FormGroup>
                            <FormGroup controlId="password" bsSize="large">
                                <ControlLabel>Password</ControlLabel>
                                <FormControl
                                    value={this.state.fields.password}
                                    onChange={this.onInputChange}
                                    type="password"
                                />
                            </FormGroup>
                            <div className="wrapper">
                                <Button block bsSize="large" disabled={!this.validateForm()} type="submit">Login</Button>
                            </div>
                        </form>
                    </Col>
                </Row>
                {
                    this.props.isFailed ?
                        <Row>
                            <Col mdOffset={5} md={3}>
                                <Alert bsStyle="danger">
                                    <p>Login failed</p>
                                </Alert>
                            </Col>
                        </Row> :
                        null
                }
            </Grid>
        );
    }
};

function mapStateToProps(state) {
    return {
        isLoading: state.users.isLoading,
        fields: state.users.loginFormData,
        isFailed: state.users.isFailed,
        isLoggedIn: state.users.isLoggedIn,
    }
}

function mapDispatchToProps(dispatch) {
    return {
        onSubmit: (credentials) => {
            dispatch(actions.login(credentials.username, credentials.password));
        }
    }
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Login);