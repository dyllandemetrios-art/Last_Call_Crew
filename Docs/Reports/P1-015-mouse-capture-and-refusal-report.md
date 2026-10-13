# Rapport P1-015 — Capture souris et refus

**Statut : PASS** — correctif implémenté, compilé et couvert par les tests EditMode existants, puis protocole réel validé par Dyllan dans Multiplayer Play Mode le 13 octobre 2026.

## Départ

Branche `feature/p1-001-network-foundation`, commit `8a1995f`, arbre propre. Unity 6000.3.8f1, aucun package ajouté.

## Cause (analyse de code, non reproduite)

- Aucun code ne gérait `Cursor.lockState`/`visible` : le pointeur restait libre pendant le regard (Look = `<Mouse>/delta`), d'où la fuite hors de la Game View.
- Le binding `Refuse` = `<Mouse>/rightButton` et la logique `ReadNewInput`/`UpdateHold` sont corrects. Hypothèse retenue : le pointeur sorti de la Game View (ou la Game View sans focus) ne délivre plus le clic droit à l'instance, donc le refus « ne répond pas ». **Le lien entre les deux symptômes reste une hypothèse** tant que Dyllan n'a pas rejoué le protocole.

## Fichiers modifiés

- `Assets/_Project/Scripts/Runtime/Network/PlayerController.cs`
- `Assets/_Project/Scripts/Runtime/Network/PlayerInteractor.cs` (+`ConsumedCancelThisFrame`, 5 lignes)

Commandes, durées, règles réseau, HUD et assets inchangés (aucune scène/prefab touché).

## Comportement

- Propriétaire uniquement : capture (`Locked`, curseur masqué) à l'apparition du joueur ; libération au despawn.
- `Échap` (action `DialogueCancel` existante) libère la souris, sauf si ce même `Échap` vient de fermer un dialogue (lecture en `LateUpdate`, après `PlayerInteractor`).
- Capture perdue : un clic gauche volontaire la reprend (pas de re-verrouillage par frame ; rien tant que `Application.isFocused` est faux). Indication « Clic gauche : reprendre la souris » en bas d'écran (IMGUI minimal dans `PlayerController`).
- Pointeur libéré : le regard est ignoré.
- Bilan de nuit (P1-033) : le pointeur est libéré au gel pour permettre le clic sur `RELANCER`, puis repris à la relance (transition seulement).
- Éditeur vs build : même code. Dans l'éditeur, Unity relâche lui-même le verrou sur `Échap` / perte de focus ; la reprise par clic gauche couvre ce cas. Différence éventuelle avec MPPM non observée.

## Vérifications réalisées

- Compilation par recompilation Editor : terminée, 0 erreur/avertissement de compilation dans la console.
- EditMode : **174/174 passés** (avant et après l'ajout du comportement de gel).
- `Assets/DefaultNetworkPrefabs.asset` recréé par NGO supprimé ; `git status` ne montre que les deux scripts + ce rapport.

## Validation manuelle de Dyllan

Dyllan a validé le protocole demandé : capture et regard sur les deux instances, reprise de capture, refus complet et relâché trop tôt, fermeture du dialogue avec `Échap`, contrôles existants, clic sur `RELANCER` et reprise du regard.

## Limites / risques

- Si le clic droit ne répond toujours pas avec le pointeur capturé, la cause serait ailleurs (focus MPPM / Input System « Play Mode Input Behavior ») : prévoir la vérification dans un build Windows.
- Le clic gauche de reprise est lu via `Mouse.current` (aucune action existante n'y est dédiée).

## Git

Prochaine action : P1-010, greybox jouable. Aucun push.
