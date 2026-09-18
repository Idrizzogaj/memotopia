> Det äldre Mac-releasearkivet finns numera komprimerat och innehållsverifierat i `ios-build/Memotopia_1.0.31_2026070701.xcarchive.tar.gz`. Det ska behållas på Macen och ingår inte i Windows-källkodspaketet.

> Status 2026-09-17: Windows-flytten är uppskjuten tills iOS/Achievements är klart. Det tidigare ZIP-paketet har tagits bort som inaktuellt för att ge plats åt bygget. Skapa och verifiera ett nytt paket före överföring; äldre checksummor gäller inte längre. Arbetskopian och Git-historiken finns kvar på Macen.

# Memotopia: övergång till Windows

Uppdaterad 2026-09-17 efter installation av **1.0.33 (2026091701)** på ZogajSon. **Windows-kopian är ännu inte provkörd på Windows och ingen ny GitHub-push är gjord.** Behåll arbetskopian på Macen tills Windows-kopian är kontrollerad.

## Lokal överföringskopia

`windows-transfer/Memotopia-Windows-20260917-PRIVATE.zip` är avsedd för direkt, privat överföring till din egen dator. Den innehåller arbetskopian inklusive Git-historik, ospårade filer och eventuella lokala miljöfiler. Publicera den inte på GitHub eller via en offentlig delningslänk.

Byggprodukter i `ios-build`, Unity-cache, `node_modules`, genererad backend/dist och själva överföringsmappen undantas. Originalen på Macen behålls. Filer utanför projektmappen, som Apple-certifikat i nyckelringen, releasearkivet i Xcode och den saknade Android-keystoren, ingår inte.

Extrahera kopian till en kort sökväg, exempelvis `C:\Projects\memotopia`. Med denna kopia behöver du inte först klona GitHub: kontrollera `git status` och `git rev-parse HEAD` mot medföljande manifest. De lokala ändringarna ska fortfarande synas som lokala ändringar. Vid skillnader enbart i exekveringsrättigheter på Windows, kör `git config core.filemode false` i den överförda kopian. GitHub-inloggning behövs när du senare hämtar eller pushar, men inte för att öppna denna lokala kopia. Öppna sedan `frontend` enligt installationsstegen nedan. GitHub-alternativet kräver fortfarande granskad commit och push.

## Projektet

- GitHub: `git@github.com:Idrizzogaj/memotopia.git`.
- Unity-projektet är undermappen `frontend`, inte repots rot.
- Nuvarande Unity: **2019.3.12f1 (84b23722532d)**. Uppgradera samordnat i en separat ändring, inte genom att öppna en enda dator med en annan editorversion.
- Alla tre spel använder `frontend/Assets/Resources/MajorSystem/00.png`–`99.png`. Behåll alla `.meta`-filer; deras GUID:er knyter ihop scener, bilder och prefab-filer.
- Bildordlista och källinformation finns i `docs/major-system/`.
- Rena motiv väljs via `frontend/Assets/Resources/MajorSystemSourceMap.txt`: 54 återanvända äldre original utan ramar, 45 nya motiv från MajorSystem-paketet och en ny ROCK-variant. `MajorSystemImages` normaliserar presentationen i minnet; källbilderna bevaras.
- `BoxesPicturePerspective` visar ett upprätt motiv på en separat isometrisk platta; endast Boxes får den plattan. Flash och Pairs använder samma rena motiv utan platta.
- Förhandsbilder från Unity finns i `docs/major-system/normalized-review/`. Den nya stenens bildverktyg och prompt dokumenteras i `docs/major-system/rock-generation.md`.
- API-koden finns i `backend`. Paketverktyg är Yarn 1.22.22 enligt package.json. Shell-skript för lokal drift är Mac/Linux-skript och behöver anpassas eller köras i en lämplig miljö på Windows.

## Innan överföringen

1. Slutför och dokumentera telefonbygget och bildkontrollen i `CODEX_HANDOFF.md`.
2. Granska hela arbetskopian, inklusive tidigare lokala ändringar, borttagna filer och ospårade resurser. `git clone` bevarar inte sådant som inte är committat och pushat.
3. Kontrollera ignore-regler och hemligheter före en avgränsad commit/push. Lägg inte byggprodukter, `.env`, privata nycklar, signeringsfiler eller tillfälliga backuper på GitHub.
4. Inventera nödvändiga filer utanför Git och kopiera dem separat via säker överföring. Skriv inte in lösenord eller nyckelinnehåll i överlämningen.
5. Verifiera att GitHub har exakt den avsedda committen och skriv dess ID här. **Commit-ID för överföring: ännu inte fastställt.**

## På Windows

1. Installera Git och Unity Hub, logga in på ditt Unity-konto och installera därefter samma editorversion med Android Build Support och tillhörande SDK/NDK/OpenJDK. Installera även iOS Build Support: projektets Editor-skript använder UnityEditor.iOS.Xcode även när du arbetar med Android.
2. Klona repot och checka ut den verifierade överföringscommitten.
3. Öppna `frontend` i Unity Hub. Unity återskapar Library/Temp lokalt; kopiera inte Macens cache.
4. Kör `Memotopia > Validate Major System images` och kontrollera att exakt 100 bilder laddas.
5. Provspela Pairs, Boxes och Flash. Kontrollera både visningsfas och svar/dragning samt inloggning och scenbyten.
6. Bygg och installera ett Android-test på den tillgängliga telefonen innan Android-versionen räknas som verifierad.

## iOS och bevarade filer på Macen

Slutlig iOS-kompilering/signering kräver Mac/Xcode eller en sådan byggmiljö i molnet. GitHub-push utlöser för närvarande ingen TestFlight-uppladdning.

- `frontend/Assets/Editor/MemotopiaBuild.cs` innehåller iOS-export och kompatibilitetsfixar för nuvarande Xcode. Exportvägen beräknas nu relativt projektet, så att samma skript fungerar från en annan arbetskopia. Själva Xcode-kompileringen körs fortfarande på Mac.
- Behåll `GCC_STRICT_ALIASING=NO`, borttagningen av `-mno-thumb` och skyddet för saknad dSYM tills Unity-migrationen är verifierad.
- Fungerande äldre releasearkiv finns i `~/Library/Developer/Xcode/Archives/2026-07-07/Memotopia_1.0.31_2026070701.xcarchive`.
- Exporterad IPA finns i `ios-build/export_1.0.31_2026070701/`.
- Bildkällpaketet finns i `~/Downloads/Memotopia_Major_System_00-99_PNG.zip`; de importerade bilderna finns också i projektet.
- Apple-signering och eventuell Android-keystore behöver inventeras separat. Nuvarande Android-inställning hänvisar till en keystore i Downloads; filen hittades inte på den angivna Downloads-sökvägen på Macen. Den behöver hittas separat innan en uppdatering av en tidigare publicerad Android-app kan signeras.

Överföringen är klar först när Windows kan öppna projektet utan saknade resurser och rätt commit och alla nödvändiga lokala filer är verifierade. Macens projekt behålls under tiden.
