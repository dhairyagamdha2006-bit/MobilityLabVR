# Third-party notices

## Project-created content

All environment geometry, characters, vehicles, road markings, traffic lights,
UI layout, color palette, and the SVG wordmark are project-owned procedural or
code-native content. No external art, audio, font file, model, texture, paid
Asset Store package, or sample scene is bundled.

The runtime UI requests Unity’s built-in `LegacyRuntime.ttf`; the repository
does not redistribute a font file.

## Unity packages

The package manifest references official Unity packages:

- Universal Render Pipeline
- Input System
- XR Plug-in Management
- OpenXR Plugin
- Unity UI
- Unity Test Framework
- Unity IDE integrations and built-in engine modules

Those packages are downloaded through Unity Package Manager and are governed by
their respective Unity/package terms. They are not copied into this repository.

## Python dependencies

The analysis environment installs pandas, Matplotlib, scikit-learn, and joblib
from their official distributions. Each remains governed by its own open-source
license. The repository does not vendor those packages.

If a future contribution adds external content, record its name, source URL,
author, exact license, version, modifications, and redistribution requirements
here before committing it.
