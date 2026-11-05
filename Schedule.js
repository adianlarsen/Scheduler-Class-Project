CREATE TABLE users (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  email TEXT UNIQUE NOT NULL
);

CREATE TABLE groups (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL
);

CREATE TABLE group_members (
  group_id INTEGER REFERENCES groups(id),
  user_id INTEGER REFERENCES users(id),
  PRIMARY KEY (group_id, user_id)
);

CREATE TABLE schedules (
  id INTEGER PRIMARY KEY,
  user_id INTEGER REFERENCES users(id),
  busy_start INTEGER NOT NULL,
  busy_end INTEGER NOT NULL
);

CREATE TABLE meetings (
  id INTEGER PRIMARY KEY,
  group_id INTEGER REFERENCES groups(id),
  title TEXT,
  start_time INTEGER NOT NULL,
  end_time INTEGER NOT NULL
);

CREATE TABLE tasks (
  id INTEGER PRIMARY KEY,
  group_id INTEGER REFERENCES groups(id),
  name TEXT NOT NULL,
  description TEXT,
  due_date TEXT,
  priority TEXT,
  status TEXT DEFAULT 'to do',
  estimated_time INTEGER,
  meeting_id INTEGER REFERENCES meetings(id)
);

CREATE TABLE task_members (
  task_id INTEGER REFERENCES tasks(id),
  user_id INTEGER REFERENCES users(id),
  PRIMARY KEY (task_id, user_id)
);

CREATE TABLE work_sessions (
  id INTEGER PRIMARY KEY,
  task_id INTEGER REFERENCES tasks(id),
  start_time INTEGER NOT NULL,
  end_time INTEGER NOT NULL
);

CREATE TABLE work_session_members (
  work_session_id INTEGER REFERENCES work_sessions(id),
  user_id INTEGER REFERENCES users(id),
  PRIMARY KEY (work_session_id, user_id)
);
