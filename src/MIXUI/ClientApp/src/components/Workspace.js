import React from 'react';
import Header from './Header';
import Loader from './Loader';
import { Grid, Row, Col, PageHeader } from 'react-bootstrap';
import { Controlled as CodeMirror } from 'react-codemirror2';
import { connect } from 'react-redux';
import { actions } from '../store/Workspace';

import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/material.css';

class Workspace extends React.Component {
    state = {
        value: ''
    }

    constructor(props) {
        super(props);

        props.load(props.match.params.workspaceId);
    }

    renderWorkspace = () => {
        if (!this.props.workspace) {
            return null;
        }

        var options = {
			lineNumbers: true,
		};
        return (
            <Grid>
                <PageHeader>
                    {this.props.workspace.name}
                </PageHeader>

                <Grid>
                    <Row>
                        <Col md={2}>
                            File list goes here
                        </Col>
                        <Col md={10}>
                            <Grid>
                                <Row>
                                    <CodeMirror
                                        value={this.state.value}
                                        options={options}
                                        onBeforeChange={(editor, data, value) => {
                                          this.setState({value});
                                        }}
                                        onChange={(editor, data, value) => {
                                        }}
                                    />
                                </Row>

                                <Row>
                                    Simulator goes here
                                </Row>
                            </Grid>
                        </Col>
                    </Row>
                </Grid>
            </Grid>
        );
    }
    
    render() {
        return (
            <div>
                <Loader show={this.props.isFetching} />
                <Header />
                {this.renderWorkspace()}
            </div>
        )
    }
};

function mapStateToProps(state, ownProps) {
    return {
        isFetching: state.workspaces.isFetching,
        workspace: state.workspaces.list.find(item => item.id === ownProps.match.params.workspaceId),
    };
}

function mapDispatchToProps(dispatch) {
    return {
        load: (id) => {
            dispatch(actions.getWorkspace(id));
        } 
    };
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Workspace);