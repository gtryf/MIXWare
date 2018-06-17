import './Login.css';
import React from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/User';

class Login extends React.Component {
    constructor(props) {
        super(props);

        // reset login status
        this.props.logout();

        this.state = {
            username: '',
            password: '',
        };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(e) {
        const { name, value } = e.target;
        this.setState({ [name]: value });
    }

    handleSubmit(e) {
        e.preventDefault();
        
        const { username, password } = this.state;
        if (username && password) {
            this.props.login(username, password);
        }
    }

    render() {
        const { username, password } = this.state;
        return (
            <div className='ui middle aligned center aligned grid'>
                <div className='column'>
                    <h2 className='ui teal header'>
                        <div className='content'>
                            Login to your account
                        </div>
                    </h2>
                    <form className='ui large form' onSubmit={this.handleSubmit}>
                        <div className='ui stacked segment'>
                            <div className='field'>
                                <div className='ui left icon input'>
                                    <i className='user icon"'></i>
                                    <input type='text' name='username' placeholder='Username' value={username} onChange={this.handleChange} />
                                </div>
                            </div>
                            <div className='field'>
                                <div className='ui left icon input'>
                                    <i className='lock icon'></i>
                                    <input type='password' name='password' placeholder='Password' value={password} onChange={this.handleChange} />
                                </div>
                            </div>
                            <button className='ui fluid large teal submit button'>Login</button>
                        </div>
                    </form>
                </div>
            </div>
        );
    }
};

export default connect(
    state => state.user,
    dispatch => bindActionCreators(actionCreators, dispatch)
  )(Login);