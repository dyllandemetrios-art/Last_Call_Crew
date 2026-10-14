# Rapport P1-051 — Clés de la maison

**Statut : PASS** (règles automatiques + hôte/client en Multiplayer Play Mode, demandes et états injectés par script ; aucune entrée système envoyée à l'écran).

## Départ

Branche `feature/p1-001-network-foundation`, commit `2d4c92c`. `Docs/Backlog.md` et `Docs/Specs/P1-051-house-keys.md` non commités préservés. Aucun package ajouté.

## Fichiers et scène

- Nouveaux : `Scripts/Runtime/Night/KeyRules.cs` (règles pures), `Scripts/Runtime/Network/HouseKeys.cs`, `Tests/EditMode/KeyRulesTests.cs`, matériaux `Keys` et `HouseSideboard`.
- Modifiés : `PlayerInteractor.cs` (visée, `E`, RPC de prise), `NightRound.cs` (réapparition à la relance), `IntoxicationHud.cs` (invite, statut, feedback).
- Scène `BarGreybox.unity` : buffet `House_Sideboard` contre le mur ouest, au nord de la porte (0,7 × 0,9 × 1,6 m à (15,4 ; 19)), et objet réseau de scène `HouseKeys` (`NetworkObject` + `HouseKeys`, collider **trigger** 0,5 × 0,3 × 0,5, bloc jaune 0,28 × 0,08 × 0,18 à y = 1,05) ; le passage de la porte (3 m) reste libre. Nouveau `GlobalObjectIdHash` propre à l'objet ; les cinq autres hashes inchangés.

## Modèle d'autorité

- État unique répliqué `HouseKeys._taken` (`NetworkVariable<bool>`), écrit par l'hôte seul (`ServerTake`, `ServerResetForRound`). Pas de porteur, d'inventaire, de serrure, de perte ni de transfert.
- Le propriétaire vise (raycast existant) et voit « E — PRENDRE LES CLÉS » ; `E` envoie `TakeKeysRpc` (propriétaire → hôte). L'hôte reconstruit tout : émetteur, cible, gel, KO, porté, porteur, portée (`cameraRoot` → `ClosestPoint`, même `MaxServerRange` que les autres interactions), puis `KeyRules.Evaluate`. Une demande sur des clés déjà prises répond « déjà prises » sans mesurer de distance. Seul un résultat `Taken` écrit l'état. Le visuel et le collider suivent la valeur répliquée (donc aussi pour un client qui arrive tard).
- Retour au propriétaire seulement pour les refus : TROP LOIN, VOUS PORTEZ QUELQU'UN, NUIT TERMINÉE, CLÉS DÉJÀ PRISES.

## Présentation

Statut discret en bas à gauche (« CLÉS · À PRENDRE » / « CLÉS · PRISES »), sans nouveau grand panneau ; feedback « CLÉS RÉCUPÉRÉES » 2 s sur la transition répliquée (pas rejoué pour un client qui arrive tard, réinitialisé à chaque manche).

## Cycle de manche

La relance remet les clés disponibles dans `NightRound.ServerRestart` avant la remise à zéro du résultat, donc avant la reprise des contrôles. Une déconnexion ne change pas l'état (il appartient à la manche). Victoire, échec, retour et chronomètre inchangés.

## Tests automatiques

EditMode **224/224** (210 + 14) : prise acceptée, seconde demande sans transition, deux demandes successives (une seule acceptée), sept rejets (émetteur, cible, gel, KO, porté, porteur, portée) qui laissent l'état disponible, distance infinie ou NaN, remise à zéro de manche, conservation après déconnexion du demandeur, libellés. Compilation sans erreur.

## Validation hôte + client (Multiplayer Play Mode)

| # | Scénario | Résultat |
|---|---|---|
| A | l'hôte, à portée, visant les clés (`AimedKeys` vrai), prend | `keysTaken=True`, visuel masqué sur **les deux** instances, journal `client 0 took the house keys` |
| B | le client demande ensuite | `RejectedAlreadyTaken`, état inchangé |
| C | victoire puis relance | clés de nouveau visibles sur les deux instances |
| D | le client prend | même résultat (`client 1 took…`), identique sur les deux |
| E1 | hors portée (hôte au départ) | `RejectedOutOfRange`, clés présentes |
| E2 | joueur KO | `RejectedKnockedOut`, clés présentes |
| E3 | joueur porté (aussi KO) | refusé (`RejectedKnockedOut` : un porté est toujours KO ; `RejectedCarried` couvert par test pur) |
| E4 | joueur portant un partenaire | `RejectedCarrying`, clés présentes |
| E5 | mauvaise cible | `RejectedInvalidTarget` |
| E6 | manche terminée (échec par déconnexion P1-040) | `RejectedFrozen` |
| F | deux demandes presque simultanées (hôte et client en parallèle) | une seule prise validée (`client 2 took…`), l'autre `RejectedAlreadyTaken` |
| G | déconnexion du joueur qui a pris les clés | clés restent prises (résultat `Failure` P1-040) |
| H | client qui rejoint après la prise | clés absentes et statut « PRISES » dès l'arrivée ; relance ensuite : clés de nouveau à prendre |
| R | régressions | boire (`accept -> Accepted 2,40 m`), portage et retour (scénarios E4/C), relance, reconnexion ; aucune erreur dans la console hôte |

Mauvais émetteur : un RPC propriétaire → hôte ne peut pas être émis par un autre joueur (propriété exigée par NGO) ; le rejet est couvert par le test pur.

## Limites

- Demandes et visées injectées par script (comme demandé) ; la touche `E` réelle et le feeling de la visée sur le buffet restent à voir en jeu. Dialogue et réveil non rejoués (code inchangé, tests verts).
- Les clés ne verrouillent pas la porte et ne conditionnent pas la victoire.
- Un client reconnecté ne « possède » rien : l'état est collectif.

## Doublon NGO et réglages

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) supprimé après le dernier test (doublon NGO non référencé déjà constaté). `ProjectSettings/ProjectSettings.asset` avait de nouveau perdu son `preloadedAssets` (retouche automatique de l'éditeur) : restauré à l'état du dépôt. Aucun commit, aucun push.
