// Application layer: schedules, meetings, and common availability.

// Core logic: given each member's busy blocks, return the free blocks shared by everyone.
// busyByMember: [ [ {start, end}, ... ], ... ]   (start/end as numbers, e.g. minutes in the week)
// range: { start, end } window to search within
function findCommonAvailability(busyByMember, range) {
  const allBusy = busyByMember
    .flat()
    .sort((a, b) => a.start - b.start);

  const free = [];
  let cursor = range.start;
  for (const block of allBusy) {
    if (block.start > cursor) free.push({ start: cursor, end: Math.min(block.start, range.end) });
    cursor = Math.max(cursor, block.end);
    if (cursor >= range.end) break;
  }
  if (cursor < range.end) free.push({ start: cursor, end: range.end });
  return free;
}

exports.findCommonAvailability = findCommonAvailability;

exports.importSchedule = (req, res) => res.status(501).json({ message: "Not implemented" });
exports.addMeeting = (req, res) => res.status(501).json({ message: "Not implemented" });
exports.getCommonAvailability = (req, res) => res.status(501).json({ message: "Not implemented" });
