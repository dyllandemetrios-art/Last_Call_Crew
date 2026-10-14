# Rapport P1-055 — Taxi collectif

**Statut : PASS validé par Dyllan le 14 octobre 2026.** Règles automatiques, hôte/client en Multiplayer Play Mode et aller-retour réel approuvé par Dyllan. Claude Code n'a utilisé ni la souris ni le clavier du système.

## Départ

Branche `feature/p1-001-network-foundation`, commit `0de93e2` (squelette de ville). Documents Codex conservés (`Docs/Backlog.md`, fiche P1-055). Aucun package ajouté. Rien commité ni poussé.

## Fichiers

- Nouveaux : `Night/TaxiRules.cs` (états, issues, validation pure, libellés), `Night/TaxiPath.cs` (trajet pur), `Network/CollectiveTaxi.cs`, `Network/TaxiTerminal.cs`, `Tests/EditMode/TaxiRulesTests.cs`, matériaux `CityQuay`, `CityTaxiBody`, `CityTaxiDark`, `CityTaxiPost`, capture `Docs/Reports/img/P1-055-top-view.png`.
- Modifiés : `PlayerInteractor.cs` (borne visée, `TaxiInteractRpc`, retour), `PlayerController.cs` (déplacement et regard bloqués en trajet), `NightRound.cs` (remise à zéro du taxi), `IntoxicationHud.cs` (invite et état `TAXI`), `BarGreybox.unity`.
- Prefab joueur **non modifié** : le taxi suspend lui-même `NetworkTransform` et collider des joueurs.

## Conception

- **Autorité** : `CollectiveTaxi` (`NetworkObject` de scène) écrit quatre petites `NetworkVariable` (état, instant de départ, durée, numéro de relance) **uniquement à chaque transition**. Aucune position répliquée.
- **Mouvement** : chaque instance échantillonne `TaxiPath` à partir de `ServerTime`, de l'instant et de la durée répliqués. Axe de la carte A `(0 ; 8)` → `(0 ; −4)` → `(−130 ; −4)` → B `(−130 ; 13)` (≈ 159 m) aux virages adoucis (rayon 8 m), accélération douce. Durée : paramètre Inspector `tripSeconds` = 12 s (appel à vide inclus).
- **États** : `AtA`, `AtB`, `EmptyToA`, `EmptyToB`, `PassengersToA`, `PassengersToB`.
- **Validation hôte** (`TaxiRules.Evaluate`) : mauvais émetteur, mauvaise borne, manche terminée, demandeur KO ou porté, hors portée, trajet en cours (`TAXI EN ROUTE` / `TAXI DÉJÀ EN APPROCHE`), téléphone absent, portefeuille absent, moins de deux joueurs, joueur hors zone, KO non porté, KO porté par un joueur hors zone. L'appel exige le téléphone ; **le départ exige le téléphone puis le portefeuille** (lecture de « exige aussi » de la fiche).
- **Porteur** : E sur une borne demande le taxi même en portant (ailleurs E reste « lâcher »), sinon un porteur ne pourrait jamais partir.
- **Passagers** : pendant un trajet avec passagers chaque instance place les joueurs debout sur deux sièges (rang par identifiant) ; `NetworkTransform` et collider suspendus, donc aucun flux réseau de position. Un partenaire porté n'est pas touché : `PlayerCarry` le replace sur son porteur, le portage est conservé. À l'arrivée, chaque instance place le joueur à son point de dépose du quai ; seul le propriétaire réactive son `NetworkTransform` et se téléporte.
- **Contrôles** : `PlayerController` (déplacement, regard) et `PlayerInteractor` (toutes interactions, maintiens abandonnés) sortent tôt si `CollectiveTaxi.IsRiding`. Le HUD masque les invites et affiche `TAXI · EN ROUTE → B · n s` ; invites `E — APPELER LE TAXI`, `E — PARTIR VERS LA VILLE`, `E — RENTRER À LA COLOC`, `TAXI EN APPROCHE`.
- **Relance** : `NightRound.ServerRestart` appelle `CollectiveTaxi.ServerResetForRound` avant de replacer les joueurs (état A, trajet annulé, numéro de relance incrémenté). Les instances en train de rouler distinguent arrivée et annulation par ce numéro : annulation = poses de départ de la coloc.
- **Déconnexion pendant un trajet avec passagers** : le taxi dépose tout de suite le passager restant à destination ; l'échec P1-040 reste décidé par `NightOutcome`. Expiration pendant un trajet : le trajet **n'est pas figé** (il se termine normalement pendant le bilan) ; seule la relance le remet à zéro.
- **Client tardif** : reçoit l'état par les `NetworkVariable`, sans RPC de rattrapage.

## Scène (via l'éditeur Unity)

- Objet `CollectiveTaxi` (racine, `NetworkObject` + `CollectiveTaxi`), enfant `Vehicle` en primitives jaunes (corps, toit, 4 montants, enseigne, 4 roues, deux sièges), **sans collider** ; couleur dédiée `CityTaxiBody`.
- `City/TaxiStops/TaxiTerminal_A|B` : borne jaune à collider, zone d'embarquement (trigger 6 × 3 × 9 m), deux points de dépose.
- **Quai A** `x 3…9, z 3…12`, relié à la promenade par `z 12…13` (6 m de large), borne `(8,3 ; 9)`. **Quai B** `x −127…−121, z 9…18` relié à la rue nocturne par un passage de 6 m (`z 18…26`), borne `(−121,7 ; 13,5)`.
- **Chaussée non ouverte** : bordures de 1,2 m (`Kerb_A`, `Kerb_B`) le long du quai et fermetures sud (`Seal_A_South`, `Seal_B_South`). Les 29 volumes d'îlots ont été régénérés avec les deux quais ouverts.
- Écarts avec la carte : arrêt A conservé près de `(0 ; 9)` ; le taxi stationne en `(0 ; 8)` et en `(−130 ; 13)` (B de la carte était `(−130 ; 16)`), cap fixé vers la sortie pour que chaque trajet ne comporte qu'un demi-tour à l'arrivée ; les bornes ont été légèrement déplacées par rapport à la fiche pour rester à moins de 4 m des points de dépose.

