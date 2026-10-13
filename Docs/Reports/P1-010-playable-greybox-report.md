# Rapport P1-010 — Greybox jouable

**Statut : PASS.** Parcours, boucle complète à deux joueurs, portage depuis le bar, victoire et relance observés (hôte + client en Multiplayer Play Mode), puis déplacements, collisions, visée et lisibilité validés par Dyllan avec de vraies entrées le 13 octobre 2026.

## Départ

Branche `feature/p1-001-network-foundation`, commit `595888e`, arbre propre. Unity 6000.3.8f1, aucun package ajouté, aucun asset importé de `D:\Assets\LastCallCrew`.

## Stratégie de scène

`NetworkTest.unity` est inchangée (aucune ligne modifiée). Nouvelle scène `Assets/_Project/Scenes/BarGreybox.unity`, copie de `NetworkTest` puis construction autour du socle ; scripts, prefab joueur, `TuningConfig` et `DefaultNetworkPrefabs` (`_Project/Prefabs`, même GUID) sont partagés.

## Plan et dimensions finales (mètres, sens +Z = vers le bar)

- Zone de départ (bleue) : x −3…9, z −5…5. Points de départ inchangés (formule existante `RestartRules.StartPosition`) : hôte (0,1,0), client (3,1,0) ; un client ultérieur arrive à x = 6, 9… toujours sur la zone.
- Chemin vers le bar (pierre) puis tapis d'entrée orange ; entrée : porte de 3 m (x 0…3) en z = 8, linteau et montants orange, hauteur libre 2,6 m.
- Salle : x −7…10, z 8…24 (17 × 16 m), murs 4 m d'épaisseur 0,4, sol bois, sans toit. Comptoir 10 × 1,1 × 1,2 (x −2…8, z 22,8…24). Quatre tables carrées 1,6 × 0,8 × 1,6 à (−4 ; 13), (−4 ; 18), (7 ; 12,5), (7 ; 19,5). Passages ≥ 2,2 m (axe central ≥ 3 m).
- Marcel (`NpcDialogue`) à (−4,5 ; 21), proposition de boisson (`DrinkOffer`) à (2 ; 22,3) devant le comptoir.
- Sortie : porte de 3 m (z 14,5…17,5) dans le mur est, linteau et montants **turquoise**, tapis turquoise, chemin de pierre de 5,6 m jusqu'à la zone de retour.
- Zone de retour : `ReturnZone` déplacée en (20 ; 0 ; 16), tournée de 90° (portique ouvert côté ouest, 8 × 8 m), **verte** avec colonne verte émissive de 14 m (`ReturnBeacon`, sans collider).
- Clôture périmétrique basse (1,5 m) ; sol existant agrandi (échelle 7, 70 × 70 m) ; `Overview Camera` repositionnée (8 ; 34 ; −16).
- Couleurs distinctes : asphalte, départ bleu, entrée orange, sortie turquoise, retour vert, murs clairs, bois, comptoir/tables bruns. 11 matériaux URP/Lit simples dans `Assets/_Project/Materials/Greybox/`.

## Objets / références modifiés

