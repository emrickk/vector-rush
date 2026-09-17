# Vector Rush: opening-road light response

> 状态更新，2026-09-15：道路候选已保留在 `7358f77`。用户现要求重新规划整体赛车体验。本文保留为道路任务的历史说明；新的制作入口为 [整体体验计划](../2026-09-15-integrated-racing-experience/PLAN.md) 与 [新交接](../2026-09-15-integrated-racing-experience/HANDOFF.md)。不要把本文当作下一轮独立修补任务启动。

Status: plan ready for execution in a new GPT-5.6 Sol conversation. This document authorizes no implementation in the planning conversation.

## Outcome and scope

Make the opening bend's road visibly respond to existing track lighting, with colored highlights that change convincingly as the craft moves and the road banks. Prove one continuous 15–20-second passage before extending the treatment. Preserve dark surface detail, racing-line visibility and the night-city identity.

Keep track geometry/collisions, handling, speed, camera behavior, exposure, bloom, craft, blue exhaust, city composition, HUD and menus fixed. No new gameplay features, pipeline migration or full-track rollout. Road-specific materials/shaders, bounded lighting contributions and diagnostic controls are in scope. A camera depth/color texture request needed by the chosen method is allowed; camera framing and motion changes are not.

## Baseline and evidence

Start from commit `b987c23` in the opening-city checkout named in HANDOFF.md. It contains the blue-particle exhaust candidate. The owner has authorized the next road task but has not declared the exhaust artistically accepted; preserve it unchanged.

The current RoadSurface and OpeningRoadFinish material files include `_ENVIRONMENTREFLECTIONS_OFF` while direct specular is enabled. Verify the actual instantiated road material, active shader and runtime overrides. That keyword disables environment sampling; it does not by itself explain the rejected SSR result, which has a separate rendering path.

Read these repository files before choosing a method:

- `docs/blue-exhaust-review.md`
- `docs/road-reviews/005-reflection-feasibility.md`
- `docs/visual-target-reviews/001-reflection-feasibility.md`
- `docs/visual-target-reviews/002-improvement-approach.md`
- `UnityProject/Assets/Scripts/World/OpeningRoadFinish.cs`
- `UnityProject/Assets/Scripts/World/WorldBuilder.cs`
- `UnityProject/Assets/Scripts/World/NightTrackLighting.cs`

Earlier environment-reflection/probe candidates did not demonstrate useful broad reflection shape. SSR02 executed CPU work but showed no convincing full-frame gain; actual GPU contribution remained unverified. These are failed configurations, not proof that every reflection technique is unsuitable. Do not repeat them without identifying a concrete missing input or changed hypothesis.

## Execution sequence

### 1. Preserve and inspect the current road

Inventory the working tree and running Editor; preserve unrelated changes. Inspect the baseline at normal driving distance. Define the exact passage using existing route progress/time markers. Capture its baseline and record app/source identity.

Trace the road mesh to its runtime material, smoothness/normal inputs, light sources, reflection sources, shader variants and renderer. Distinguish three contributions: diffuse illumination, direct specular highlights, and reflected scene information. Use temporary isolated toggles or diagnostic views to establish which inputs actually reach the road. Record a concise diagnosis, then implement; avoid a long planning detour.

### 2. Establish one useful contribution

First prove the material can receive a known light/specular contribution. Try a localized reflection probe only if the audit identifies why the prior test lacked useful input, and capture it after the runtime environment exists. Restrained roughness variation may shape an available contribution; smoothness alone cannot supply missing scene information. Judge broad lighting and reflections separately.

If that route cannot represent nearby cyan rails convincingly, prototype a bounded analytic rail-light approximation in a road-specific shader. Derive source positions/directions from the actual rail segments and banked track frame. Shape the highlight using surface normal, view direction, roughness and distance attenuation; blend segment transitions smoothly. Keep energy and source count bounded, and confine the effect to the selected road receiver. Reuse the road's material detail instead of replacing it with uniform gloss.

This approximation may reproduce selected rail highlights. It must not be described as arbitrary reflections of buildings, vehicles or offscreen scenery. Avoid a fixed emissive strip painted in road coordinates. A broad single planar mirror does not fit the changing road bank and height.

Keep the existing SSR implementation disabled. A renewed SSR test is justified only by a specific missing-buffer/variant diagnosis and a small check of the actual reflection output, not another strength sweep. Do not expand into a rendering-system rewrite.

### 3. Prove the result in motion

Iterate in the running Editor. After a failed visual test, explain what failed and change the hypothesis before the next attempt. If a technique still produces negligible gain or unstable artifacts after one focused correction, reject it and move to the next bounded method. Do not repeatedly build subtle parameter variations.

Choose one successful treatment and record matched road-off/road-on footage in the same binary. Keep the new blue exhaust on in both. Compare approach, mid-bend and exit; also inspect banking transitions and the boundary of the treated section. Diagnostic false colors or exaggerated brightness are for debugging only and must be removed from the candidate.

### 4. Deliver and stop at this passage

Deliver a separately named native candidate with the treatment enabled on ordinary launch and a documented baseline switch. Preserve the existing exhaust app. Run relevant regressions and separate 1080p real-time performance checks with no capture/encoding workload. Record mean and p95 frame times plus hardware, settings and sampling window; report GPU cost only if measured directly. Investigate a repeatable material slowdown before delivery.

If no bounded method produces a convincing result, preserve the baseline and provide the failed evidence plus the precise unresolved rendering dependency. Do not label a merely compiling or brighter candidate complete.

## Acceptance

- Colored highlights occupy a meaningful part of the road at ordinary gameplay scale and move consistently with view, curvature and banking.
- Roughness breaks up and softens the response while road edges, dark aggregate and the upcoming corner remain readable.
- No painted luminous stripe, mirror-like sliding, seams, popping, flashing, obvious clipping or distracting noise through the passage.
- A same-build comparison records actual pose/FOV deltas. Match inputs and timing; disclose differences rather than presenting unmatched views as exact.
- Native normal driving, boost and release remain functional. Relevant lifecycle checks pass, and added rendering cost is recorded separately from capture speed.
- Deliverable: 15–20-second before/after video, three matched still pairs, playable app, baseline switch, concise technique verdict and checks. Source and artifact paths must be explicit.

Successful delivery means a reviewable, visibly improved road passage. Owner artistic acceptance and full-track rollout remain separate.
