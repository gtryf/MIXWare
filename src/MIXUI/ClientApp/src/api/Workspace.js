import ApiBase from './ApiBase';

const RESOURCE_URL = '/api/workspaces';

class Workspace extends ApiBase {
    getAllWorkspaces = () => this.getApiAuth(RESOURCE_URL);
    getWorkspace = (id) => this.getApiAuth(`${RESOURCE_URL}/${id}`);
    createWorkspace = (workspace) => this.postApiAuth(RESOURCE_URL, workspace);
    updateWorkspace = (id, workspace) => this.putApiAuth(`${RESOURCE_URL}/${id}`, workspace)
    deleteWorkspace = (id) => this.deleteApiAuth(`${RESOURCE_URL}/${id}`);
}

export default Workspace;