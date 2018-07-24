import ApiBase from './ApiBase';

const RESOURCE_URL = '/api/submissions';

class Submission extends ApiBase {
    createSubmission = (submission) => this.postApiAuth(RESOURCE_URL, submission);
    getSubmission = (id) => this.getApiAuth(`${RESOURCE_URL}/${id}`);
}

export default Submission;