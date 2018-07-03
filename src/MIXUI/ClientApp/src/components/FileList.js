import React from 'react';
import { Table, Well } from 'react-bootstrap';
import { withRouter } from 'react-router-dom';
import Link from './Link';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCode, faFileArchive, faTrash, faDownload } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

library.add(faCode, faFileArchive, faTrash, faDownload);

const SourceFile = (props) => (
    <tr>
        <td className="text-muted text-right">{props.index}</td>
        <td className="text-left">
            <FontAwesomeIcon icon={faCode} />&nbsp;
            <Link to={`/workspaces/${props.workspaceId}/${props.file.id}`}>
                <strong>{props.file.name}</strong>
            </Link>
        </td>
        <td className="text-right">{(props.file.size / 1024.0).toFixed(2)}KB</td>
        <td className="text-right">
            <a role="button" onClick={() => props.onFileDelete(props.file.id)}>
                <FontAwesomeIcon icon={faTrash} />
            </a>
        </td>
    </tr>
);

const OtherFile = (props) => (
    <tr>
        <td className="text-muted text-right">{props.index}</td>
        <td className="text-left"><FontAwesomeIcon icon={faFileArchive} />&nbsp;<strong>{props.file.name}</strong></td>
        <td className="text-right">{(props.file.size / 1024.0).toFixed(2)}KB</td>
        <td className="text-right">
            <a role="button">
                <FontAwesomeIcon icon={faDownload} />
            </a>
            <a role="button" onClick={() => props.onFileDelete(props.file.id)}>
                <FontAwesomeIcon icon={faTrash} />
            </a>
        </td>
    </tr>
);

const FileList = (props) => (
    <div>
        {
            props.files && props.files.length ?
            <NonemptyFileList {...props} /> :
            <EmptyFileList {...props} />
        }
    </div>
)

const EmptyFileList = () => (
    <Well>
        <h3>Nothing here</h3>
        <p>No files in this workspace.</p>
        <p>Get started by using the editor to create one!</p>
    </Well>
)

const NonemptyFileList = (props) => (
    <Table striped responsive bordered condensed>
        <thead>
            <tr>
                <th className="text-right">#</th>
                <th className="col-sm-8 text-left">Name</th>
                <th className="col-sm-2 text-right">Size</th>
                <th className="col-sm-2">&nbsp;</th>
            </tr>
        </thead>
        <tfoot>
            <tr>
                <td colSpan={4}>
                    <small className="pull-left text-muted" dir="ltr">{props.files.length} file(s)</small>
                </td>
            </tr>
        </tfoot>
        <tbody>
            {props.files.map((file, index) => (
                (file.type === 'Source' || file.type === 'Deck') ?
                    <SourceFile workspaceId={props.match.params.workspaceId} key={file.id} file={file} index={index + 1} onFileDelete={props.onFileDelete} /> :
                    <OtherFile key={file.id} file={file} index={index + 1} onFileDelete={props.onFileDelete} />
            ))}
        </tbody>
    </Table>
);

export default withRouter(FileList);