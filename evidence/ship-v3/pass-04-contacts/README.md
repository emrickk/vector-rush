# V3 pass 04 — targeted surface contact correction

Native pass03 close-ups showed gray shards along the canopy and serrated intake/nozzle borders. The source audit confirmed physical surface conflicts: glazing/sill near-parallel contacts down to hundredths of a millimeter, exactly coplanar end caps, and positive-area white/graphite intake and nozzle-return overlap. This cause exists independently of texture quality or tangent mapping.

Only three Graphite source objects were corrected: the narrow sill lowered 40 mm and shortened 1%; nacelle backing returns recessed 15 mm; the buried graphite intake opening enlarged 15 mm. Accepted white armor, glazing, engine geometry, all runtime topology and every UV coordinate remain unchanged. The runtime change is restricted to 1,165 Graphite vertices.

The measured competing coplanar overlaps fell to zero; canopy near-parallel centroids within 2 mm fell from 145 to zero. Noncoplanar assembly intersections remain, including the hidden glazing underside entering the monocoque. These counts do not constitute a final visual pass. See before/after reports and validation-summary.json for exact scope.

The separate FBX reimport passes manifold, triangulation, tangent and triangle-count checks. The generic whole-source equality field is expected to be false; the specific three-object change allowlist and all-UV equality checks passed.

No textures were baked and no live Unity file was edited. Existing maps align for a controlled native before/after geometry test. A final Graphite rebake is recommended because moved source positions and contact relationships change procedural grain and AO. Parent owns native validation and any texture refresh.
