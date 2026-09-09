# Persistent baseline and regional road templates

Opening02 built but the generated baseline RoadSurface lost `_NORMALMAP` and `_METALLICSPECGLOSSMAP`; native actual-material diagnostics confirmed missing features and ineffective assigned maps. That app/evidence remains preserved.

OpeningFinishPreviewSetup now seeds the baseline build template with existing authored normal/gloss textures and white emission, restores its four intended keywords and disables both detail keywords. It then prepares the separate regional template with the same baseline feature set plus its authored detail seed and `_DETAIL_MULX2`. Post-save checks require both distinct combinations. WorldBuilder/OpeningRoadFinish overwrite template maps and emission before creating renderers, preserving original runtime baseline parameters. No new art constants, camera, geometry, route, physics or light changes occur in this technical correction.

Build/native assertions and original-pixel off preservation remain required; successful source review alone is not acceptance. Separate output: Builds/Vector Rush-opening-03.app.
