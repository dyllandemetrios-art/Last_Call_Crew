# Rapport P1-031a — Présence réelle dans la zone de retour

**Statut : PASS** (règles automatiques + hôte/client en Multiplayer Play Mode dans `BarGreybox.unity`). Dyllan a ensuite validé le scénario réel entrée → sortie → second joueur seul → retour simultané le 13 octobre 2026.

## Départ

Branche `feature/p1-001-network-foundation`, commit `9a379a2`, arbre propre. Aucun package ajouté.

## Reproduction initiale et cause

Reproduction fournie par la fiche (non rejouée avant correctif) : A entre, ressort, reste « RENTRÉ » ; B entre ; victoire à tort. Cause confirmée à la lecture du code : `PlayerReturn.ServerMarkReturned` conservait le premier instant sans transition inverse, et `ReturnZone` ne traitait que les joueurs dedans.

## Fichiers modifiés

- `Scripts/Runtime/Night/ReturnRules.cs` : `ShouldRecord`/`Record` remplacés par `Next(courant, présent, maintenant)` (entrée → instant, maintien → instant conservé, sortie → `NotReturned`).
- `Scripts/Runtime/Network/PlayerReturn.cs` : `ServerMarkReturned` remplacé par `ServerSetPresence(présent, maintenant)`, écriture de la `NetworkVariable` seulement si la valeur change.
- `Scripts/Runtime/Network/ReturnZone.cs` : présence calculée à chaque frame par l'hôte pour tous les joueurs.
- `Tests/EditMode/ReturnRulesTests.cs` : tests de règles remplacés/ajoutés.
Inchangés : `NightOutcome`, `NightOutcomeRules`, HUD, relance, gel, scène.

## Transitions autoritaires (hôte seul)

1. Pour chaque joueur : position dans le trigger ; après relance, `ServerPoseSettled` garde sa protection (ancienne position ignorée tant que la pose de départ n'est pas arrivée).
2. Un joueur porté est présent si et seulement si son porteur l'est (`CarrierClientId`) ; un KO non porté suit sa propre position.
3. `ServerSetPresence` : entrée = nouvel instant serveur, maintien = rien, sortie = effacement, nouvelle entrée = nouvel instant. Journaux `[P1-031] … entered return zone` / `[P1-031a] … left return zone`.
Aucune demande client n'est lue. Le gel P1-033 (`NightOutcome.IsFrozen`) court-circuite toujours `ReturnZone.Update`, donc après décision rien ne change.

## Coût réseau

Écritures seulement aux transitions. Mesure sur l'hôte (callbacks `OnValueChanged` sur les deux `PlayerReturn`) : entrée + 4 s dedans → 1 changement ; sortie puis nouvelle entrée → 3 au total ; l'autre joueur : 0.

## Tests automatiques

EditMode **178/178** (174 avant ; 5 tests supprimés, 9 ajoutés) : sentinelle, instant ≥ 0, entrée, entrée à t = 0, maintien conserve l'instant, sortie efface, dehors reste dehors, nouvelle entrée remplace l'ancien instant, et A sort / B entre → `Pending` puis A rentre → `Victory` via `NightOutcomeRules`. L'égalité à l'échéance (victoire) / après (échec) reste couverte par les tests existants de `NightOutcomeRules`. Compilation sans erreur, console hôte et client sans nouvelle erreur.

## Tests manuels (hôte + client, états relus sur les deux instances)

| Scénario | Résultat |
|---|---|
| A entre | A rentré (@66,2) chez les deux |
| A ressort | A dehors chez les deux |
| B entre, A dehors | B rentré, `Pending`, pas de victoire |
| A rentre, B dedans | `Victory` |
| Relance | `Pending`, tous dehors, positions de départ |
| A entre, sort, rentre | instant 66,2 → effacé → 81,1 |
| A porte B (KO), entre | A et B rentrés (même instant 116,3) ; décision retardée en désactivant l'évaluation de `NightOutcome` pour le test |
| A porteur ressort | A et B dehors ensemble |
| A porteur rentre (@125,8) puis évaluation réactivée | `Victory` |
| KO non porté, B hors zone | dehors ; téléporté dans la zone : rentré (position propre) ; A entre → `Victory` |
| Client déconnecté puis reconnecté (id 2) pendant une manche | voit A « rentré » et lui-même dehors ; entre → `Victory` ; relance → `Pending`, positions (0;1;0) et (6;1;0) |

## Erreurs et limites

- L'évaluation de `NightOutcome` a été désactivée à la main uniquement pour le scénario porteur (puis réactivée) ; aucun fichier modifié.
- Un seul client virtuel, pas de test à trois ; le cas échéance (entrée exactement à l'échéance / après) n'a pas été rejoué en jeu, seulement par tests de règles.
- Un joueur porté déconnecté ou un porteur dont le partenaire n'existe plus : non traité (P1-040).
- Entrées réelles clavier/souris non utilisées.

## Git

Les quatre fichiers ci-dessus et ce rapport sont prêts à être committés. Aucun push, aucune capture. Prochaine action : reprendre la progression P1.
