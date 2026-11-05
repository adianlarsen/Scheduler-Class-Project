class Group {
  constructor({ id, name, memberIds = [] }) {
    this.id = id;
    this.name = name;
    this.memberIds = memberIds;
  }
}
module.exports = Group;
