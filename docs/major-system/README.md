# Major System images

All three games (Pairs, Flash and Boxes) draw from the same 100 pictures in
`frontend/Assets/Resources/MajorSystem/00.png` through `99.png`.
Game IDs remain 1–100 internally; `MajorSystemImages` maps them to 00–99.
Level difficulty still determines how many pictures appear in each round.

Source: `Memotopia_Major_System_00-99_PNG.zip`, downloaded from the ChatGPT Work
task “Hämta Major system bilder” on 2026-09-16:
https://chatgpt.com/g/g-p-6981ac3783b481918a5c8c3c168a62d6-memotopia/c/6aaaf3ea-4388-83ed-83fe-4ff262f9a1aa

PNG bytes are preserved from that export, including its existing backgrounds.
The set combines the existing illustrations with the additional illustrations
created in Work. See `00-99_mapping.txt` and `contact-sheet.jpg` for the complete set.
The previous resource folders are retained, but gameplay now loads this shared set.

Run **Memotopia → Validate Major System images** in Unity to verify that all 100
numbered assets import as sprites and resolve to their expected resource paths.


## Enhetlig visning, 2026-09-17

De ursprungliga 100 PNG-filerna bevaras. Spelen laddar nu visningsmotiven via `MajorSystemImages`: exakt verifierade äldre motiv återanvänder sina rena original från ImagesWithoutBackground, och nya motiv får kantansluten vit bakgrund borttagen i minnet. Samtliga normaliseras till en genomskinlig kvadrat med samma marginal. Mapping finns i `clean-source-map.json` och spelets `MajorSystemSourceMap.txt`.

ROCK (47) har en separat ny variant utan grottliknande form; ursprungsbilden behålls. Prompt och verktyg finns i `rock-generation.md`. Endast Boxes lägger till en gemensam isometrisk platta med sidoytor och kontaktskugga. Motivet självt förvrängs inte. Flash och Pairs använder de rena motiven utan platta. Unity-renderade kontroller av alla motiv och exempelplattor finns i `normalized-review/`.
