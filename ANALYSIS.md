# TikTok Duplicate Detection -- Analysis and New Methods

## Why the Previous 40 Methods Failed

The root cause is **container forensics** -- TikTok's first detection layer. Every
previous output carried unmistakable ffmpeg fingerprints: `encoder: Lavf60.16.100`,
`major_brand: isom` (not `qt` like iPhone), `handler_name: VideoHandler/SoundHandler`
(ffmpeg defaults, not `Core Media Video/Core Media Audio`), no Apple QuickTime
metadata, missing bt709 color space flags, and 44100 Hz audio instead of 48000 Hz.
These markers let TikTok classify the file as "processed by a tool" before any
content analysis runs. Once flagged at the container level, even aggressive spatial,
temporal, and frequency-domain modifications cannot override the verdict. The 40
methods attacked layers 2-5 while layer 6 (container forensics) was rejecting
the file outright.

## New Methods Implemented (Not Previously Tried)

### Container / Metadata (Layer 6 -- the key fix)

1. **Binary MOV atom patcher** -- Parses the QuickTime atom tree, replaces `ftyp`
   brand with `qt`, rewrites `hdlr` handler names to `Core Media Video` / `Core
   Media Audio` / `Core Media Metadata`, removes encoder tag atoms, and adjusts
   `stco`/`co64` chunk offsets after any size changes.

2. **Apple QuickTime metadata injection** -- Builds proper `moov/udta/meta` atoms
   with `keys`/`ilst` structure containing `com.apple.quicktime.make`,
   `com.apple.quicktime.model`, `com.apple.quicktime.software`, and
   `com.apple.quicktime.creationdate`. Uses mdta handler type like real iPhones.

3. **bt709 color space flags** -- Sets `color_space`, `color_transfer`,
   `color_primaries` to bt709 and `color_range` to tv (limited), matching iPhone
   H.264 High profile output.

4. **48 kHz audio** -- iPhone records at 48000 Hz; previous outputs used ffmpeg's
   default 44100 Hz, an instant giveaway.

5. **Deterministic creation timestamps** -- Derived from file hash so re-runs
   produce identical output.

### Visual Processing (Layers 2, 4, 5)

6. **Low-frequency luminance pattern** -- A subtle sinusoidal pattern (3 horizontal
   cycles, 2 vertical cycles) that is invisible at full resolution but directly
   changes the PDQ hash's 64x64 downsample and DCT coefficients. Temporally
   drifting to also defeat TMK+PDQF temporal fingerprinting.

7. **Fade-in / fade-out** -- First and last 20 frames fade from/to 92% brightness.
   Changes the temporal signature at clip boundaries, which are weighted heavily
   by TMK+PDQF.

8. **Per-channel gamma curves** -- Different gamma values to R (1.03), G (1.01),
   and B (0.98) channels, shifting color balance in a way that affects PDQ hashing
   and CLIP embeddings differently from uniform changes.

9. **Film grain overlay** -- Deterministic per-frame gaussian grain with
   channel-specific weights (B:0.8, G:1.0, R:0.8), simulating real sensor noise
   that disrupts C2PA watermark detection in mid-frequency DCT coefficients.

10. **Vignette** -- Radial corner darkening at 8% strength, applied
    multiplicatively to create a natural lens effect.

### Audio Processing (Layer 3)

11. **STFT constellation poisoning** -- Phase perturbation and magnitude noise
    injection targeted at the 200-5000 Hz Shazam-sensitive band with per-channel
    independent perturbations.

12. **Second-harmonic injection** -- Squares the signal (creating 2x frequency
    content), removes DC, and adds at 0.3% amplitude.

13. **Micro-echo at 7ms / 6% decay** -- Chosen to fall between standard room
    reverb times, less likely to be filtered by echo cancellation.

### Architecture

14. **Pipe-based processing with deterministic seeding** -- All random operations
    seeded from SHA-256 of input file. Precomputed LUTs and maps for efficiency.

15. **Speed factor 1.008** -- 0.8% speed increase shifting both video timing and
    audio pitch by ~14 cents (imperceptible).
