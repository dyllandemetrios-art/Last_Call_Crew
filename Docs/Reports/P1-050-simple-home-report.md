# Rapport P1-050 — Maison simple

**Statut : PASS technique.** Maison, règle d'activation autoritaire, HUD, relance, portage et reconnexion sont validés en hôte + client (Multiplayer Play Mode, états injectés par script). Le parcours manuel spécifique à la nouvelle porte est **différé et non bloquant** : déplacements, collisions, portage et retours avaient déjà été validés aux étapes précédentes (P1-010, P1-031a, P1-040).

## Départ

Branche `feature/p1-001-network-foundation`, commit `1be60c0`. Documents Codex non commités laissés tels quels (`Docs/Backlog.md`, fiches/rapports P1-041, P1-042, P1-043, P1-050). Aucun package ajouté.

## Implantation et dimensions (mètres)

Maison autour de `ReturnZone`, centre (20 ; 0 ; 16), sous `Greybox/House` dans `BarGreybox.unity` :
- intérieur 10 × 10 (x 15…25, z 11…21), murs 0,4 × 3,5 ; sol crème ; tapis vert de la zone conservé ;
- porte ouest de 3 m (z 14,5…17,5, linteau à 2,6 m, montants verts), face au chemin de la sortie du bar (chemin extérieur raccourci à x 12…14,6 pour ne pas entrer dans le mur) ;
- toit rouge sans collider ; lumière ponctuelle intérieure ; ancien portique supprimé, repère vert vertical déplacé sur le toit ;
- trigger de retour séparé des murs, 9 × 3 × 9 couvrant l'intérieur utile ;
- matériaux simples : `HouseWall`, `HouseRoof`, `HouseFloor`, `HouseDoor`. Aucun asset tiers.

## Règle d'activation

`Night/HouseExitTracker.cs` (pur) : mémorise qui a déjà quitté la maison ; `TryActivate(joueurs)` n'est vrai qu'une fois, quand au moins deux joueurs sont tous sortis au moins une fois ; irréversible jusqu'à `Reset`. Sortir puis revenir avant son partenaire garde la sortie mémorisée.

## Autorité réseau

- `ReturnZone` (hôte seul) lit les positions, applique le portage (un joueur porté suit son porteur, aussi pour la sortie), nourrit le tracker et écrit **une seule fois** `NightOutcome._returnActive` (NetworkVariable bool, remis à faux à la relance). Aucun RPC client.
- Retour inactif : personne n'est marqué rentré et `NightOutcome` évalue avec une liste vide (aucune victoire possible ; l'échec à l'échéance et celui de P1-040 restent inchangés). Retour actif : instants d'entrée de P1-031a.
- Protection des poses : un joueur dont la pose de départ n'est pas encore arrivée sur l'hôte (apparition ou relance) n'est ni présent ni compté sorti (`PlayerReturn.ServerExpectPose` à l'apparition ; l'ancienne protection de relance est étendue).
- Relance : `NightRound` appelle `ReturnZone.ServerResetForRound()` avant de replacer les joueurs.

## Points de départ

`RestartRules.StartPosition(emplacement)` = (21 ; 1 ; 15,2 + 1,6 × emplacement), orientation 270° (face à la porte). Emplacements par ordre des joueurs connectés (P1-040), jamais par `ClientId`. Observé : (21 ; 1 ; 15,2) et (21 ; 1 ; 16,8) au départ, après relance et après reconnexion (client d'identifiant 2).

## Interface

Carte NUIT : « MAISON · QUITTEZ LES LIEUX » (orange) avant activation ; carte joueur « MAISON » à la place de « ENCORE DEHORS » avant activation ; feedback « RETOUR ACTIVÉ » 2 s sur les deux instances (un arrivant tardif ne le déclenche pas) ; ensuite « ENCORE DEHORS » / « RENTRÉ ». Capture unique d'un build (intérieur de la maison, HUD lisible) vue pendant l'essai ; non conservée dans le dépôt.

## Tests

EditMode **210/210** (199 + 11) : `HouseExitTrackerTests` (personne sorti, un seul sorti, tous sortis, sortie puis retour anticipé, activation irréversible, remise à zéro, un seul joueur connecté, identifiants 0 et 47, poses dans la maison sans chevauchement) et `RestartRulesTests` adaptés. Compilation sans erreur.

## Validation hôte + client (Multiplayer Play Mode, positions injectées)

| # | Scénario | Résultat |
|---|---|---|
| 1 | départ | hôte (21 ; 15,2), client (21 ; 16,8), yaw 270, `returned=False`, `active=False`, résultat `Pending` sur les deux instances |
| 2 | l'hôte sort puis revient | retour toujours inactif, personne marqué rentré |
| 3 | le client sort | `[P1-050] return activated` **une seule fois**, `active=True` identique sur les deux instances ; l'hôte, dans la maison, est marqué rentré |
| 4 | les deux reviennent | `Victory` normale |
| 6 | relance | `round 2`, joueurs replacés dans la maison, `active=False`, aucun état résiduel |
| 5 | client KO dans le bar, porté par l'hôte jusqu'à la maison | retour actif (les deux étaient sortis), porté dehors = non rentré, porteur dans la maison avec le partenaire KO : `Victory`, les deux rentrés |
| 7 | reconnexion (client id 2) puis relance | poses (21 ; 15,2) et (21 ; 16,8), retour inactif, aucune erreur hôte |

Aucune erreur dans la console hôte pendant les essais.

## Changements annexes

- `NetworkTestHud.cs` : le journal `[P1-001]` ajoute `yaw=` (diagnostic d'orientation) ; sans effet de jeu.
- `Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) : réapparu en début de tâche, supprimé (même GUID `6caa043d…` que lors des contrôles P1-040 : doublon NGO non référencé) et de nouveau après le dernier test ; non re-comparé octet à octet cette fois.
- `ProjectSettings/ProjectSettings.asset` : l'éditeur a un moment remis `preloadedAssets` à vide ; rétabli à l'état du dépôt.

## Limites

- **Parcours maison → bar → maison avec de vraies entrées : différé, non bloquant** (automatisation système interrompue : risque d'entrées envoyées à une autre application). Déplacements, collisions et portage déjà validés ; seule la porte de 3 m de la maison reste à parcourir à l'occasion d'un prochain playtest.
- Build Windows Development reconstruit (`D:/REPOSITORY/LCC_TestBuild/P1-050/`) et premières vérifications en build (apparition dans la maison, HUD) faites ; la suite des scénarios a été jouée dans l'éditeur avec états injectés.
- **Élément inutilisé à nettoyer plus tard** : l'ancienne zone de départ bleue (`Greybox/Paths/StartPad`) et son chemin vers le bar, conservés pour l'instant ; la caméra de vue d'ensemble voit le toit.
- Raison du résultat de victoire : `Deadline` (valeur existante de P1-040, non affichée).

## Git

Modifications non commitées : scène `BarGreybox.unity`, scripts réseau et règles Night, nouveaux `HouseExitTracker.cs` et `HouseExitTrackerTests.cs`, matériaux `House*`, ce rapport. Aucun commit, aucun push. Prochaine action : nettoyage ultérieur de la zone bleue ; parcours de la porte à l'occasion d'un prochain playtest.
