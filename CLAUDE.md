# Personal Instructions

## Behavior
- When troubleshooting an issue, don't be too eager to jump to a solution: discuss your strategy with me first (keep it short).
- A subagent should not spawn another subagent as its first action without doing any work itself. If a task looks too broad for one agent, break it down before delegating rather than passing the whole brief down a chain


## Communication
- Keep answers short, sharp, and to the point unless asked otherwise. This applies to audits, diagnoses, and everything else. When in doubt, default to short and concise, and let me ask for more when I need it.
- Keep specs mechanical: describe only what to do and how — no unsustained claims, no anticipating risks or prescribing actions beyond what's discussed.
- Default to answers that are about 1 paragraph at most. Prefer synthetic answers. I almost never need more than a couple of sentences to understand something. Keep that in mind


## Code style
- Favor readable code over comments. Before writing or editing any code, load the `comment-taste` skill and follow it — it is the single source of truth for comment rules.
- Before writing or editing any code, also load the `code-taste` skill and follow it — it is the single source of truth for function size, naming, and type-driven structure preferences.
- Never insert license headers.


## Investigation & tool use
- Prefer ripgrep over grep.
- Prefer Sed and Edit over python for simple edits.
- Use offset/limit to read only the sections needed; reference earlier reads instead of re-reading whole files.
- Cap ad hoc grep/rg/Read calls at 3 per investigation thread — on the 4th, hand the rest to an Explore agent.
- Delegate codebase-claim verification (e.g. checking a plan against the code) to a single upfront Explore agent call, not incremental checks.
- Explore agents must run on Sonnet or Haiku, never Opus. Default to Haiku for exploration; use Sonnet if the task looks difficult.
- `find` cannot run against `/` (root).
- Comments and doc comments go stale, so treat them as unverified claims about the code, never as evidence. Never draw a conclusion from a comment alone — read the code it describes first, and if the two disagree, the code wins and the comment is a bug worth reporting. This applies to comments quoted back by a subagent too.


## Git
- Never commit the contents of `.plans/` folders.
- Never reference something in `.plans/` in the comments or the code you write.
- Never answer PR comments via `gh` on your own.
- Never use worktrees.
- Never push upstream unless explicitly asked to.
- Never commit directly to `main` or `master` (unless explicitly asked to), create a branch instead and ask the user for a name.
- Prefix commit messages with a prefix `prefix : commit_message` where `prefix` can be feature, doc, config, fix (...) depending on the change

## Stacking PRs
- When a branch becomes too big to be reviewed as a single PR, take advantage of the GitHub stacked PR capabilities to ease the review task.
- Naming convention: `<original-branch-name>-stack-<stacknumber>-<a-good-suffix-you-pick>`.


## Quality gates
- Run build/test as a single command that writes its full log to a file; bring only the pass/fail summary and actual failures into the conversation.


## Bug fixes
- When fixing a bug surfaced by a failing test: proceed autonomously only if the fix is small after initial diagnosis; otherwise stop and report back.


## Hygiene with regards to temporary files
- Do not create temporary files in the repository you are working on (for e.g. `output.log`). Instead, always put them under `/Users/xxx/tmp/<repo-name>/<branch-name>/`.


## Spikes and POCs
- Anytime you are asked to build or validate a hypothesis with a short spike (either yourself or by delegating to an agent), record the created code in a patch file so that it's not lost.
- If the ask happens while manipulating a dev-strategy plan, store the patch in the plan folder.


## On writing tests
- A test's goal is dual: it has to explain the behavior and test it. Comment rules are the `comment-taste` skill's — if a test reads as though it needs a comment on top to be understood, refactor the test.
