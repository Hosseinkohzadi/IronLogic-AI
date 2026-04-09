#!/usr/bin/env node

// PreCompact Hook — GitHub Copilot (new — no Cline equivalent)
//
// Fires before conversation context is compacted (auto or manual).
// Reminds the agent to checkpoint the active aiWorkplans/ file before truncation,
// implementing the globalRules.md Context Window rule:
// "Checkpoint the active workplan before compressing — task must remain resumable."

const chunks = [];
process.stdin.on("data", (d) => chunks.push(d));
process.stdin.on("end", () => {
    try {
        process.stdout.write(
            JSON.stringify({
                systemMessage:
                    "[Context Compaction Imminent] Before context is truncated:\n" +
                    "(1) In the active aiWorkplans/ file, mark all completed steps [x] and note current state so the task can resume cleanly.\n" +
                    "(2) Note any critical file paths, patterns, or decisions discovered so far — these will not survive compaction.\n" +
                    "(3) Confirm the workplan is updated and the next unchecked step is clear before compaction proceeds.",
            }),
        );
    } catch {
        process.stdout.write(JSON.stringify({}));
    }
});