## Tests automatiques

EditMode **267/267** (240 + 27) : appel, appel sans téléphone, appel sans portefeuille ni partenaire, seconde demande, demande pendant un trajet, départ complet, sans téléphone, sans portefeuille, partenaire absent, joueur seul, KO au sol, KO porté dans et hors zone, émetteur et cible invalides, gel, demandeur KO ou porté, portée (NaN compris), arrivées, aller-retour, appel à vide puis départ collectif, relance depuis tout état, blocage des contrôles, libellés, trajet (extrémités, longueur, monotonie, caps). Compilation sans erreur ; console sans exception.

## Validation hôte + client (éditeur principal = hôte, clone MPPM = client)

Positions et demandes injectées (téléportation propriétaire, appel de la méthode RPC par réflexion, états via `ServerTake`, `ServerEnterKnockout`, échéance forcée). Chaque ligne a été relevée sur les deux instances.

| # | Scénario | Résultat |
| --- | --- | --- |
| 4 | départ sans téléphone, puis sans portefeuille | `RejectedNoPhone`, `RejectedNoWallet`, taxi inchangé |
| 4 | partenaire hors de la zone ; demande depuis trop loin | `RejectedPartnerMissing`, `RejectedOutOfRange` |
| 4 | partenaire KO au sol ; demandeur KO | `RejectedPartnerKnockedOutNotCarried`, `RejectedKnockedOut` |
| 5 | hôte porte le client KO, départ demandé par le porteur | `Departed`, arrivée à B, les deux restent `carrying`/`carried` sur les deux instances, `NetworkTransform` du porté suspendu par le portage ; lâcher puis réveil sans blocage |
| 2 | retour B → A (téléphone et portefeuille déjà pris) | `PassengersToA`, arrivée `AtA`, déposes `(5,5 ; 7,5)` et `(5,5 ; 10,5)` |
| 6 | départs concurrents hôte + client | une seule transition (`Departed`), l'autre requête rejetée |
| 3 | appel depuis B, taxi à A ; seconde demande ; demande à l'autre borne | `Called` → `EmptyToB`, puis `RejectedAlreadyComing` et rejet hors portée ; arrivée à vide, **aucun joueur déplacé** (`NetworkTransform` actif) |
| 7 | pendant un trajet | `IsRiding` vrai sur chaque instance, `NetworkTransform` et collider désactivés (aucun flux de position), joueurs sur les sièges, véhicule en mouvement sur les deux instances |
| 1 | départ de A (aller) | arrivée après les 12 s configurées (`PassengersToB` à t 123,20, état `AtB` à ≈ +12 s) ; obtenu en séquence avec le test 5 |
| 9 | échéance forcée pendant un trajet, puis relance **pendant** ce trajet | `Failure`, trajet poursuivi pendant le bilan ; relance : taxi à A, joueurs en `(21 ; 15,2)` et `(21 ; 16,8)`, `NetworkTransform` et colliders réactivés, téléphone et portefeuille réinitialisés, `Pending` |
| 4 | appel sans téléphone après relance | `RejectedNoPhone`, aucun trajet |
| 8 | client déconnecté en plein trajet | `Failure PartnerDisconnected`, taxi `AtB`, hôte déposé en `(−124,2 ; 12)`, `NetworkTransform`/collider actifs, aucun contrôle bloqué |
| 8 | reconnexion du client | état `AtB` et véhicule en `(−130 ; 13)` reçus sans RPC de rattrapage |
| 9 | relance après déconnexion/reconnexion | taxi à A, joueurs à la coloc |

Console hôte et client : aucune erreur ni exception pendant ces essais.

Accès piéton vérifié par recherche de chemin (capsule r 0,4 m) depuis la coloc : quai A à 19 m, quai B à ≈ 171 m ; chaussée, route et branches de la route **inaccessibles**.

## Validation de Dyllan et limites

- Dyllan a joué et approuvé l'aller-retour réel : bornes, quais, caméra, demi-tour et durée de 12 s sont acceptés pour P1.
- Contrôles bloqués vérifiés par l'état (`IsRiding`, composants suspendus, positions) et lecture du code, pas par de vraies touches.
- Le texte du HUD (invites, état `TAXI`) n'a pas été vu à l'écran ; seule la logique est couverte. Une seule capture : `P1-055-top-view.png` (vue du dessus, taxi à A).
- Plus de deux joueurs : les déposes se répètent modulo deux points ; hors périmètre P1.
- Aucune physique, collision, trafic, argent, animation de porte ni offre ajoutés.

## Nettoyage et état Git

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) étaient de nouveau présents : même GUID `6caa043d…`, non suivis, non référencés (doublon NGO) ; supprimés avant ce compte rendu. `ProjectSettings` inchangé. Changements non commités : scripts et tests ci-dessus, `BarGreybox.unity`, 4 matériaux (+ `.meta`), capture, ce rapport, plus les documents Codex.

## Prochaine action recommandée

Remplacer les volumes visuels principaux par les assets déjà téléchargés, en conservant les racines gameplay, colliders simples et règles réseau validées.
