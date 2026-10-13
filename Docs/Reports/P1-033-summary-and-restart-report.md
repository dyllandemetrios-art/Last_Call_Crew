# Rapport P1-033 — Bilan et relance

**Statut : PASS** (hôte + client, Multiplayer Play Mode, trois manches successives dont deux relances). Dyllan a ensuite validé le clic réel sur `RELANCER` pendant deux manches successives.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `6fb4428`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés : `Night/RestartRules.cs` (règles pures), `Network/NightRound.cs` (manche et relance), `Tests/EditMode/RestartRulesTests.cs` (12 cas), `.meta`.

Modifiés :
- `Scenes/NetworkTest.unity` : objet `NightRound` (+77 lignes, ajouts uniquement, via l'éditeur).
- Gel : `PlayerController`, `PlayerInteractor`, `PlayerCarry`, `PlayerWake`, `PlayerIntoxication`, `ReturnZone`, `NightTimer`, `NightOutcome`.
- Remises à zéro : `PlayerCarry`, `PlayerWake`, `PlayerReturn`.
- Énumérations de rejet : `RejectedFrozen` ajouté dans `DrinkRules`, `CarryRules`, `WakeRules`.
- HUD : `IntoxicationHud.cs` (bilan, nettoyage de présentation).
- Non modifiés : `TuningConfig` et règles du chronomètre / de victoire.

## Données du bilan

Carte centrale sous le message KO : résultat (texte P1-032), `TEMPS RESTANT MM:SS` (temps figé à l'instant de la décision, `00:00` en échec), puis par joueur : `RENTRÉ/DEHORS · KO/DEBOUT` et `IVRESSE n`. Hôte : bouton `RELANCER` ; client : `EN ATTENTE DE L'HÔTE`. Les invites d'action sont masquées pendant le bilan ; le compte à rebours du réveil automatique aussi. Mêmes données sur les deux instances (états répliqués, gelés).

## Gel local et serveur

- **Local** : `PlayerController` ne lit plus déplacement ni regard ; `PlayerInteractor` n'accepte plus aucune interaction et abandonne les maintiens ; caméra et HUD restent actifs. Le gel dérive du résultat répliqué (`NightOutcome.IsFrozen`).
- **Hôte** : refus `RejectedFrozen` pour boire, refuser (début et fin), porter, lâcher, réveiller (début et fin). Trois évolutions dans le temps sont aussi gelées : décroissance d'ivresse, réveil automatique, enregistrement des retours ; le chronomètre ne passe plus à `Expired` et son temps affiché reste celui de la décision.
- Preuves (journal hôte), RPC forcés par réflexion après la victoire : depuis l'hôte `drop request`, `wake start`, `wake end`, `accept`, `refuse-start` → `RejectedFrozen` ; depuis le client `carry request`, `wake start`, `accept` → `RejectedFrozen`. Aucun état n'a changé (portage, KO, ivresse, positions identiques avant/après). Déplacement : `W` maintenu 1,5 s sur l'hôte (porteur) et sur le client (debout, manche 2) : positions inchangées.

## Ordre de remise à zéro (hôte, `NightRound.ServerRestart`)

1. Portages (positions de dépose = points de départ), maintiens de refus, maintiens et échéances de réveil.
2. Sortie de KO de chaque joueur, ivresse au minimum (`TuningConfig`), retours effacés, et pose de départ envoyée au propriétaire (`ResetPoseRpc`, car la position est en autorité propriétaire).
3. Chronomètre à `Waiting`, résultat à `Pending`, puis compteur de manche +1 (dans la même frame : l'évaluation de `LateUpdate` ne peut pas conclure à partir d'anciennes données). Le chronomètre repart seul avec les joueurs requis (une fois).

Bug trouvé pendant les tests : juste après la relance, le joueur client était de nouveau marqué « rentré », car l'hôte voyait encore son ancienne position dans la zone avant que son propriétaire ait reçu la pose de départ. Corrigé : `PlayerReturn.ServerPoseSettled` ignore la position tant que la pose de départ n'est pas arrivée (ou 3 s). Les manches suivantes l'ont prouvé.

## Compteur de manche

`NightRound._round` (`NetworkVariable<int>`, 1 à la première manche), incrémenté à chaque relance. L'hôte relance seulement depuis un résultat terminal (`RestartRules.Evaluate`) : une demande répétée trouve `Pending` et est ignorée. Chaque instance nettoie une seule fois sa présentation HUD (compteurs de feedback et textes temporaires) quand le compteur change ; aucun abonnement n'est ajouté à chaque manche (le HUD compare un entier).

## Scénarios

| Scénario | Résultat |
|---|---|
| Manche 1 : l'hôte porte le client KO dans la zone | `Victory`, gel confirmé, RPC forcés rejetés, bilan identique hôte/client |
| Client tente de relancer | `restart request from client 1 -> RejectedNotHost`, aucun changement |
| Relance par l'hôte | `Restarted`, `round 2 started` ; deux joueurs debout, ivresse 0, non portés, collider actif, retours effacés, positions de départ (0 ; 1 ; 0) et (3 ; 1 ; 0) ; `night started` une fois, ≈ 290 s restantes sur l'hôte et 288 s sur le client ; résultat `Pending` |
| Six secondes plus tard | rien de parasite (aucun retour, résultat inchangé) |
| Manche 2 : un seul rentré, échéance ramenée à +5 s | `Failure`, bilan « DEHORS · DEBOUT », `TEMPS RESTANT 00:00` |
| Client debout : W et demande de relance | position inchangée ; relance refusée (`RejectedNotHost`) |
| Rejoin pendant le bilan d'échec | le nouveau client (id 2) reçoit `Failure`, manche 2, sans RPC de rattrapage |
| Relance 2 | `round 3 started`, tous les états initiaux ((0 ; 1 ; 0) et (6 ; 1 ; 0) pour le client id 2) |
| Manche 3 : les deux rentrent | `Victory` (résultat différent du précédent) |
| Double demande de relance dans la même frame | `Restarted` puis `RejectedNotFinished` : une seule manche (round 4) |
| Après trois relances | déplacement fonctionnel (z 0 → 5), boire/KO, portage `Started` |

Les relances ont été déclenchées par `NightRound.RequestRestartRpc()` appelé sur l'hôte, qui est le code du bouton. L'échéance de la manche 2 a été réécrite par réflexion (comme pour P1-032), sans toucher à l'asset. Les KO, téléportations et retours dans la zone sont injectés par script.

## Interface

Captures minimales : bilan victoire 1280×720 (hôte), bilan échec 1280×720 (hôte), une vérification finale 1920×1080 (client KO porté, victoire) : bilan lisible sous le message KO, sans chevauchement avec NUIT, le panneau Ivresse ni le panneau réseau. Défauts corrigés avant : « TEMPS RESTANT » aligné à gauche, compte à rebours de réveil à « 0 s » pendant le bilan. Pas de capture après relance (états prouvés par valeurs inspectées).

## Tests

- Compilation : 0 erreur. **157 tests EditMode sur 157 réussis** (145 + 12). Console : aucune erreur de jeu.
- Une seconde instance cliente est apparue en fin de session ; non utilisée (P1-033 suppose deux joueurs).

## Erreurs rencontrées

- Le retour fantôme après relance (voir plus haut), corrigé.
- Des erreurs « NetworkPrefab hash was not found » restent dans l'historique de la console d'un joueur virtuel qui avait gardé une scène périmée ; résolues en rouvrant la scène sur chaque joueur virtuel.

## Limites

- Pas de test à trois joueurs ni de build autonome ; rejoin testé avec le même éditeur virtuel.
- Les déconnexions pendant une manche ou une relance restent P1-040 : un client qui revient reçoit un nouvel identifiant et un nouveau point de départ.
- La relance suppose un roster stable d'au moins deux joueurs.
- Aucun score ni statistique persistants (hors périmètre).

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés ; aucun changement de `ProjectSettings`.

## Validation manuelle de Dyllan

Le 13 octobre 2026, Dyllan a terminé deux manches successives et utilisé le bouton `RELANCER` réel sur l'hôte. Les deux relances ont remis les joueurs dans l'état initial attendu, sans double relance ni état résiduel.

## Prochaine action recommandée

Consolider le jalon de boucle jouable, puis choisir la prochaine fiche. P1-040 reste bloqué tant que Relay et le build Windows à deux joueurs (P1-003/P1-004) ne sont pas validés.
