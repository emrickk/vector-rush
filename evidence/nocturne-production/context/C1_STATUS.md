# C1 production runtime and geometry context

Status: COMPLETE

- Source commit: `b0dd53da80faf4d96ae22af775f877d34bb08fab`
- Course hash: `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`
- Geometry context SHA-256: `5a22d7779b229a7f7d38342a393414d99c5804f13badab5e6d8040ded782c764`
- Geometry context: `/Users/anping/Documents/Stuff/AI Space/Vector Rush/evidence/nocturne-production/context/geometry-context.json`
- Course data: `/Users/anping/Documents/Stuff/AI Space/Vector Rush/evidence/nocturne-production/context/course-data.json`

The production runtime uses only the saved `ProductionWorld`, its material library, and one scene-authored MainCamera. When the production reference is null, the legacy Solstice world/material/camera path remains active. Production validation never regenerates scenery.
