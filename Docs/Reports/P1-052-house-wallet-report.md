# Rapport P1-052 — Portefeuille de la maison

**Statut : PASS** (règles automatiques + hôte/client en Multiplayer Play Mode, demandes et états injectés par script, sans entrée système). Non commité.

## Départ

Branche `feature/p1-001-network-foundation`, commit `5e575d4` (`feat: add shared house keys`, qui clôture P1-051 avec le backlog et la fiche P1-052). Aucun package ajouté.

## Fichiers et scène

- Nouveaux : `Scripts/Runtime/Night/WalletRules.cs`, `Scripts/Runtime/Network/HouseWallet.cs`, `Tests/EditMode/WalletRulesTests.cs`, matériau `Wallet`.
- Modifiés : `PlayerInteractor.cs`, `NightRound.cs`, `IntoxicationHud.cs`, `BarGreybox.unity`.
- Scène : petite table `House_SideTable` contre le mur ouest, au **sud** de la porte (15,4 ; 12,6), et objet réseau de scène `HouseWallet` (`NetworkObject` + `HouseWallet`, collider trigger, bloc bleu 0,2 × 0,05 × 0,14). Emplacement distinct des clés (buffet au nord de la porte) ; le passage de 3 m reste libre. Nouveau `GlobalObjectIdHash` (7 objets réseau de scène, hashes existants inchangés).

## Réutilisation de code

- `WalletRules` réutilise tels quels `KeyRequest`, `KeyOutcome` et la validation de `KeyRules` (une ligne par méthode) : aucune logique dupliquée, aucun changement de comportement des clés. Seul le libellé de statut est propre au portefeuille.
- `HouseWallet` est un composant parallèle volontaire à `HouseKeys` (~50 lignes), sans classe de base ni inventaire ; la RPC et la construction de la demande dans `PlayerInteractor` sont parallèles (~25 lignes) pour ne pas toucher au code validé de P1-051. Le retour au propriétaire (`KeysResultRpc`) est partagé.
- Seul changement visible côté clés : le message de seconde demande devient « DÉJÀ PRIS » (commun aux deux objets) au lieu de « CLÉS DÉJÀ PRISES ».

## Autorité

`HouseWallet._taken` (`NetworkVariable<bool>`) écrit par l'hôte seul. `E` sur le portefeuille visé envoie `TakeWalletRpc` ; l'hôte reconstruit émetteur, cible, gel, KO, porté, porteur et portée (`MaxServerRange`, `ClosestPoint`), évalue via `WalletRules`, et n'écrit l'état que sur `Taken`. Visuel et collider dérivés de l'état réseau (donc aussi pour un client tardif). Invite « E — PRENDRE LE PORTEFEUILLE » ; feedback « PORTEFEUILLE RÉCUPÉRÉ » 2 s sur la transition répliquée.

## HUD

La ligne compacte de P1-051 devient une seule barre en bas à gauche (560 × 36) : « CLÉS · … » à gauche et « PORTEFEUILLE · … » à droite, chaque état lu depuis son propre état répliqué. Aucun nouveau grand panneau.

## Indépendance clés / portefeuille et relance

Deux variables, deux composants, deux `ServerResetForRound` appelés séparément par `NightRound.ServerRestart` (clés puis portefeuille) avant la reprise des contrôles. Observé : le portefeuille pris laisse les clés à prendre ; le client prend les clés, les deux sont pris ; la relance remet les deux visibles ; pris l'un, la relance (qui remet les deux) rend l'autre inchangé tant qu'il n'a pas été pris. Aucun des deux ne conditionne le retour ou la victoire.

## Tests automatiques

EditMode **230/230** (224 + 6) : prise acceptée, seconde demande sans transition, rejet qui laisse disponible, remise à zéro et déconnexion, indépendance avec les clés, libellés. Compilation sans erreur.

## Validation hôte + client (injection)

| # | Scénario | Résultat |
|---|---|---|
| 1a | l'hôte vise (`AimedWallet` vrai, `AimedKeys` faux) et prend | `wallet=TAKEN`, visuel masqué sur les deux instances ; clés intactes |
| 7 | le client prend les clés ensuite | clés et portefeuille pris, états indépendants, journaux `[P1-052]` puis `[P1-051]` |
| 6 | relance | clés **et** portefeuille visibles sur les deux instances |
| 1b | le client prend le portefeuille (nouvelle manche) | même résultat, clés disponibles |
| 3 | hors portée / joueur KO / joueur portant / mauvaise cible / manche gelée | `RejectedOutOfRange`, `RejectedKnockedOut`, `RejectedCarrying`, `RejectedInvalidTarget`, `RejectedFrozen` ; portefeuille toujours présent à chaque fois |
| 2 | deux demandes presque simultanées (hôte et client en parallèle) | une seule prise (`client 2 took…`), l'autre `RejectedAlreadyTaken` |
| 5 | déconnexion du joueur qui l'a pris | reste pris (échec P1-040) |
| 4 | client qui rejoint après la prise | portefeuille absent et statut « PRIS » dès l'arrivée |
| 8 | boire puis prendre les clés | `accept -> Accepted (2,40 m)` ; clés prises, portefeuille intact |

Aucune erreur dans la console hôte. Mauvais émetteur : NGO exige la propriété pour ces RPC ; le rejet reste couvert par la règle pure. « Porté » seul : un joueur porté est toujours KO (rejet `KnockedOut`) ; cas pur couvert par les tests.

## Limites

Entrées injectées (visée forcée par position et inclinaison) ; la touche `E` réelle et la lisibilité de la barre à 960 × 540 ne sont pas rejouées. Pas d'argent, de contenu, de perte, d'échange ni de taxi.

## Réglages et nettoyage

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) supprimé (doublon NGO non référencé déjà constaté). `ProjectSettings/ProjectSettings.asset` et `HouseSideboard.mat` retouchés automatiquement par l'éditeur puis rétablis à l'état du dépôt. Aucun commit de P1-052, aucun push.
