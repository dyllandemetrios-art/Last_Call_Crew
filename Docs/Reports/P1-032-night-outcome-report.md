# Rapport P1-032 — Victoire et échec de la nuit

**Statut : PASS** (hôte + client, Multiplayer Play Mode, validation manuelle de Dyllan le 13 octobre 2026). Test à trois joueurs : **non réalisé**, couvert par les tests purs. Les contrôles ne sont pas bloqués (P1-033).

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `08f01f0`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Night/NightOutcomeRules.cs` (assembly `LastCallCrew.Night`) : règles pures.
- `Scripts/Runtime/Network/NightOutcome.cs` : composant réseau de décision.
- `Tests/EditMode/NightOutcomeRulesTests.cs` : 15 tests.
- `.meta` correspondants.

Modifiés :
- `Scenes/NetworkTest.unity` : objet `NightOutcome` (`NetworkObject` de scène + composant, référence au `NightTimer`), +75 lignes, ajouts uniquement, via l'éditeur.
- `NightTimer.cs` : propriété en lecture seule `DeadlineServerTime` (aucune règle du chronomètre changée).
- `IntoxicationHud.cs` : bandeau de résultat.
- Non modifiés : `TuningConfig`, `PlayerReturn`, `ReturnZone`, KO, portage, réveil.

## Règles exactes

`NightOutcomeRules.Evaluate(étatChrono, échéance, instantsDeRetour)` :
- chronomètre `Waiting` → `Pending` ;
- **Victoire** : au moins 2 joueurs et chacun a un instant de retour enregistré **≤ échéance** (égalité acceptée) ;
- **Échec** : chronomètre `Expired` et pas de victoire ;
- sinon `Pending`.
`Next(actuel, évalué)` : un résultat déjà terminal n'est jamais remplacé. Un joueur porté marqué rentré compte comme les autres (même `PlayerReturn`).

## Autorité

`NightOutcome` : deux `NetworkVariable` écrites uniquement par l'hôte, une seule fois (résultat et temps serveur de la décision). Aucun RPC, aucune demande client, aucun trafic continu (deux écritures par partie). Un client qui rejoint reçoit l'état par les `NetworkVariable`. L'hôte évalue tous les `PlayerReturn` des joueurs connectés.

## Course à l'échéance

La décision compare des instants serveur (`PlayerReturn.ReturnServerTime` et `NightTimer.DeadlineServerTime`), pas l'ordre des `Update`. Pour qu'un retour enregistré pendant la même frame que l'expiration soit déjà visible, `NightOutcome` évalue dans `LateUpdate` (après `ReturnZone.Update` et `NightTimer.Update`).

## Scénarios (journaux hôte et client)

| # | Scénario | Résultat observé |
|---|---|---|
| 1 | Hôte rentre à 16,63 s, client à 24,95 s (échéance 307,43) | `Pending` après le premier, `Victory` décidée à 24,945 dans la même frame du second retour ; identique sur l'hôte et le client |
| 2 | Un seul rentré (17,10) ; échéance ramenée à +6 s | `Failure` décidée à l'échéance (28,663) ; identique partout |
| 3 | Second joueur rentre après zéro (t = 50,41) | résultat inchangé : `Failure`, décidée 28,663 |
| 4 | Porteur à (0 ; 1 ; 16) avec le partenaire KO porté | les deux rentrés au même instant (28,875) → `Victory` |
| 5a | Course : retour du 2ᵉ exactement à l'échéance (31,175) | `Victory` |
| 5b | Course : 1 ms après l'échéance (31,327 pour 31,326) | `Failure` |
| 5c | Course : 1 ms avant l'échéance (31,613 pour 31,614) | `Victory` |
| 7 | Rejoin après victoire | le nouveau client (id 2, pas rentré) reçoit `Victory`, décidée 24,945, sans RPC |
| 7 | Rejoin après échec | le nouveau client reçoit `Failure`, décidée 28,663 |

Dans les scénarios 5, les instants de retour du second joueur ont été **injectés par réflexion** côté hôte (`échéance + décalage`) ; l'échéance a aussi été réécrite par réflexion (sans toucher à l'asset). Les retours des scénarios 1 à 4 viennent de vrais passages dans la zone (position téléportée par script).

## Troisième joueur

Non testé en jeu : seules deux instances (hôte + joueur virtuel) étaient disponibles. Couvert par `ThirdPlayer_MustAlsoBeBack` (3 joueurs, un manquant : `Pending` puis `Failure` ; tous rentrés : `Victory`), et par la conception : `NightOutcome` itère sur tous les clients connectés. À refaire à trois si Dyllan en dispose (il a déjà joué à trois).

## Interface

Bandeau central : « VICTOIRE — TOUT LE MONDE EST RENTRÉ » (vert) ou « ÉCHEC — QUELQU'UN EST RESTÉ DEHORS » (rouge), sous le message KO du joueur local s'il est KO, au-dessus des invites ; le chronomètre et les cartes joueur restent visibles. Captures relues à 1280×720 et 1920×1080, hôte (porteur) et client (KO porté). Deux défauts corrigés après les premières captures : le bandeau passait derrière le message KO, puis était tronqué à 1280×720 (largeur étendue à la largeur utile ; sans chevauchement car placé sous les panneaux latéraux).

## Coût réseau

Deux écritures de `NetworkVariable` (résultat + instant) par partie, plus la synchronisation initiale d'un client qui rejoint. Pas de RPC. Non mesuré avec un profiler.

## Tests

- Compilation : 0 erreur. **145 tests EditMode sur 145 réussis** (130 + 15).
- Consoles : aucune erreur de jeu. KO, portage, retour et chronomètre se sont comportés comme avant pendant tous les scénarios ; les contrôles restent actifs après le résultat.

## Erreurs rencontrées

- Aucune erreur de jeu. Une session lancée en arrière-plan a duré plus de 2 minutes ; un échange a été relancé en tâche de fond sans conséquence.

## Limites

- Pas de test à trois joueurs ni de build autonome ; rejoin testé avec le même éditeur virtuel.
- **P1-040** : un joueur qui se déconnecte avant le résultat disparaît de l'évaluation (son `PlayerReturn` est détruit). S'il ne reste qu'un joueur, la victoire est impossible (minimum 2) et la nuit finit en échec à l'expiration ; si le dernier restant est rentré avec un autre joueur parti, le test « tous » ne voit plus le disparu. Une reconnexion crée un nouveau joueur non rentré. Non résolu ici, comme demandé.
- La décision a lieu dans la frame où la dernière condition devient vraie ; l'instant enregistré est celui de cette frame (pas de rétro-datation).
- Le résultat n'est pas persisté entre parties ; pas de relance (P1-033).

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés. Aucun changement de `ProjectSettings`.

## Changements non commités

Tous les fichiers listés. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan joue une nuit complète à deux (puis à trois) pour juger le rythme, valider le bandeau, puis P1-033 (fin de partie et relance).
