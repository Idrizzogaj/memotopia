# Bildgranskning 2026-09-17

Granskade användarens sex nya bifogade skärmbilder (en dubblett) och förklaringen i ChatGPT-uppgiften `6aab0bb9-6178-83ed-9184-79fad0eabd9f`. Representativa originalskärmbilder sparas här, oförändrade.

## Bekräftad orsak

MajorSystem-paketet blandar två presentationer. Exempel: `60.png` (JUICE) och `47.png` (ROCK) innehåller en vit rundad ruta och en cyan skugga. `53.png` (LIME) och `78.png` (CAVE) saknar den ramen. Det är alltså inte bara spelens placering som skiljer sig: dekoration, marginaler och motivets relativa storlek finns redan i källbilderna.

Skärmbilderna flash-juice.png och flash-lime.png visar detta tydligt. JUICE visas som ett litet kort inuti spelrutan; LIME visas som ett fristående, större motiv. Svarsalternativen blandar också samma två format.

Äldre kod bekräftar att Flash laddade `ImagesWithoutBackground`, medan Boxes laddade `3DImages`. De gamla 3D-bilderna har en ritad isometrisk plattform. De var separata presentationer, inte samma bildfil i alla spel. De gamla mappnumren kan INTE kopplas direkt till Major System-numren: exempelvis gamla Artboard-47 är en nyckel, medan nya 47 är ROCK.

## Begränsning i senaste rättningen

2026091602 löste geometrisk passning genom att göra Flash-bilden mindre och projicera hela Boxes-kortet på en diamant. Det rensar inte inbakade ramar. I boxes-projected.png syns både den extra ramen och att även själva motivet plattas till. Att enbart justera skala eller hörn igen löser därför inte grundproblemet.

## Sammanhållen lösning att genomföra

1. Behåll exakt 100 identiteter, nummer och ord i gemensam mapping. Samma motiv i alla spel kräver inte samma färdigdekorerade PNG.
2. Ta fram enhetliga rena motiv med transparent bakgrund och jämförbar synlig storlek. Återanvänd de äldre rena original som verkligen matchar rätt motiv, efter explicit kartläggning; generera inte om fungerande motiv i onödan. Bevara de godkända originalen.
3. Låt spelet stå för ruta, skugga och plattform. Pairs och svarsalternativ får samma kortbehandling för samtliga motiv. Flash visar ett upprätt motiv i diamantens vita yta. Boxes behöver en gemensam plattform med ett läsbart motiv ovanpå, inte en projektion av hela bilden inklusive ramen.
4. Kontrollera ett äldre och ett nytt motiv sida vid sida, därefter samtliga 100. Testa Boxes både före recall och efter dragning, Flash både visning och svarsalternativ, samt Pairs. Godkänn förhandsvisningar före nytt telefonbygge.

## ROCK och CAVE

Mapping: **47 ROCK**, **78 CAVE**. Ingen post heter STONE. Det är två separata filer och nummer, men användarens förväxlingsrisk är tydlig: båda har grå kantiga bergformer, och ROCK har ett mörkare område nedtill som liknar en öppning.

Rekommenderad ändring: ROCK som en ensam kompakt, gärna rundad sten utan öppning. CAVE behåller sin stora tydliga mörka ingång. Ändra inte numren eller orden. Nytt motiv behöver granskas visuellt innan det ersätter den godkända bilden.

## Status

Denna omgång är en orsaksanalys. Inga nya bildfiler har ersatt spelbilderna, ingen ny app är byggd eller installerad och inget har pushats. Telefonen har fortfarande 2026091602. Windows-arkivet som skapades tidigare är en ögonblicksbild före denna granskning; uppdatera överföringskopian när nästa bildändring är klar.
