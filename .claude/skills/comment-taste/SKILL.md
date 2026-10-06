---
name: comment-taste
description: Rules for inline comments in any language — whether a comment earns its place, where the information belongs instead, and the failure modes to never emit. Load before writing or editing any code, and when reviewing the comments in a diff.
---

# Comments

Scope: inline comments — anything written inside a function body, or above a statement, field, or block. Whether a public API *deserves* a doc comment is a separate contract question and out of scope; doc comments remain a legitimate destination for information moved off a call site.

The Never list below applies wherever a comment lives, doc comments included. Hoisting a banned comment up onto the function or module does not launder it.

These rules are absolute. A repo that already comments heavily is not a license to add more: match its formatting, not its density.

## The reader test

Bad comments almost all come from writing for the wrong reader — either the human reviewing this diff, or an imagined grader checking that the task was understood. Both readers are gone within the hour.

The only reader that counts opens this file later with no access to the conversation, the plan, or the diff.

Before writing any comment: does it carry information for that reader? If it only makes sense to someone watching the work happen, delete it.

## Wanting to comment is a signal, not a decision

A well-named call reads as a description of its own behavior. The urge to explain one is evidence about the code. Sort the urge before acting on it.

**Narration → reshape the code, don't write the comment.**
A comment restating what a chain of calls already says means the names carry too little meaning. Rename, split, or add a narrower method until the code reads as the description, then drop the comment.

**A property of the abstraction → put it on the definition.**
Why a default is what it is, why two steps must happen in this order, a gotcha a past bug revealed — real information, but it belongs to the thing it describes, not to one caller. Write it once on that method, type, or constant. A crate, module, or package boundary inside the same workspace is not an ownership boundary: if we wrote it, go fix the doc where it lives, however far away. This routes to a call-site comment only for genuinely external or vendored code, where there is no definition of ours to attach it to.

**A call site explaining a limitation of the abstraction → fix the abstraction.**
A comment that apologizes for a missing setter or a wrong default is a request to add the setter or fix the default, so the call site stops needing to explain itself.

**Debugging context → put it in the failure output, not the source.**
If the point is "here's why this value matters, so a failure is easier to diagnose," it belongs in the assertion or error message. A reader debugging sees it exactly when they need it; a reader skimming doesn't have to step over it.

**Intent → legitimate, and usually not per-line.**
Why this code exists: the hypothesis it checks, the regression it guards, a real-world constraint that shaped it. Belongs at function or module level, not scattered inline.

The discriminator is whether the sentence is derivable from the name and the body. If it is, it is narration wearing intent's clothes — delete it. "Tests that a promoted key is pruned on query" above `fn promoted_key_is_pruned_on_query()` adds nothing; "guards the regression from #4412, where pruning ran before promotion" does.

**A deliberately arbitrary literal → legitimate, one line.**
`// arbitrary; any value above the admission gate works` earns its place, because the point is that the value is *not* meaningful and naming it would wrongly imply that it is. Applies to a literal, never to a whole call or chain.

## Never

1. **Diff narration.** No `now`, `new`, `instead`, `previously`, `added`, `changed`. "Now also handles the empty case" is PR-description content: noise after one review, a lie after two.
2. **Restating what the code already says.** Inline and in doc comments alike: a type already on the line (`# returns a list of users` above `-> list[User]`), a docstring re-listing parameters with nothing added, or a comment above a test that paraphrases its name and body. A well-named test needs no summary above it — if the name doesn't convey the behavior, fix the name.
3. **Plan residue.** `# Step 1: parse`, `# --- Validation ---`, `# Setup`, `# Main loop`. The plan was made in steps and then emitted as comments. If the steps are real, they are functions.
4. **Teaching the language or library.** "dict comprehension for O(1) lookup", "Arc for shared ownership across threads". The reader knows the language better than the author.
5. **Hedging.** "may need revisiting", "consider caching here", "note: assumes X". Uncertainty is a message to the user — it goes in the reply, not the source.
6. **Comments that carry the structure.** If deleting the comments makes a block unnavigable, the block is too long: split it. A comment that makes bad structure tolerable removes the pressure to fix it, which makes this the most damaging item here.
7. **Uniform density.** A comment every few lines regardless of whether anything is non-obvious. Real code clusters comments at the genuinely weird parts and leaves the rest bare; even distribution is the tell that no judgment was applied.
8. **Stale comments.** When a line changes, the comment above it is part of that line. An actively lying comment is worse than every other item here.
9. **Commented-out code.** Never, for any reason. Version control already keeps it.

## Length

One to three short sentences, three lines hard ceiling. Needing more means the abstractions are wrong and the code should be refactored, not annotated.

## Before finishing

- Every surviving comment passes the reader test.
- Nothing sits in a bucket that routes it elsewhere.
- Nothing on the Never list.
- Placement is uneven, clustered at the non-obvious parts.
