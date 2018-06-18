import PropTypes from 'prop-types';
import React from 'react';

export default class Field extends React.Component {
    static propTypes = {
        placeholder: PropTypes.string,
        type: PropTypes.string.isRequired,
        name: PropTypes.string.isRequired,
        value: PropTypes.string,
        validate: PropTypes.func,
        onChange: PropTypes.func.isRequired,
    };

    state = {
        value: this.props.value,
        error: false,
    };

    componentWillReceiveProps(update) {
        this.setState({ value: update.value });
    }

    onChange = (evt) => {
        const name = this.props.name;
        const value = evt.target.value;
        const error = this.props.validate ? this.props.validate(value) : false;

        this.setState({ value, error });

        this.props.onChange({ name, value, error });
    };

    render() {
        let fieldClass = this.state.error ? 'field error' : 'field';
        return (
            <div className={fieldClass}>
                <input
                    type={this.props.type}
                    placeholder={this.props.placeholder}
                    value={this.state.value}
                    onChange={this.onChange}
                />
            </div>
        );
    }
};