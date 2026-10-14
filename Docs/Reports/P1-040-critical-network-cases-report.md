# Rapport P1-040 — Cas réseau critiques

**Statut : PASS**, avec deux écarts d'essai signalés (scénario « départ avant démarrage » et scénarios portage / maintien, voir plus bas). Essais faits avec de vrais processus Windows (jusqu'à trois en même temps, profils UGS distincts) et des entrées système réelles pour les clics, le collage et les arrêts ; les déplacements de test dans l'éditeur sont injectés.

## Départ

Branche `feature/p1-001-network-foundation`, commit `3fb9cd0`. Tous les changements P1-003, P1-004 et P1-015a non commités sont conservés. Unity 6000.3.8f1, aucun package ajouté.

## Fichiers modifiés

- Règles pures (`Scripts/Runtime/Night/`) : `NightOutcomeRules.cs` (+ `NightReason`, `OnClientDisconnected`, `Banner(résultat, raison)`), `RestartRules.cs` (`RejectedNotEnoughPlayers`, `Evaluate(…, joueursConnectés)`, `StartPosition(emplacement)` remplace `3 × ClientId`), **nouveau** `PlayerSlots.cs` (`FirstFree`, `Rank`).
- Réseau (`Scripts/Runtime/Network/`) : `NightOutcome.cs` (raison répliquée, abonnement au départ d'un client), `NightRound.cs` (emplacements par rang, refus de relance à moins de deux joueurs), `PlayerController.cs` (emplacement répliqué attribué par l'hôte), `IntoxicationHud.cs` (raison dans le bilan, bouton de relance désactivé), `NetworkTestHud.cs` (message d'erreur à la ligne), `RelayConnection.cs` (hôte parti, session pleine, libération de place).
- Tests : `NightOutcomeRulesTests.cs`, `RestartRulesTests.cs`.
- `ProjectSettings/UnityConnectSettings.asset` reste modifié par l'éditeur (déjà signalé en P1-004).

## Règles de chaque départ

| Cas | Résultat |
|---|---|
| Client quitte, chronomètre en attente (hôte seul) | rien : pas de résultat, la manche ne démarre pas (observé : hôte seul, aucune ligne `night started` ni résultat) |
| Client quitte pendant une manche (chronomètre en cours ou échéance atteinte, sans résultat) | échec immédiat `PartnerDisconnected`, raison répliquée avec le résultat, gel P1-033 |
| Client quitte après un résultat | résultat et raison inchangés (`no change`) |
| Hôte quitte | session Relay supprimée ; le client revient hors ligne avec « HÔTE DÉCONNECTÉ : la session est terminée… » |

Le premier résultat terminal reste immuable (`NightOutcomeRules.Next`). L'échec de déconnexion est écrit une seule fois (`_reason`, puis `_decidedAt`, puis `_result`, dans la même frame de réplication).

## Ordre des callbacks et course déconnexion / échéance

Sur l'hôte, `NightOutcome` s'abonne à `OnClientDisconnectCallback` à `OnNetworkSpawn` et se désabonne à `OnNetworkDespawn` (une fois par spawn ; ignoré pendant l'arrêt de NGO et pour l'hôte lui-même). Les événements de transport sont traités au début de la frame NGO, **avant** `NightTimer.Update`, `ReturnZone.Update` et le `LateUpdate` d'évaluation : une déconnexion survenant dans la même frame qu'une échéance ou qu'un retour collectif est donc décidée en premier et gagne (décision de l'hôte, premier résultat terminal). Une victoire déjà décidée à la frame précédente n'est jamais modifiée.

## Nettoyage des références

`PlayerCarry.OnNetworkDespawn` (déjà existant) lâche ou délie le partenaire. Vérifié : porteur avec partenaire KO porté, déconnexion du client → `carrying=False`, échec `PartnerDisconnected`, aucune erreur hôte ; réveilleur en plein maintien de réveil, déconnexion du client → aucune erreur, maintien abandonné par le gel. Les maintiens de refus appartiennent à l'objet joueur détruit. Le gel P1-033 abandonne aussi les maintiens locaux de l'hôte.

## Attribution des points de départ

Plus de `3 × ClientId`. À l'apparition, l'hôte attribue à chaque joueur le premier emplacement libre (réplication `PlayerController.Slot`) ; à la relance, emplacement = rang du `ClientId` parmi les joueurs connectés (hôte = 0). Position = `(emplacement × 3 ; 1 ; 0)`. Observé avec des identifiants 1, 2 et 3 après reconnexions : toujours (0 ; 1 ; 0) et (3 ; 1 ; 0). Tests : emplacements avec identifiants 0 et 47, et 5 et 2.

## Essais à deux et trois processus (build Windows Development)

