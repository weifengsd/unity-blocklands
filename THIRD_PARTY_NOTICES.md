# Third-party notices

The project's own source code and documentation are licensed under the MIT
License in [LICENSE](LICENSE). Third-party components retain their licenses.

## Noto Sans CJK SC

- File: `Assets/Resources/NotoSansCJKsc-Regular.otf` (unmodified font).
- Copyright notice embedded in the font: © 2014–2021 Adobe
  (http://www.adobe.com/).
- License: SIL Open Font License 1.1, reproduced in
  [Assets/Fonts/OFL.txt](Assets/Fonts/OFL.txt).
- The MIT License does not replace the font's OFL license.

## Project illustrations

`Assets/Resources/BlocklandsIcon.png` and
`Assets/Resources/LoadingWorldKeyArt.png` were created with AI image generation
for this original game. They are concept illustrations, not gameplay captures.
The project's license applies to these assets to the extent the author has
licensable rights. No rights to third-party game brands or characters are granted.

## Unity and macOS frameworks

Unity Editor, Unity runtime libraries and Apple frameworks are not licensed by
this repository's MIT License. They are not distributed as source dependencies
in this repository. Install Unity separately under its applicable terms.
The optional macOS voice bridge uses Foundation, Speech and AVFoundation from
the system SDK; only the project's bridge source is included.

## Omitted local assets

The local project has four pre-generated NPC speech clips. They are omitted from
the public repository because their redistribution provenance has not been
confirmed. Their `.meta` files are omitted too. NPC text remains available when
the clips are absent. The compiled native voice plugin is also omitted; build it
from `Native/` if needed. Player saves, microphone recordings, transcripts,
developer logs, caches and compiled game applications are not included.
