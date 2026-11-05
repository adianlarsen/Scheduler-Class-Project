class WorkSession {
  constructor({ id, taskId, start, end, memberIds = [] }) {
    this.id = id;
    this.taskId = taskId;
    this.start = start;
    this.end = end;
    this.memberIds = memberIds;
  }
}
module.exports = WorkSession;