Dossier `D:/REPOSITORY/LCC_TestBuild/P1-040/` (hors Git), `LastCallCrew.exe`, profils `p1-040-host`, `p1-040-client`, `p1-040-third`, journaux `host*.log`, `client*.log`, `third*.log` (suffixes = cycles). Builds successifs : `build 00c23920`, `f714c774`, `d9a23080` (identifiant affiché en bas à gauche).

| # | Scénario | Observé |
|---|---|---|
| 1 | mauvais code puis code valide | `ZZZZZ9` refusé, message à la ligne lisible à 960×540 ; code valide rejoint sans redémarrage |
| 2 | session pleine (troisième processus) | `SessionException: session is full` → « SESSION COMPLÈTE : la partie a déjà deux joueurs. Réessayez avec un autre code. » ; hôte et client inchangés (positions et manche continues) |
| 3 | départ avant démarrage | voir écart 1 |
| 4 | client quitte pendant la manche (ARRÊTER réel) | hôte : `client 1 disconnected … -> night Failure (PartnerDisconnected)` ; bilan « ÉCHEC — PARTENAIRE DÉCONNECTÉ », bouton `ATTENTE DU PARTENAIRE` désactivé |
| 4b | client tué brutalement (processus terminé) | détection au bout de ≈ 30 s (délai du transport NGO), même échec |
| 5 | reconnexion avec le même code | client (id 2) reçoit le bilan d'échec avec sa raison ; relance par clic réel : `Restarted`, positions (0 ; 1 ; 0) et (3 ; 1 ; 0) |
| 5b | idem avec un identifiant élevé (3) | positions identiques |
| 6 | client quitte après le résultat | `no change (result Failure…)`, TEMPS RESTANT figé (04:11), raison conservée |
| 7a | hôte quitte (ARRÊTER) | client hors ligne « HÔTE DÉCONNECTÉ… », puis le client héberge une nouvelle session (`TNNQNP`) sans redémarrer |
| 7b | hôte tué brutalement | le client détecte la perte au bout de ≈ 30 s, même message, hors ligne |
| 8 | client tué puis relancé avec le **même profil UGS** | place libérée par l'hôte (`released the session seat`, 32 s) puis le même code est rejoint ; sans cette libération : `player is already a member of the session` |

Après chaque arrêt, 0 processus restant ; aucune session fantôme observée (`Sessions.Count`, journaux `session stopped`).

## Journaux

11 journaux de joueurs : aucune exception ni erreur non provoquée. Les lignes `SessionException` (code invalide, session pleine, session introuvable, déjà membre) sont les échecs voulus des essais. `[P1-040]` : départ de client, absence de changement après résultat, perte de l'hôte, libération de place.

## Tests automatiques

EditMode **199/199** (190 + 9) : départ avant démarrage, pendant la manche (en cours / échéance atteinte), après résultat (victoire, échec), immuabilité du premier résultat, libellé de raison, relance à deux joueurs, emplacements avec identifiants élevés ou non consécutifs, réutilisation d'un emplacement libéré. Compilation sans erreur.

## Écarts et incidents

1. **Départ avant démarrage** : avec l'architecture actuelle, le chronomètre démarre dès que deux joueurs sont connectés ; un client ne peut donc pas partir « avant le départ » une fois connecté. Couvert par la règle pure (`Waiting` → inchangé) et par l'état observé de l'hôte seul. Non reproductible autrement sans modifier le démarrage de la manche.
2. **Portage et maintien** : réalisés dans l'éditeur (Multiplayer Play Mode) avec états et RPC injectés, pas avec de vraies entrées dans le build.
3. Ajout nécessaire non demandé : après un processus tué, la place du joueur restait réservée dans la session du service. L'hôte libère maintenant les places autres que la sienne à chaque déconnexion NGO (valable pour P1 à deux joueurs).
4. À chaque génération de build, l'éditeur a affiché « Scene(s) Have Been Modified » pour `BarGreybox` (sans écart dans Git) et a bloqué le CLI ; « Don't Save » choisi pour garder la scène du disque.
5. Un client tué est détecté en ≈ 30 s (délai de transport NGO par défaut) : pas de réglage dans cette étape.

## Limites

Deux (puis trois) processus sur la même machine et le même réseau ; pas de réseau dégradé ; aucune reprise d'identité ou d'état du joueur parti ; pas de migration d'hôte ; les emplacements supposent au plus deux joueurs. Entrées système automatisées : ressenti et lisibilité à valider par Dyllan.

## Doublon NGO

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) à nouveau confirmé : contenu identique à `_Project/Prefabs/DefaultNetworkPrefabs.asset`, GUID `6caa043d…` référencé nulle part ; supprimé après le dernier test. Aucun commit, aucun push.
