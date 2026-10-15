# Rapport P1-056 — Première passe d'assets

**Statut : FAIL visuel après playtest de Dyllan le 14 octobre 2026.** Import, rendu URP, références, trajet hôte/client et appel à vide passent techniquement. Dyllan demande une caméra extérieure pendant le taxi et l'ouverture du monde actuellement contraint par les blocs. Aucun code, gameplay ni règle réseau n'avait été modifié dans cette première passe ; aucun test EditMode relancé. Rien n'est commité ni poussé.

## Départ

Branche `feature/p1-001-network-foundation`, commit `05e1928` (`feat: add the collective taxi`, qui clôture P1-055 avec le backlog, la fiche P1-056 et le rapport P1-055). Arbre propre au départ.

## Import (sources locales `D:/Assets/LastCallCrew`, aucun ZIP copié)

| Dossier | Contenu retenu |
| --- | --- |
| `Assets/ThirdParty/Quaternius/RealisticCarPack/` | `FBX/Taxi.fbx`, `License.txt` (CC0) |
| `Assets/ThirdParty/Kenney/CityKitRoads/` | `Models/FBX/` : `road-straight`, `road-bend`, `road-end`, `light-square` + `Textures/colormap.png`, `License.txt` (CC0) |
| `Assets/ThirdParty/Kenney/CityKitSuburban/` | `Models/FBX/` : `building-type-a`, `b`, `c`, `e`, `fence`, `tree-large`, `tree-small` + `Textures/colormap.png`, `License.txt` (CC0) |

Noms de modèles d'origine conservés. La structure `Models/FBX/Textures/` reproduit celle de l'archive pour que les FBX retrouvent leur texture. Deux modèles importés au départ (`fence-low`, `planter`) n'ont finalement pas été placés : fichiers et prefabs supprimés avant ce rapport.

Réglages d'import : animation désactivée, `Read/Write` désactivé, aucune caméra ni lumière, aucun collider généré, compression de maillage désactivée. Textures `colormap` en filtre point, sans mipmaps, non compressées (palettes : pas de bavure ni de flou).

## Matériaux et prefabs adaptés

- `Assets/_Project/Materials/Environment/` (URP Lit) : `Kenney_Roads_Colormap`, `Kenney_Suburban_Colormap`, `Taxi_Yellow/Black/Windows/Headlights/Grey/TailLights` (couleurs reprises du modèle), `Env_Lawn`. Chaque FBX est remappé vers ces matériaux dans son importeur.
- `Assets/_Project/Prefabs/Environment/` : un prefab enveloppe par modèle (`Env_Taxi`, `Env_RoadStraight|Bend|End`, `Env_LightSquare`, `Env_HouseA|B|C|E`, `Env_Fence`, `Env_TreeLarge|Small`), enfant `Model` mis à l'échelle. Aucun collider.
- Échelles retenues : taxi × 1,4 (≈ 5,9 × 2,5 × 1,9 m), tuiles de route × 10 en plan et × 0,3 en hauteur, bâtiments, arbres et lampadaire × 7, clôture × 4.

## Scène (via l'éditeur Unity)

- **Taxi** : les primitives de `Vehicle` sont désactivées (non supprimées, comparaison possible) et le prefab `Env_Taxi` est ajouté sous le nom `TaxiModel`. Racine `CollectiveTaxi`, `NetworkObject`, scripts, sièges, bornes, trajet et points de dépose **inchangés**. Pivot au sol, avant vers +Z comme le véhicule : aucune compensation sur la trajectoire. Échelle × 1,4 choisie pour que la caméra du joueur assis (œil à 1,6 m) reste sous le toit (1,83 m).
- **Route A–B** (`City/Dressing/Road`) : tuiles Kenney de 10 m dont la chaussée mesure 6 m, donc exactement la chaussée réservée. Deux virages (rotations 270° et 0°), 12 tuiles droites en bas, une droite et une extrémité à l'ouest, une extrémité à l'est (étirées de 0,85 à 1,1 selon la longueur restante), deux lampadaires aux quais. Les trois `RoadSurface_*` greybox sont masqués (rendu seulement) ; trottoirs, quais, bordures et colliders conservés.
- **Résidentiel** (`City/Dressing/Residential`, `Lawns`) : rendu des blocs gris `Block_02/04/06/07/11/12` masqué (colliders conservés), pelouses plates à leur place, 8 maisons (A, B, C, E), 11 segments de clôture le long de la promenade, 7 grands et 3 petits arbres. Coloc, porte verte, balise verte, carrefour bleu et arrêts non touchés ; aucune entrée interactive.
- **Seul ajustement de collision** : la hauteur des deux bordures de quai (`Kerb_A`, `Kerb_B`) passe de 1,2 m à 0,5 m (même emprise). À 1,2 m elles cachaient la moitié du taxi vu du quai ; à 0,5 m elles recoupent toujours la capsule joueur et bloquent le passage.
- Scène sauvegardée, non modifiée par le Play Mode.

