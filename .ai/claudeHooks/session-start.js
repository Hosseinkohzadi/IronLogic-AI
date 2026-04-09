#!/usr/bin/env node

// SessionStart Hook — GitHub Copilot (new — no Cline equivalent)
//
// Fires at the start of every new agent session.
// Checks aiWorkplans/ for any in-progress workplan and injects it as context,
// implementing the globalRules.md Work Plans rule:
// "At task start, check aiWorkplans/ for an in-progress plan and resume from the first unchecked step."

const fs = require("fs");
const path = require("path");

const chunks = [];
process.stdin.on("data", (d) => chunks.push(d));
process.stdin.on("end", () => {
    try {
        const input = JSON.parse(Buffer.concat(chunks).toString());
        const cwd = input.cwd ?? process.cwd();
        const workplansDir = path.join(cwd, "aiWorkplans");

        if (!fs.existsSync(workplansDir)) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        const inProgress = fs
            .readdirSync(workplansDir)
            .filter((f) => f.endsWith(".md"))
            .map((f) => ({
                name: f,
                content: fs.readFileSync(path.join(workplansDir, f), "utf8"),
            }))
            .filter((f) => /^Status:\s*IN PROGRESS/m.test(f.content));

        if (inProgress.length === 0) {
            process.stdout.write(JSON.stringify({}));
            return;
        }

        // If multiple plans are in progress, list them all so the agent knows
        const planList = inProgress
            .map((p) => `- aiWorkplans/${p.name}`)
            .join("\n");

        // Use the first in-progress plan as the primary context to inject
        const plan = inProgress[0];

        // Cap injected plan content to avoid burning session context budget
        const MAX_PLAN_LINES = 80;
        const planLines = plan.content.split("\n");
        const truncated = planLines.length > MAX_PLAN_LINES;
        const planContent = truncated
            ? planLines.slice(0, MAX_PLAN_LINES).join("\n") +
              `\n\n[Truncated — ${planLines.length - MAX_PLAN_LINES} more lines not shown. Read aiWorkplans/${plan.name} directly for the full plan.]`
            : plan.content;

        const multiNote =
            inProgress.length > 1
                ? `\n\n⚠️ ${inProgress.length} in-progress plans found:\n${planList}\nShowing the first one below. Review the others if needed.`
                : "";

        const additionalContext =
            `[Work Plan Detected] An in-progress work plan was found: aiWorkplans/${plan.name}${multiNote}\n` +
            `Resume from the first unchecked step (- [ ]) in that file. ` +
            `Do not restate the full plan — just confirm which step you are resuming from.\n\n` +
            `Plan content:\n${planContent}`;

        process.stdout.write(
            JSON.stringify({
                hookSpecificOutput: {
                    hookEventName: "SessionStart",
                    additionalContext,
                },
            }),
        );
    } catch {
        process.stdout.write(JSON.stringify({}));
    }
});
