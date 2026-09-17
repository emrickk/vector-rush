#!/usr/bin/env python3
"""Reproduce the original filtered stereo rain loop; standard library only."""
from pathlib import Path
import random
import wave
import struct

output = Path(__file__).resolve().parents[1] / 'UnityProject/Assets/Audio/Rain/CityRain.wav'
output.parent.mkdir(parents=True, exist_ok=True)
random_source = random.Random(2781)
channels = []
for channel in range(2):
    low = mid = 0
    samples = []
    for i in range(24000 * 8):
        value = random_source.uniform(-1, 1)
        low += .018 * (value - low)
        mid += .28 * (value - mid)
        samples.append((mid - low) * .42 + low * .5)
    fade = 2400
    for i in range(fade):
        samples[i] = samples[-fade+i] * (1-i/fade) + samples[i] * i/fade
    channels.append(samples[:-fade])
with wave.open(str(output), 'wb') as stream:
    stream.setparams((2, 2, 24000, 0, 'NONE', 'not compressed'))
    stream.writeframes(b''.join(struct.pack('<hh', int(a*32767), int(b*32767)) for a,b in zip(*channels)))
print(output)