## Vérifications

| Contrôle | Résultat |
| --- | --- |
| Compilation, console | aucune nouvelle erreur (console hôte propre) |
| Audit de scène | 177 renderers, **0** matériau manquant, rose ou non URP ; 0 `Missing Script` ; 0 prefab manquant ; aucun importeur avec lecture, animation ou collider |
| Hôte + client, trajet A → B injecté (téléphone/portefeuille pris, deux joueurs sur le quai A) | `PassengersToB`, joueurs sur les sièges, `NetworkTransform` et collider suspendus, `IsRiding` vrai sur les deux instances ; arrivée `AtB` après 12 s, déposes `(−124,2 ; 12)` et `(−124,2 ; 15)`, composants réactivés : identique à P1-055 |
| Appel à vide depuis A (taxi à B) | `Called` → `EmptyToA`, arrivée `AtA` ; aucun joueur déplacé, `NetworkTransform` actif |
| Collision | `TaxiModel` : 0 collider. Les colliders de blocs, bordures et périmètre sont ceux de P1-054/P1-055 (sinon seule la hauteur de bordure ci-dessus change) |
| Rendu à 960 × 540 depuis une caméra de hauteur joueur | taxi identifiable ; porte verte, balise, carrefour bleu et arrêt A lisibles ; maisons sans obstruction du passage |

Réserve : le client Multiplayer Play Mode a affiché trois fois « Asset Database is set to Read Only, but it has found out-of-date assets » après l'import (le clone n'a pas réimporté seul). Sans effet sur la session, ni côté hôte.

## Captures (3 au maximum, 960 × 540)

- `Docs/Reports/img/P1-056-taxi-player-view.png` : taxi à l'arrêt A, vu du quai à hauteur de joueur.
- `Docs/Reports/img/P1-056-residential-from-home.png` : quartier résidentiel, quai A, borne et taxi, vus depuis la sortie de la coloc.
- `Docs/Reports/img/P1-056-taxi-interior-issue.png` : **limite connue** — vue depuis le siège. Le modèle n'a pas de face intérieure : le joueur assis voit l'extérieur à travers la coque, avec capot, rétroviseurs, roues et enseigne inversée visibles. Aucun traversement du toit.

## Limites et à valider par Dyllan

- Échelle (taxi un peu long, maisons à 7 ×) et cohérence : à juger en jeu. Toits verts et pelouse peuvent réduire le contraste avec la coloc ; la balise verte reste le repère.
- L'intérieur du taxi : face intérieure à ajouter ou masquer la coque pour le joueur propriétaire, dans une fiche séparée.
- Blocs gris restants volontairement : promenade, ateliers et centre ; fermetures et alentours de la coloc sans habillage ; route non habillée au-delà des tuiles et lampadaires.
- Demi-tour du taxi à l'arrivée (P1-055) non revu avec le nouveau modèle.
- Aucun post-traitement, éclairage artistique, PNJ, mobilier, son ni autre pack.

## Nettoyage et état Git

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) : recréés par Unity pendant les essais, même GUID `6caa043d…`, non suivis, non référencés ; supprimés. `ProjectSettings` inchangé. Changements non commités : `BarGreybox.unity`, `Assets/ThirdParty/` (+ `.meta`), `Assets/_Project/Materials/Environment/`, `Assets/_Project/Prefabs/Environment/`, les trois captures et ce rapport.

## Prochaine action recommandée

Dyllan regarde un aller-retour avec le nouveau taxi et le résidentiel. Selon son retour : réglage d'échelle, face intérieure du taxi, habillage de la place et du centre dans une fiche suivante.
