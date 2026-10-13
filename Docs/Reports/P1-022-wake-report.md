# Rapport P1-022 — Réveil d'un partenaire KO

**Statut : PASS** (hôte + client, tests automatisés et validation manuelle de Dyllan le 13 octobre 2026).

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `dac4191`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Wake/LastCallCrew.Wake.asmdef` (référence `LastCallCrew.Config`) et `WakeRules.cs` : règles pures (validation, fin de maintien, échéance automatique, ivresse finale).
- `Scripts/Runtime/Network/PlayerWake.cs` : composant réseau de réveil.
- `Tests/EditMode/WakeRulesTests.cs` : 23 tests.
- `.meta` correspondants.

Modifiés :
- `Assets/InputSystem_Actions.inputactions` : action `Player/Wake` (bouton), liée à `<Keyboard>/f`.
- `Prefabs/Player.prefab` : composant `PlayerWake` (+16 lignes, référence au `TuningConfig` posée via l'éditeur).
- `PlayerInteractor.cs` : lecture de F, maintien côté propriétaire, `AimOrigin`, `IsWaking`, `WakeProgress`.
- `PlayerKnockout.cs` : armement de l'échéance à l'entrée en KO, nettoyage à la sortie.
- `PlayerIntoxication.cs` : `ServerSetValue` (hôte).
- `IntoxicationHud.cs` : invites, jauge, compte à rebours, feedback.
- `LastCallCrew.Tests.EditMode.asmdef` : référence `LastCallCrew.Wake`.
- Non modifiés : `TuningConfig`, `NetworkTest.unity`.

## Autorité

- Le propriétaire du réveilleur n'envoie que `RequestStartRpc(cible)` et `RequestEndRpc(cible)` (`PlayerWake`, `SendTo.Server`, propriété requise).
- L'hôte valide à chaque étape (`WakeRules.Validate`) : émetteur, cible résolue en joueur, cible ≠ soi, réveilleur debout / ni porté ni porteur, cible KO, cible non portée, portée (`MaxServerRange` = 3 m + 1 m, depuis la caméra du réveilleur).
- `PlayerWake.ServerWake(cause, réveilleur)` est le seul chemin de réveil : il fixe l'ivresse (`ServerSetValue`) puis sort du KO (`ServerExitKnockout`, qui libère aussi du porteur) puis incrémente le compteur répliqué.
- État répliqué (écrit par l'hôte, donc reçu par un nouveau client sans RPC de rattrapage) : `_autoWakeDeadline` (temps serveur), `_wakeCount`, `_lastWakerPlusOne`.

## Mesure du maintien

L'hôte note `Time.realtimeSinceStartupAsDouble` au début, la compare à la fin, et accepte `durée + 0,15 s ≥ WaterWakeHoldSeconds` (même marge de gigue que le refus). La durée vient de `tuning.WaterWakeHoldSeconds`, jamais du client. Le client sert seulement à la jauge et à signaler l'arrêt (relâché, visée perdue, durée atteinte). À la fin, les conditions sont revalidées : une cible déjà réveillée, portée ou hors portée annule.

## Arbitrage manuel / automatique

Tout passe par `ServerWake`, qui rend `false` si la cible n'est plus KO : le second arrivé ne fait rien (le réveil manuel perdant renvoie `RejectedTargetNotKnockedOut`). L'échéance automatique est `ServerTime` au KO + `AutoWakeSeconds`, armée dans `ServerEnterKnockout`.

## Valeurs d'ivresse

`DrunkennessAfterWaterWake` (50) et `DrunkennessAfterAutoWake` (90), lues dans le `TuningConfig` via `WakeRules.DrunkennessAfter` ; aucune valeur dupliquée en littéral (les valeurs 50/90 n'apparaissent que dans un test qui vérifie la configuration actuelle). La décroissance passive repart de la nouvelle valeur (observé : 90 → 85,7 après ~21 s, soit −0,2/s).

## Restauration des systèmes

Après réveil, relevé sur hôte et client : KO faux, collider actif, `NetworkTransform` actif, joueur non porté ; `cameraRoot` revenu à `(0 ; 0,6 ; 0)` rotation nulle ; déplacement (touche W, z 0 → 4,99) fonctionnel ; l'invite de visée redevient disponible.

## Résultats (Multiplayer Play Mode)

Entrées : touche F maintenue par événements d'état Input System réinjectés à chaque update. **Injectés** (appels de test, non joueur) : `ServerAddDrink()` pour le KO, téléportations des joueurs, appels directs des RPC pour les rejets, modification de l'échéance par réflexion pour la course, mode `AllDeviceInputAlwaysGoesToGameView` (en mémoire) pour que l'éditeur accepte le clavier simulé.

| Cas | Résultat observé (journal hôte) |
|---|---|
| Hôte réveille client | `wake hold started` puis `woken (Water) drunkenness=50`, `WokenByWater (held 3,00 s, distance 1,70 m)` |
| Client réveille hôte | idem, `held 2,99 s`, ivresse 50 |
| Relâchement à 1,5 s | `HoldTooShort (held 1,50 s)`, cible toujours KO |
| Perte de visée à 1,0 s | `HoldTooShort (held 1,00 s)`, cible toujours KO |
| Cible debout | `RejectedTargetNotKnockedOut` |
| Cible à 12 m | `RejectedOutOfRange` |
| Soi-même | `RejectedSelf` |
| Fin sans début | `RejectedNoHoldStarted` |
| Réveilleur porteur | `RejectedRequesterCarrying` |
| Réveilleur KO | `RejectedRequesterKnockedOut` |
| Auto complet | KO 12:48:19,437 → `woken (Auto) drunkenness=90` 12:49:04,419 : **44,98 s** (valeur réelle 45 s) |
| Course, échéance à +3,0 s | l'auto gagne de 5 ms, le manuel est rejeté ; un seul réveil (compteur +1) |
| Course, échéance à +3,1 s | le manuel gagne, l'auto ne se déclenche pas ; un seul réveil (compteur +1) |

Ivresse relevée sur les deux instances après réveil : 50 (manuel) et 90 (auto), puis décroissance identique des deux côtés. Pendant ces réveils, l'autre joueur est resté libre (déplacement testé après réveil, aucune contrainte posée sur lui).

Interface (captures 1280×720 relues) : invite en deux lignes « E — PORTER JOUEUR 1 » et « MAINTENIR F — RÉVEILLER AVEC DE L'EAU » ; « RÉVEIL EN COURS » avec grande barre (capturée à ~33 %) ; « VOUS AVEZ RÉVEILLÉ JOUEUR 1 » côté réveilleur ; « RÉVEIL ANNULÉ » après un relâchement ; côté KO « RÉVEIL AUTOMATIQUE DANS 9 s » sous « VOUS ÊTES KO ». Un premier essai avait un texte coupé à 1280 px : corrigé en empilant les deux invites.

Compilation : 0 erreur. **103 tests EditMode sur 103 réussis** (80 précédents + 23 `WakeRulesTests`), relancés après la dernière modif du HUD. Console : aucune erreur de jeu ; les 2 erreurs de la console de l'éditeur viennent de mes appels `capture_game_view` avec un chemin hors projet (outillage).

## Erreurs rencontrées

- Rejet client « NetworkConfig mismatch » : le joueur virtuel MPPM était resté bloqué (asset DB en lecture seule) après mes imports ; relancé manuellement par Dyllan, après quoi les hashes de prefab concordaient. Hors code de jeu.

## Limites

- Les réveils s'appuient sur une décision de portée mesurée à la fin du maintien ; sortir de portée *pendant* le maintien n'est détecté qu'à la fin (le client annule en pratique sur perte de visée, à 3 m).
- « Cible portée » : avec deux joueurs, le seul autre joueur est le porteur, donc l'hôte rejette d'abord `RejectedRequesterCarrying`. Le motif `RejectedTargetCarried` est couvert par les tests purs, pas en jeu (il faut 3 joueurs). Même remarque pour « sortie de portée » avec cible portée.
- Pas testé : rejoin tardif d'un 3ᵉ participant (état par `NetworkVariable`, sans RPC), mauvaise connexion, Relay, build autonome.
- Le compte à rebours du joueur KO utilise `ServerTime` : il peut différer d'une fraction de seconde de l'hôte selon la latence.
- Eau illimitée, aucun son, animation ni particule (hors périmètre). Le feeling du maintien de 3 s est à valider par Dyllan.
- Le test réel du réveil automatique n'a mesuré qu'une instance à 45 s (client KO) ; l'autre sens a été vu aussi (hôte, ivresse 89,7 après réveil auto).

## Validation de Dyllan

Le 13 octobre 2026, Dyllan a validé manuellement le maintien de réveil, sa lisibilité et le retour en jeu. Le playtest global portage plus réveil P1-023 est accepté.

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés. Le mode d'entrée `AllDeviceInputAlwaysGoesToGameView` n'a été posé qu'en mémoire (aucun changement de `ProjectSettings`).

## Changements non commités

Tous les fichiers listés. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan réveille un partenaire à la main dans les deux sens (feeling du maintien et lisibilité des invites), puis fiche suivante.