- Scène : hiérarchie `Greybox` (Paths, Walls, Furniture, Fence, Markers) ; `DrinkOffer`, `NpcDialogue`, `ReturnZone`, `Ground`, `Overview Camera` déplacés. `NetworkManager`, `NightTimer`, `NightOutcome`, `NightRound`, `IntoxicationHud` conservés avec leurs références.
- Vérification après duplication : `NetworkManager` pointe le même `DefaultNetworkPrefabs` (guid `afee2fb6…`) et le prefab joueur `64d669df…` ; les cinq `NetworkObject` de scène ont reçu de nouveaux `GlobalObjectIdHash` propres à la scène (écrits à l'enregistrement, différents de `NetworkTest`) ; hôte et client chargent le même asset. Les deux joueurs virtuels ont rouvert la scène depuis l'asset.
- **Build Settings** : `BarGreybox` ajoutée (NGO avec gestion de scène l'exige, sinon un client ne peut pas synchroniser — dialogue observé en Play). `ProjectSettings/EditorBuildSettings.asset` modifié.
- **Code** : `PlayerController.cs` — nouvelle poussée horizontale hors des colliders solides (`PushOutOfObstacles`, après le déplacement du propriétaire). Nécessaire : les joueurs se déplaçaient par `transform` sans corps physique et traversaient tout mur. Elle ignore les autres joueurs, les triggers et le sol ; aucune règle réseau, durée ou commande modifiée.

## Collisions

Cubes avec BoxCollider = silhouette (murs, linteaux, comptoir, tables, clôture) ; tapis, montants de porte, marqueurs de sol et colonne sans collider. Capsule joueur (r 0,5) : portes de 3 m sans étranglement.

## Parcours et tests (hôte + client, MPPM, round 2→3)

Déplacement simulé par pas de 0,083 m (5 m/s à 60 Hz) vers des waypoints, avec la poussée appelée à chaque pas :
- Hôte : départ → porte → salle → comptoir → Marcel → table → sortie → zone de retour : tous les waypoints atteints. Client : départ → porte → salle → sortie (14,4 ; 16) → retour dans la salle : atteint.
- Blocages vérifiés (arrêt au contact) : mur sud, mur nord, mur est depuis l'extérieur (x ≈ 10,9), comptoir (z ≈ 22,3), table (z ≈ 11,7), deux montants de porte, `DrinkOffer`, clôture. Hauteur Y stable (1,00).
- Boire depuis (2 ; 19,4) : visée réelle du raycast, `accept -> Accepted (2,45 m)`. Dialogue de Marcel depuis (−1,4 ; 20,5) : ouverture + choix 1 acceptés par l'hôte.
- Réveil : client KO à (−5,7 ; 15,5), entre deux tables et près du mur ouest ; hôte à (−3,3 ; 15,6) visant le partenaire ; `WokenByWater (distance 1,96 m)`.
- Portage : même client KO dans la salle, `carrying/carried = True`, hôte porteur jusqu'à (17,9 ; 16) en traversant la porte est : aucun blocage, `result=Victory`, les deux « rentrés ».
- Relance (`RequestRestartRpc` côté hôte) : round 3, `Pending`, hôte (0 ; 1 ; 0) et client (3 ; 1 ; 0) sur chaque instance, debout, ni porté ni portant, retours effacés.
- EditMode : **174/174**. Compilation sans erreur. Console de jeu sans nouvelle erreur (les erreurs de console visibles datent d'avant, ou sont des timeouts de l'outil CLI pendant le dialogue ci-dessous).
- La manche 2 a démarré par un `ServerRestart` appelé par réflexion pour repartir d'un état propre après les tests de mur ; la relance finale utilise le chemin réel du bouton.

## Captures (3 prises au maximum)

Deux captures relues puis archivées hors dépôt dans `D:\Assets\LastCallCrew\PlaytestCaptures\P1-010` : vue générale et sortie vers la zone verte. Elles ne sont pas versionnées à cause d'un échec local du filtre Git LFS. **Pas de capture de l'intérieur du bar** : la troisième tentative n'a pas déplacé la vue et a été supprimée (doublon). Vues de l'éditeur, pas de Game View.

## Incidents

- Au premier Play, NGO a ouvert un dialogue modal « scène absente des scenes in build » sur l'éditeur principal et le joueur virtuel 2, bloquant les commandes CLI ; fermé via l'automatisation de fenêtre, puis scène ajoutée aux Build Settings.
- `Assets/DefaultNetworkPrefabs.asset` recréé par NGO supprimé (conservé : `_Project/Prefabs/`).

## Limites / non réalisé

- `simulate_key` n'a produit aucun mouvement : toute la marche est simulée (le code de poussée est le vrai, pas la lecture des touches). Reste à jouer au clavier/souris : angles de portes, glissement le long des murs (la poussée est horizontale simple, sans glissement tangentiel), distance de visée réelle, lisibilité du HUD sur ces fonds, capture souris (P1-015) dans la nouvelle scène.
- Pas de capture de l'intérieur, pas d'essai à trois, pas de build autonome.
- Les points de départ restent codés (x = 3 × id) : un client ayant un identifiant ≥ 4 démarrerait hors de la zone bleue (x ≥ 12, près du mur du bar).
- La colonne de retour est visible de loin depuis l'extérieur ; depuis la salle elle est cachée par les murs (pas de toit, vue plongeante de l'éditeur seulement).
- Le playtest a révélé un défaut antérieur de P1-031 : sortir de la zone conserve le statut « rentré ». Il ne provient pas de la greybox et fait l'objet de P1-031a.

## Git

Modifications à committer : `PlayerController.cs`, `ProjectSettings/EditorBuildSettings.asset`, nouvelle scène + `.meta`, `Assets/_Project/Materials/` (+ `.meta`), deux captures et ce rapport. Aucun push. Prochaine action : corriger P1-031a avant de poursuivre la boucle.
