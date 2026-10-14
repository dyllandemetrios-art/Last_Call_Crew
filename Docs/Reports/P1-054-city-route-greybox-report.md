# Rapport P1-054 — Squelette mesurable de la ville

**Statut : PASS validé par Dyllan le 14 octobre 2026.** Construction et vérifications techniques faites par Claude Code ; parcours, rythme, KO/portage et poches validés ensuite par Dyllan. Les chronos numériques du playtest n'ont pas été transmis et ne sont donc pas inventés dans ce rapport.

## Départ

Branche `feature/p1-001-network-foundation`, commit `302e7ab`. Documents Codex non commités préservés (`Docs/Backlog.md`, `Docs/Design/`, la fiche P1-054). Aucun commit, aucun push, aucun package, aucun script C# modifié.

## Ce qui a été fait (éditeur Unity ouvert, via `unity command eval`, aucun YAML édité à la main)

- **Bar déplacé comme un ensemble**, translation `(−131,5 ; 0 ; +28)` appliquée à : `Greybox/Walls`, `Furniture`, `Markers`, `Paths/InteriorFloor`, `EntranceMat`, `ExitMat`, `DrinkOffer` (NetworkObject) et `NpcDialogue` (Marcel, NetworkObject). Aucune duplication, hiérarchie et références intactes. Nouvelle position : entrée principale `(−130 ; 36)`, sortie latérale `(−121,3 ; 44)`, comptoir `(−128,5 ; 51,4)`, `DrinkOffer` `(−129,5 ; 50,3)`, Marcel `(−136 ; 49)`. Les coordonnées d'origine confirmaient exactement la translation proposée.
- **Coloc intacte** : maison, porte, `ReturnZone` `(20 ; 16)`, clés, portefeuille, téléphone, poses de départ (code, dans la maison) non touchés. `NetworkObject` : 8 avant, 8 après.
- **Sol** étendu (`Ground`, x −148…34, z −10…70) et clôture périmétrique de 2 m.
- Nouvel objet racine `City` : `Strips` (promenade, ruelle-raccourci, rue nocturne, poches R1–R3, sans collider), `Blocks` (27 volumes d'îlots de 4 m avec `BoxCollider`, générés par fusion de cellules de 1 m pour fermer tout ce qui n'est pas un passage), `Perimeter`, `TaxiRoute`, `Landmarks`.
- **Passages** (axes de la fiche) : promenade 6 m de large, ruelle 4 m, rue nocturne 8 m, sortie latérale du bar 4 m (porte 3 m) raccordée à la rue.
- **Poches** R1 `x −104…−98, z 32…38`, R2 `x −94…−88, z 42…48`, R3 `x −82…−76, z 32…38`, ouvertes sur la ruelle (passage central 4 m libre).
- **Repères** : maison verte existante, carrefour bleu (dalle `(−110 ; 40)` + pilier), enseigne et balise ambre à l'entrée du bar.
- **Route du taxi** : trottoirs 3 m + chaussée 6 m le long de A `(0 ; 16)` → `(0 ; −4)` → `(−130 ; −4)` → `(−130 ; 16)` B ; arrêts A et B en jaune. Primitives uniquement, aucun véhicule.
- 9 matériaux `City*.mat` ajoutés dans `Assets/_Project/Materials/Greybox/`.

## Décisions et écarts

1. **Route réservée non accessible à pied** : elle est scellée par les îlots et la clôture (la fiche ne donne aucun raccord entre B et la rue nocturne ; l'ouvrir créerait un troisième chemin de ~170 m non demandé). À trancher dans la fiche taxi.
2. Arrêt A tracé à `(0 ; 9)` au bout de la branche est, un îlot de 1 m le séparant de la promenade `(0 ; 16)`.
3. Raccords locaux ajoutés : rue de façade `x −134…−106, z 26…34` + approche `x −132…−128, z 34…36` vers l'entrée du bar.
4. Ancien décor devenu sans objet supprimé (visuels sans collider ni script) : `Paths/StartPad`, `PathToBar`, `PathToReturn` et le groupe `Fence` (ancienne clôture 41 × 38 m).
5. Aucune règle réseau, timer, spawn ou vitesse modifié.

## Vérifications réalisées

| Contrôle | Résultat |
| --- | --- |
| Console Unity après la construction | aucune erreur de compilation ni de scène (seuls bruits : message du module IA Unity « NoSubscription » et deux de mes appels de commande inexistante) |
| Scripts | aucun modifié ; tests EditMode **non relancés** (aucun code touché) |
| Scène sauvegardée | oui (`save_scene`), non modifiée ensuite |
| Parcourabilité (BFS, capsule r 0,4 m / h 1,8 m, pas 0,5 m, depuis la pose de départ dans la maison) | atteint l'entrée du bar, l'intérieur, la sortie latérale, la ruelle, R1, R2, R3 et la promenade nord ; **ne** atteint **pas** la route réservée (A/B/chaussée) ; zone atteinte x −138…24, z 11,5…62 |
| Distance BFS maison → entrée du bar | ≈ 167 m en ligne tendue (axes de la fiche : promenade 225 m, raccourci 185 m, sortie latérale ≈ 164,5 m) |
| Vue du dessus | `Docs/Reports/img/P1-054-top-view.png` |

La distance BFS est une borne basse (coupe les virages), pas une mesure de durée.

## Validation de Dyllan

Le 14 octobre 2026, Dyllan a validé l'ensemble des contrôles manuels restant après le rapport technique : parcours hôte/client, trajet maison → bar → sortie latérale → maison, KO et portage, lâcher/réveil, usage des poches R1–R3, orientation et rythme général.

Les durées exactes n'ont pas été communiquées. La validation confirme le feeling et l'acceptation des parcours, mais ne constitue pas une mesure numérique reproductible des cibles de 45 s, 37 s et 33 s.

Aucun contrôle des règles de victoire/relance n'a été rejoué : aucun changement n'y touche et aucune régression n'a été signalée.

## Changements non commités

`Assets/_Project/Scenes/BarGreybox.unity`, 9 matériaux `City*` (+ `.meta`), `Docs/Reports/img/P1-054-top-view.png`, ce rapport, plus les documents Codex préexistants. `Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) est réapparu (doublon NGO déjà rencontré en P1-050) : je ne l'ai pas supprimé, à effacer avant tout commit.

## Prochaine action recommandée

Conserver cette géométrie pour P1 et cadrer séparément le taxi. Sa fiche devra trancher l'accès piéton aux arrêts, le raccord de B à la rue nocturne et le décalage de A à `(0 ; 9)` avant toute logique de véhicule.
