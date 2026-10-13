# Rapport P1-012 — Boire et refuser

**Statut : PASS** (preuve hôte + client en Multiplayer Play Mode ; feeling du maintien et lisibilité du feedback validés par Dyllan le 13 octobre 2026).

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `34f1100` (parent `4757202`), arbre propre.
- Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Assets/_Project/Scripts/Runtime/Drink/LastCallCrew.Drink.asmdef`, `DrinkRules.cs` : règles de décision pures (sans NGO), énumérations `DrinkDecision` et `DrinkRequestOutcome`. Assembly séparé pour que les tests EditMode puissent le référencer.
- `Assets/_Project/Scripts/Runtime/Network/DrinkOffer.cs` : proposition de verre (NetworkBehaviour).
- `Assets/_Project/Tests/EditMode/DrinkRulesTests.cs` : 10 tests.
- `.meta` correspondants.

Modifiés :
- `Assets/_Project/Scripts/Runtime/Network/PlayerInteractor.cs` : réécrit à partir de P1-002 (mêmes raycast, portée + tolérance, RPC propriétaire → hôte) pour gérer accepter et refuser.
- `Assets/_Project/Scenes/NetworkTest.unity` : l'objet « InteractiveCube » devient « DrinkOffer » (même position et même `NetworkObject`, composant `InteractiveCube` remplacé par `DrinkOffer`, référence explicite à l'asset `TuningConfig`).
- `Assets/InputSystem_Actions.inputactions` : action `Player/Refuse` liée à `<Mouse>/rightButton` (groupe Keyboard&Mouse). `Player/Interact` (E) inchangée.
- `Assets/_Project/Tests/EditMode/LastCallCrew.Tests.EditMode.asmdef` : référence à `LastCallCrew.Drink`.

Supprimés : `InteractiveCube.cs` et son `.meta` (remplacé par `DrinkOffer`, plus référencé nulle part). Choix assumé : « remplacer » plutôt que « spécialiser » ; l'acceptation par E d'une proposition disponible reproduit le comportement validé en P1-002 (l'état ON/OFF du cube est remplacé par disponibilité/décision).

`Player.prefab` n'est pas modifié (les nouveaux champs de `PlayerInteractor` ont des valeurs par défaut).

## Protocole d'autorité

Le propriétaire lit `Player/Interact` et `Player/Refuse`, vise par raycast (3 m), affiche l'invite, la barre de progression locale et les messages. Il n'envoie que des demandes (3 RPC `SendTo.Server`) :
1. `AcceptRequestRpc(cible)` sur appui E.
2. `RefuseStartRpc(cible)` à l'appui du bouton droit.
3. `RefuseEndRpc(cible)` quand le bouton est relâché, quand la visée est perdue ou quand la durée locale configurée est atteinte. Le client n'envoie aucune durée ni aucun « j'ai tenu assez longtemps ».

L'hôte, pour chaque demande, vérifie :
- identité : `SenderClientId == OwnerClientId` (rejet `RejectedInvalidSender`) ;
- cible : référence résolue vers un `DrinkOffer` (`RejectedInvalidTarget`) ;
- portée : distance caméra → point le plus proche du collider ≤ portée + tolérance (`RejectedOutOfRange`) ;
- disponibilité : instant serveur ≥ `_unavailableUntil` (`RejectedUnavailable`) ;
- pour un refus : début valide reçu pour cette cible (`RejectedNoRefusalStarted`) et durée de maintien **mesurée par l'hôte** entre début et fin (`Time.realtimeSinceStartupAsDouble`) ≥ durée configurée − `serverHoldTolerance` (0,15 s, Inspector) ; sinon `HoldTooShort` = annulation, aucune décision.

Une fin de refus consomme le début (un maintien ne produit qu'une décision). Un refus ou un accept ne peut pas coexister : toute décision démarre le délai d'indisponibilité et les demandes suivantes sont rejetées.

État répliqué (écriture hôte uniquement) dans `DrinkOffer` : dernière décision, identifiant du décideur, compteur de décisions, instant (temps serveur NGO) de retour de disponibilité. Visuel (or = disponible, gris = indisponible), texte « Dernière décision n°N : joueur X a ACCEPTÉ/REFUSÉ » et « Verre disponible / indisponible (N s) » s'affichent sur chaque instance à partir de cet état. L'hôte renvoie aussi au demandeur un `ResultRpc` pour les rejets et annulations (messages locaux).

## Usage de `TuningConfig`

`DrinkOffer` référence l'asset par un champ sérialisé (pas de singleton). Il appelle `ThrowIfInvalid()` au spawn. Durée de refus : `RefuseDurationSeconds` (2 s) ; délai d'indisponibilité : `NpcOfferCooldownSeconds` (20 s). Le client lit la durée via `DrinkOffer.RefuseDurationSeconds` pour sa barre de progression. Aucune de ces deux valeurs n'apparaît en littéral dans le code de gameplay. Seules constantes ajoutées, réglables dans l'Inspector de `PlayerInteractor` : portée 3 m, tolérance serveur de portée 1 m, tolérance de maintien 0,15 s (déjà du même ordre que P1-002 pour les deux premières). Le multiplicateur Sociable n'est pas utilisé.

## Tests automatiques

- Compilation : 0 erreur.
- `unity command run_tests --mode EditMode --filter LastCallCrew.Tests` : 34 tests, 34 réussis (24 de P1-011 + 10 `DrinkRulesTests` : acceptation valide, à portée limite, hors portée, indisponible, distance NaN/infinie ; refus tenu, tenu dans la tolérance, relâché trop tôt, hors portée ou indisponible malgré maintien suffisant, durée NaN).

## Validation hôte + client (Multiplayer Play Mode, pilotée par Unity CLI)

Hôte = éditeur principal (ClientId 0), client = clone MPPM (ClientId 1). Joueurs placés à 2 m de la proposition, face à elle. Entrées injectées par événements d'état dans l'Input System (clavier E, bouton droit de la souris maintenu 2,6 s ou 0,8 s) sur l'instance concernée ; état lu sur les deux instances après chaque étape. Cooldown réel de 20 s attendu entre les décisions.

| # | Cas | Résultat (hôte / client identiques) |
| --- | --- | --- |
| 1 | Acceptation hôte (E) | décision n°1 Accepted par 0 ; indisponible, ~18 s restantes des deux côtés |
| 2 | Acceptation client (E) | n°2 Accepted par 1 |
| 3 | Refus complet hôte (2,6 s) | n°3 Refused par 0 |
| 4 | Refus complet client (2,6 s) | n°4 Refused par 1 |
| 5 | Refus relâché à 0,8 s (hôte puis client) | `HoldTooShort`, compteur resté à 4, aucune décision, proposition toujours disponible |
| 6 | Hors portée (z = −10, 13,5 m) | E et bouton droit naturels : aucune demande, aucun changement. RPC forcés (accept, refuse-start, refuse-end) depuis client puis hôte : `RejectedOutOfRange` ×2, `RejectedNoRefusalStarted`, compteur inchangé |
| 7 | Pendant l'indisponibilité (juste après l'acceptation n°5) | client E → `RejectedUnavailable` ; client bouton droit 2,6 s → start `RejectedUnavailable` puis end `RejectedNoRefusalStarted` ; hôte E → `RejectedUnavailable` ; compteur resté à 5 |
| 8 | Émetteur invalide : l'hôte appelle `AcceptRequestRpc` du joueur du client | NGO exécute l'appel ; l'hôte le rejette : `RejectedInvalidSender` ; aucune décision |

- Après chaque décision, la disponibilité est revenue à `True` des deux côtés après 21 s (cas 5 : `available=True`, `remaining=0`), ce qui confirme le délai provenant de l'asset.
- Journaux : 5 décisions numérotées 1 à 5 sur l'hôte et sur le client, identiques (même décideur et même type) ; le compteur ne dépasse jamais le nombre de demandes valides.
- Consoles : 0 erreur, 1 warning sans rapport (hôte et client). Aucune erreur Netcode.

## Limites

- Les tests techniques ont utilisé des entrées injectées. Dyllan a ensuite validé manuellement le feeling du maintien et la lisibilité du feedback le 13 octobre 2026. Le message de rejet (`ResultRpc`) n'a pas été vérifié visuellement de manière isolée.
- La durée de maintien est mesurée à l'arrivée des RPC : une forte gigue ou des paquets groupés peuvent la fausser dans la marge de tolérance ; la perte du RPC de fin laisse un début « actif » consommé par la fin suivante ; cas non testés sous mauvaises conditions réseau (pas de test Relay).
- Un seul refus ou maintien par joueur à la fois ; un nouveau début écrase le précédent.
- Distance basée sur la position envoyée par le client (comme P1-002).
- Le cooldown de 20 s est ici global à la proposition (un seul objet) ; « par PNJ » viendra avec les PNJ.
- Aucune valeur d'ivresse n'est appliquée (P1-013).
- Le clone MPPM doit recharger la scène (`open_scene`) après modification côté éditeur principal.
- NGO a recréé `Assets/DefaultNetworkPrefabs.asset` ; supprimé (non référencé).

## Changements non commités

Tout ce qui est listé ci-dessus. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan teste à la main (E, bouton droit maintenu, relâchement anticipé, délai) et juge le feeling. Ensuite P1-013 (ivresse synchronisée) quand sa fiche sera prête.
