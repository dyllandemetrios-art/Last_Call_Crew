# Rapport P1-053 — Téléphone de la maison

**Statut : PASS** (règles automatiques + hôte/client en Multiplayer Play Mode, demandes et états injectés par script, sans entrée système).

## Départ

Branche `feature/p1-001-network-foundation`, commit `38bfd78` (`feat: add the shared house wallet`, qui clôture P1-052 avec le backlog et la fiche P1-053). Aucun package ajouté.

## Fichiers et scène

- Nouveaux : `Scripts/Runtime/Night/PhoneRules.cs` (`PhoneRules` + `DepartureChecklist`), `Scripts/Runtime/Network/HousePhone.cs`, `Tests/EditMode/PhoneRulesTests.cs`, matériau `Phone`.
- Modifiés : `PlayerInteractor.cs`, `NightRound.cs`, `IntoxicationHud.cs`, `BarGreybox.unity`.
- Scène : console `House_PhoneTable` contre le mur nord, à l'est des départs (23,5 ; 20,6), loin de la porte ; objet réseau de scène `HousePhone` (`NetworkObject` + `HousePhone`, collider trigger, bloc argenté 0,1 × 0,03 × 0,2). Troisième emplacement distinct des clés (buffet nord-ouest) et du portefeuille (table sud-ouest). Nouveau `GlobalObjectIdHash` ; 8 objets réseau de scène au total, hashes existants inchangés.

## Réutilisation et factorisation

- `PhoneRules` réutilise `KeyRequest`, `KeyOutcome` et la validation de `KeyRules` (comme le portefeuille).
- Duplication devenue évidente supprimée par un petit changement sûr : `PlayerInteractor.BuildPickupRequest` construit la partie commune de la demande (émetteur, gel, KO, porté, porteur, portée maximale) pour les **trois** RPC ; clés et portefeuille l'utilisent désormais, avec le même comportement (cible et distance restent propres à chaque objet). L'affichage de la checklist est une seule fonction à trois lignes.
- Volontairement non factorisés : les trois composants `HouseKeys` / `HouseWallet` / `HousePhone` (champs sérialisés distincts dans la scène) ; pas de base commune, d'inventaire ni de base d'objets.

## Autorité

`HousePhone._taken` (`NetworkVariable<bool>`) écrit par l'hôte seul. `E` sur le téléphone visé envoie `TakePhoneRpc` ; l'hôte reconstruit identité, cible, gel, KO, porté, porteur et portée, évalue via `PhoneRules` et n'écrit que sur `Taken`. Visuel et collider dérivés de l'état réseau. Invite « E — PRENDRE LE TÉLÉPHONE » ; feedback « TÉLÉPHONE RÉCUPÉRÉ ».

## Checklist complète

`DepartureChecklist` (pur) : `AllTaken`, `TakenCount`, `BecameReady` (faux → vrai seulement). HUD : checklist verticale compacte à gauche (3 lignes, **380 u de large, au milieu gauche** à `Screen.height × 0,42`, pas de nouveau grand panneau ; disposition finale validée à 960 × 540, voir plus bas) ; quand les trois sont pris, une 4e ligne « DÉPART · PRÊT » apparaît (dérivée des trois états répliqués, donc disparaît à la relance). L'annonce « PRÊTS À PARTIR » (2 s) n'est émise qu'au passage de « pas tout » à « tout » ; un client qui arrive tard (état observé −1) ne la déclenche pas. Ce statut est purement informatif : il ne conditionne ni la sortie ni le retour.

## Indépendance et relance

Trois variables, trois composants, trois `ServerResetForRound` appelés séparément par `NightRound.ServerRestart` (clés, portefeuille, téléphone) avant la reprise du contrôle. Observé : prendre le téléphone laisse les deux autres inchangés ; la relance remet les trois à disposition et « DÉPART · PRÊT » disparaît.

## Tests automatiques

EditMode **240/240** (230 + 10) : prise, seconde demande, rejet laissant disponible, remise à zéro et déconnexion, indépendance des trois objets jusqu'à la remise à zéro complète, comptage de la checklist, annonce unique « prêts », libellés. Compilation sans erreur.

