#!/usr/bin/env node

// Stop Hook — GitHub Copilot
// Migrated from .ai/clineSpecificGlobals/Hooks/TaskComplete
//
// Fires when the agent session ends (Claude "Stop" event / Copilot session close).
// Injects a three-point closing checklist into the agent's final message:
//
//   (1) aiKnowledgeBase.md — capture structural/architectural knowledge gained during
//       the session so future agents start with accurate context.
//   (2) aiWorkplans/ — if the task was tracked in a workplan file, mark all completed
//       steps [x] and flip Status to COMPLETE.
//   (3) cx-code-review — if any .cs, .ts, or .tsx files were modified, trigger the
//       holistic code review workflow before declaring the task done.
//
// Does NOT block the session (no decision: "block") — this is a non-blocking reminder.
// The stop_hook_active guard prevents the hook from firing recursively if the agent
// triggers another stop event while processing this reminder.

const chunks = [];
process.stdin.on("data", (d) => chunks.push(d));
process.stdin.on("end", () => {
    try {
        const input = JSON.parse(Buffer.concat(chunks).toString());

        // Guard: already continuing from a previous stop hook — do not fire again
        if (input.stop_hook_active) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        process.stdout.write(
            JSON.stringify({
                systemMessage:
                    "Before closing this task:\n" +
                    "(1) Update aiReferenceFiles/aiKnowledgeBase.md if any structural or architectural knowledge was gained.\n" +
                    "(2) If tracked in aiWorkplans/, mark all completed steps [x] and set Status: COMPLETE.\n" +
                    "(3) If any .cs, .ts, or .tsx files were modified, run the cx-code-review workflow before closing.",
            }),
        );
    } catch {
        process.stdout.write(JSON.stringify({}));
    }
});
