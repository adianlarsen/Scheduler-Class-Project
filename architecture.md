const STATUSES = ["to do", "in progress", "review", "done"];

class Task {
  constructor({ id, groupId, name, description, memberIds = [], date, priority, status = "to do", estimatedTime, meetingId = null }) {
    this.id = id;
    this.groupId = groupId;
    this.name = name;
    this.description = description;
    this.memberIds = memberIds;
    this.date = date;
    this.priority = priority;
    this.status = status;
    this.estimatedTime = estimatedTime;
    this.meetingId = meetingId;
  }
}
Task.STATUSES = STATUSES;
module.exports = Task;
