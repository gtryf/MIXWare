import './Login.css';
import PropTypes from 'prop-types';
import React from 'react';
import { Redirect } from 'react-router';
import { connect } from 'react-redux';
import { actionCreators } from '../store/User';
import Field from './Field';

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
        fieldErrors: {},
        isLoggedIn: this.props.isLoggedIn,
    };

    componentWillReceiveProps(update) {
        this.setState({ fields: update.fields });
    }

    validate = () => {
        const credentials = this.state.fields;
        const fieldErrors = this.state.fieldErrors;
        const errMessages = Object.keys(fieldErrors).filter((k) => fieldErrors[k]);

        if (!credentials.username) return true;
        if (!credentials.password) return true;
        if (errMessages.length) return true;

        return false;
    };

    onFormSubmit = (evt) => {
        const credentials = this.state.fields;
        evt.preventDefault();
        
        if (this.validate()) return;

        this.props.onSubmit(credentials);
    };

    onInputChange = ({ name, value, error }) => {
        const fields = this.state.fields;
        const fieldErrors = this.state.fieldErrors;

        fields[name] = value;
        fieldErrors[name] = error;

        this.setState({ fields, fieldErrors });
    };

    render() {
        if (this.props.isLoggedIn) {
            return <Redirect to='/' />;
        }
        return (
            <div className='holder'>
                <div className='ui middle aligned center aligned grid'>
                    <div className='column'>
                        <h2 className='ui teal header'>
                            <div className='content'>
                                Login to your account
                        </div>
                        </h2>
                        <form className={this.props.isLoading ? 'ui large loading form' : 'ui large form'} onSubmit={this.onFormSubmit}>
                            <div className='ui stacked segment'>
                                <Field
                                    placeholder='Username'
                                    name='username'
                                    type='text'
                                    value={this.state.fields.username}
                                    onChange={this.onInputChange}
                                    validate={(val) => val ? false : 'Username required'} />
                                <Field
                                    placeholder='Password'
                                    name='password'
                                    type='password'
                                    value={this.state.fields.password}
                                    onChange={this.onInputChange}
                                    validate={(val) => val ? false : 'Password required'} />

                                <button className='ui fluid large teal submit button'>Login</button>
                            </div>
                            {this.props.isFailed ? <div className="ui message">Login failed</div> : null}
                        </form>
                    </div>
                </div>
            </div>
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
            dispatch(actionCreators.login(credentials.username, credentials.password));
        }
    }
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Login);