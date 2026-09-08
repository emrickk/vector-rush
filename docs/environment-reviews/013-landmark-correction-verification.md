# Environment review 013: landmark correction verified

Independent verification, 2026-09-08. **Accept the slab removal and close the bounded landmark milestone.** The single composition correction requested in [review 012](012-landmarks-native-critique.md) is visible in all three checked first-sector views. The retained final-station reveal has no new composition problem in the inspected image. Proceed to the separate road-finish experiment; no additional landmark correction is requested.

## Exact inspection scope

Directly inspected four original 1920 × 1080 candidate 02 images and their four candidate 01 counterparts: frames **148, 166 and 175**, plus **05-reveal.png**. The six selected first-sector copies linked below were checked by SHA-256 against the originals inspected in each full-lap frame directory; all six are byte-identical.

| Reference | Candidate 01 | Candidate 02 | Finding |
| --- | --- | --- | --- |
| Frame 148, progress 0.1497 | [Before](../../evidence/landmarks-candidate-01/selected/frame-0148.png) | [After](../../evidence/landmarks-candidate-02/selected/frame-0148.png) | The blank slab and pale stripes are absent; the mast has more surrounding sky. |
| Frame 166, progress 0.1698 | [Before](../../evidence/landmarks-candidate-01/selected/frame-0166.png) | [After](../../evidence/landmarks-candidate-02/selected/frame-0166.png) | The former large right-hand mass is replaced by the recessed skyline. |
| Frame 175, progress 0.1797 | [Before](../../evidence/landmarks-candidate-01/selected/frame-0175.png) | [After](../../evidence/landmarks-candidate-02/selected/frame-0175.png) | The split uprights, room and supports remain clearly recognizable beside the open bend. |
| Final-station reveal | [Before](../../evidence/landmarks-candidate-01/full-lap/05-reveal.png) | [After](../../evidence/landmarks-candidate-02/full-lap/05-reveal.png) | Workshop, canopy, course gate and clear straight retain their composition. |

The first-sector road, barrier, gate, player and HUD remain readable. Removing the competing slab gives the existing mast space without changing its size or adding visual detail. The final-station image retains its near buildings and distant city relationship; the deletion does not produce a conspicuous gap that needs another asset.

## Capture identity and comparison limits

[Candidate 02 metadata](../../evidence/landmarks-candidate-02/full-lap/environment-evidence.json) records a complete 1,440-frame sequence from 17:30:34 to 17:32:12 UTC, Unity 6000.6.0f1, Apple M2 Max, build GUID `a1325c823e8b4541b20c143f42f12470`, 1,247 scene renderers and 101 active lights. It covers 60 simulation seconds with automated steering, normal hover physics and a 24 fps simulation capture rate. All three first-sector references are grounded without boost; the reveal is grounded with boost.

Against candidate 01, the three first-sector references have identical recorded race time and track progress, with approximately 0.081 m of camera displacement and less than 0.00007 degrees of field-of-view difference. The reveal is candidate 02 frame 1004 versus candidate 01 frame 1005: camera displacement is 2.660 m, absolute progress difference 0.0014594, and field-of-view difference 0.1139 degrees. The automated report marks its anchor comparisons outside tolerance. This is a visual correction check, not an identical-pose rendering control.

The [candidate 02 source manifest](../../evidence/landmarks-candidate-02/source-manifest.json) records the scope as removal of the identified skyline slab. Comparing its listed hashes with candidate 01 finds only `NightDistrict.cs` changed; the listed landmark assets and road-related entries match. This report does not evaluate the subsequent road experiment.

The thermal placement acceptance remains documented in review 012; no thermal frames were newly inspected for this correction check. Neither the full sequence nor the other four station anchors were visually reviewed in this pass. Continuous playback, audio, other aspect ratios, performance and human driving remain outside its scope. The earlier finish limitations are preserved and do not prevent closing this specific correction.

Only this verification document was authored. No runtime source or assets were edited.