## Validation hôte + client (injection)

| # | Scénario | Résultat |
|---|---|---|
| 1 | l'hôte vise (`AimedPhone` vrai, clés et portefeuille faux) et prend ; plus tard le client prend en nouvelle manche | `phone=TAKEN`, visuel masqué sur les deux instances ; journaux `client 0 / client 1 took the house phone` |
| 3 | indépendance | clés et portefeuille intacts après la prise du téléphone |
| 4 | annonce « PRÊTS À PARTIR » (texte du HUD hôte lu après chaque prise) | téléphone : « TÉLÉPHONE RÉCUPÉRÉ » ; clés (2/3) : « CLÉS RÉCUPÉRÉES », pas d'annonce ; portefeuille (3/3) : « PRÊTS À PARTIR » ; aucune répétition ensuite |
| 2 | concurrence (hôte et client en parallèle) | une seule prise (`client 1 took…`), l'autre `RejectedAlreadyTaken` |
| 2 | rejets | `RejectedOutOfRange`, `RejectedKnockedOut`, `RejectedCarrying`, `RejectedInvalidTarget` ; manche gelée : `RejectedFrozen` ; le téléphone reste présent à chaque fois |
| 5 | late join après préparation complète (trois pris, client reconnecté) | les trois absents et marqués pris dès l'arrivée |
| 6 | déconnexion du joueur ayant pris | téléphone toujours pris (échec P1-040) |
| 7 | relance | clés, portefeuille et téléphone visibles, `Pending` |
| 8 | boire, puis clés et portefeuille | `accept -> Accepted (2,40 m)` ; clés et portefeuille pris, téléphone intact |

Aucune erreur dans la console hôte. Mauvais émetteur : couvert par la règle pure (NGO exige la propriété). Le texte du HUD a été lu par réflexion sur l'instance hôte ; le rendu visuel n'a pas été regardé.

## Limites

Entrées injectées (visée forcée) ; la touche `E` réelle n'est pas rejouée. Aucun menu de téléphone, appel, taxi, monnaie ou zone ajouté.

## Vérification visuelle à 960 × 540 (ajout)

Méthode : vue Game de l'éditeur réglée en résolution fixe 960 × 540 (`Screen` = 960 × 540 confirmé), hôte seul en Play, états injectés par script, captures par `capture_game_view` (rendu interne de la vue Game : aucune entrée clavier/souris système envoyée, la souris et le clavier de l'utilisateur n'ont pas été utilisés).

- **Défaut trouvé et corrigé** : dans la version initiale (panneau de 260 u en bas à gauche), les libellés étaient **tronqués** (« EUILLE · À PRENDRE », « PHONE · À PRENDRE »). Correction minimale dans `IntoxicationHud.DrawKeysStatus` : panneau de 380 u, placé au milieu gauche (`Screen.height × 0,42`) pour ne chevaucher ni les invites du bas, ni les panneaux du haut, ni le réticule.
- Après correction, trois captures dans `Docs/Reports/img/` : `P1-053-checklist-1-available.png` (trois « À PRENDRE »), `P1-053-checklist-2-two-taken.png` (clés et portefeuille pris, téléphone à prendre, couleurs distinctes), `P1-053-checklist-3-ready.png` (trois pris et quatrième ligne « DÉPART · PRÊT »). Aucun texte tronqué, aucun chevauchement avec le panneau réseau, la carte NUIT, la carte IVRESSE, le réticule ni l'étiquette de build.
- Non vérifié visuellement : la bannière temporaire « PRÊTS À PARTIR » (2 s) et les invites « E — PRENDRE… » à ce format (texte de la bannière confirmé par lecture de l'état du HUD) ; vue à deux joueurs non capturée.
- Code modifié (disposition du HUD seulement) : tests relancés, **240/240**.

## Réglages et nettoyage

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) supprimé (doublon NGO non référencé). `ProjectSettings/ProjectSettings.asset` retouché automatiquement par l'éditeur puis rétabli à l'état du dépôt. La résolution fixe 960 × 540 ajoutée à la vue Game est une préférence d'éditeur, hors dépôt. Aucun commit de P1-053, aucun push.
