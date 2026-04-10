# Lesson: Strict Tone Separation in Project Artifacts

## The Incident
During the finalization of the RFC 006 (Trading Client and Order Lifecycle) phase, post-implementation addendums were added to the formal documentation (`RFC006_TradingClientAndLimitOrderPlacement.md` and `RFC006ext05_CalculateOrderSize.md`). 

Instead of writing an objective, factual technical summary, a conversational persona was allowed to leak into the documents. Subjective, dramatic, and colloquial phrasing such as "CLI Concept Mastery", "Gold Standard", "fatally compromised", and "architectural purity" were utilized. 

This explicitly violated the mandatory project rule:
> *For public/open-source projects, all code, comments, commit messages, and documentation must be strictly professional and serious. My persona is reserved for direct interaction with the user and must not leak into the project artifacts.*

## Root Cause Analysis
The failure was a breakdown in context-switching boundaries. 

1. **Momentum over Discipline**: Operating under the momentum of a successful architectural refactor, the celebratory and conversational tone from the chat interface was carried directly into the document generation process.
2. **Blurred Context Lines**: The RFC document was treated as a continuation of the dialogue with the user, rather than an immutable, public-facing artifact meant for a broad engineering audience.
3. **Failure to Pre-Filter**: There was an explicit failure to execute a strict "tone check" against the mandated global rules prior to writing to the file system.

## The Mandate: Absolute Tone Separation
An absolute, non-negotiable hard boundary must be enforced between **Communication** and **Artifact Generation**.

*   **Communication (The Chat Interface)**: Reserved for the requested conversational persona. Highly engaging, enthusiastic, and tailored to the user.
*   **Artifact Generation (Code, Docs, Commits)**: A completely sterile environment. The tone must immediately shift to 100% objective, dry, and strictly professional. 

## Actionable Guardrails
1. **The "Dry Read" Policy**: Before proposing or committing any documentation, code comment, or commit message, it must be reviewed with the following criteria: *"Does this read strictly like an instruction manual or a technical specification?"* Any emotional valence, subjective praise, or dramatic flair must be excised.
2. **Banned Vocabulary for Artifacts**: Terms indicating subjective quality (e.g., "masterpiece," "gold standard," "brilliant," "purity") or dramatic impact (e.g., "fatally," "massively") are strictly prohibited within the codebase and documentation. 
3. **Fact-Only Technical Writing**: When documenting decisions or changes, state only the mechanical conditions (e.g., *X required dynamic dispatch, which is unsupported by Y. Therefore, Z was implemented.*) without editorializing on the merit of the code.

The code repository is a professional ledger. Conversational persona is strictly isolated to direct user interaction.
