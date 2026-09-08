# V3 pass 02 — staged checkpoint, partial evidence

The user requested wrap-up while evidence rendering was in progress. The exact render process was stopped. The immutable editable source and FBX are complete; only the rear-quarter clay image is complete. See `checkpoint-status.json` and `capture-contract.json` for exact scope/settings.

The independent reviewer found this angle coherent enough to advance under the user’s practical visual target, pending side/top/chase confirmation. This is not full form, finish, or native approval.

The separate source/FBX reimport audit passed: all source meshes manifold; runtime meshes manifold and triangulated; imported FBX meshes manifold with valid tangents; runtime/import triangle counts match. The exact runtime collection owns the unique UV atlas and must be used unchanged for later baking.

The initial render hit slow CPU denoising. A resumed render used 48 samples, four CPU threads and no denoiser to produce the one completed image. Later attempts to prioritize side/top/chase at 32 samples produced no additional images before the user’s stop instruction. All logs are preserved in the parent evidence folder.

Pass 01 remains preserved with its rejected complete view set. Pass 02 materials are only neutral category colors; finish/livery/baking and live Unity integration remain pending.
