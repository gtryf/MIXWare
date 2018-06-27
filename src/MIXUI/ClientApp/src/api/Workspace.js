import ApiBase from './ApiBase';

const RESOURCE_URL = '/api/workspaces';

class Workspace extends ApiBase {
    getAllWorkspaces = () => this.getApiAuth(RESOURCE_URL);
    createWorkspace = (workspace) => this.postApiAuth(RESOURCE_URL, workspace);
    deleteWorkspace = (id) => this.deleteApiAuth(`${RESOURCE_URL}/${id}`);
}

export default Workspace;