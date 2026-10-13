# Rapport P1-013 — Ivresse synchronisée (avec corrections P1-013a)

**Statut : PASS après correction P1-013a** — preuves techniques hôte/client et validation visuelle finale de Dyllan le 13 octobre 2026.

## Départ

- Branche `feature/p1-001-network-foundation`. Premier passage : commit de départ `1940e4a`. Correction P1-013a : commit `3590cb4` (fiche corrective), travail P1-013 non commité conservé (pas de reset).
- Unity 6000.3.8f1 ; aucun package ajouté.

## Retour de playtest de Dyllan (13 octobre 2026)

P1-013 non validé : (1) une décision rendait la proposition indisponible pour les deux joueurs (`_unavailableUntil` partagé) ; (2) le délai de 20 s cassait le rythme et laissait l'ivresse redescendre entre deux verres ; (3) l'interface de test était trop petite, dispersée et illisible. Le palier `Seuil KO` que j'avais signalé comme éphémère devait aussi devenir stable. Les preuves de cooldown du premier rapport sont supprimées ; elles ne correspondent plus au code.

## Correction apportée

- **Plus de cooldown ni d'indisponibilité** : `DrinkOffer` ne porte plus ni `_unavailableUntil`, ni `IsAvailable`, ni délai restant, ni visuel gris, ni textes. Il garde la dernière décision, son auteur et le compteur répliqués (utiles au HUD et au diagnostic). `DrinkRules` perd le paramètre de disponibilité et `DrinkRequestOutcome.RejectedUnavailable` est supprimé. `NpcOfferCooldownSeconds` reste dans `TuningConfig`, inutilisé par cet objet de test.
- **Validations conservées** : identité de l'émetteur, cible, portée, début de refus valide, durée de maintien mesurée par l'hôte.
- **Ivresse figée au seuil KO** : `IntoxicationRules.ApplyDecay` renvoie la valeur inchangée dès qu'elle atteint `KoThreshold` (et la borne si elle le dépasse). Elle ne redescend qu'après une modification explicite par un futur système (aucune API de modification n'a été ajoutée ; seul `ServerAddDrink`, qui ne peut que monter, existe). Le mouvement et les interactions ne sont toujours pas désactivés (P1-020).
- **Interface de debug reconstruite** : voir plus bas.

## Fichiers (cumul P1-013 + P1-013a, tous non commités)

Créés :
- `Scripts/Runtime/Intoxication/LastCallCrew.Intoxication.asmdef`, `IntoxicationRules.cs` : bornes, gain, décroissance (figée à 100), paliers, libellés.
- `Scripts/Runtime/Network/PlayerIntoxication.cs` : ivresse répliquée d'un joueur.
- `Scripts/Runtime/Network/IntoxicationHud.cs` : présentateur visuel commun (panneau Ivresse, invite, barre de refus, feedback).
- `Scripts/Runtime/Network/DebugGuiKit.cs` : échelle d'écran, styles `GUIStyle` mis en cache, fonds sombres, barres.
- `Tests/EditMode/IntoxicationRulesTests.cs`, `.meta` correspondants.

Modifiés :
- `Drink/DrinkRules.cs`, `Network/DrinkOffer.cs`, `Network/PlayerInteractor.cs` (gain via `ServerAddDrink()` dans le chemin d'acceptation validée, disponibilité retirée, exposition de l'état pour le HUD, `OnGUI` supprimé), `Network/NetworkTestHud.cs` (présentation seulement ; la logique Host/Client/Shutdown, `-lcc-role` et le journal `[P1-001]` sont inchangés ; libellés en français).
- `Prefabs/Player.prefab` (`PlayerIntoxication` avec référence au `TuningConfig`), `Scenes/NetworkTest.unity` (objet `IntoxicationHud`, `DrinkOffer` sans champs de disponibilité), `Tests/EditMode/DrinkRulesTests.cs`, asmdef des tests.

## Modèle d'autorité

- Chaque joueur a son `PlayerIntoxication` : `NetworkVariable<float>` (écriture serveur, lecture par tous) et la valeur autoritaire `_serverValue` sur l'hôte seulement.
- Gain : `ServerAddDrink()` n'est appelé que dans `AcceptRequestRpc`, après validation (émetteur, cible, portée) et `offer.ServerDecide(Accepted)`. Aucun RPC de gain. Refus, annulations et rejets n'ajoutent rien.
- Décroissance : calculée sur l'hôte avec `Time.deltaTime` et `PassiveDecayPerSecond`, bornée au minimum, gelée à `KoThreshold`.
- Paliers : toujours déduits de la valeur répliquée (hôte compris) avec les seuils de l'asset. Nouveau client : état reçu par la `NetworkVariable`, sans RPC de rattrapage.
- Fréquence de réplication : écriture immédiate au gain et au changement de palier ; pendant la décroissance au plus toutes les 0,5 s et si la valeur a varié d'au moins 0,1 (réglable) ; aucune écriture quand la valeur est au minimum ou à 100. Coût : au pire 2 écritures de float par seconde par joueur.

## Interface de debug (P1-013a)

IMGUI uniquement, échelle = hauteur d'écran / 1080 (minimum 0,6), tous les rectangles dérivent de cette échelle, `GUIStyle` créés une fois dans `DebugGuiKit.Ensure()` (reconstruits seulement si la hauteur d'écran change) et texture de fond unique ; aucune allocation de style par `OnGUI`.
- Haut gauche : panneau « RÉSEAU » compact (rôle, ID, nombre de joueurs, bouton Arrêter ; Hôte / Client avant connexion).
- Haut droite : panneau « IVRESSE » à fond sombre, une carte par joueur (triés par ClientId, donc dans le même ordre sur les deux écrans) : `VOUS`/`AUTRE`, « Joueur N », valeur « N / 100 » (100 lu depuis le seuil KO du `TuningConfig`), palier en majuscules et barre de progression colorée (vert Sobre, jaune Pompette, orange Ivre, rouge Seuil KO). La carte « VOUS » est surlignée.
- Bas centre, zone unique : invite « E — BOIRE » / « MAINTENIR CLIC DROIT — REFUSER » quand une proposition est visée ; remplacée pendant le refus par « REFUS EN COURS… » et une large barre ; ligne de feedback au-dessus (« VOUS AVEZ ACCEPTÉ/REFUSÉ LE VERRE » ou « JOUEUR N A … », « REFUS ANNULÉ », « TROP LOIN »…) pendant 2 s ; réticule « + » au centre. Plus aucun compte à rebours ni texte technique.
- Corrections faites après mes captures : le nom de joueur et la valeur « 100 / 100 » se chevauchaient à 1280×720 (colonnes recalibrées à 54 % / 46 %, panneau élargi à 480 unités) ; invite élargie à 1120 unités.

## Tests automatiques

- Compilation : 0 erreur.
- `unity command run_tests --mode EditMode --filter LastCallCrew.Tests` : **54 tests, 54 réussis** (config 24 ; règles de verre 10, sans disponibilité, dont « deux demandes valides de suite = même issue » ; ivresse 20 dont gel au seuil KO : valeur au seuil, au-dessus du seuil, juste en dessous qui décroît encore, 5 verres puis 10 s de décroissance restent à 100).
- Recherche textuelle : aucune occurrence de cooldown, indisponibilité ou disponibilité dans les scripts du projet, hors `TuningConfig` et ses tests (`NpcOfferCooldownSeconds`).

## Validation hôte + client (injectée, Multiplayer Play Mode, pilotée par Unity CLI)

Entrées de jeu (E, bouton droit) injectées comme événements d'état dans l'Input System sur l'instance concernée ; valeurs lues par `eval` sur chaque instance ; **ce ne sont pas des saisies humaines**. Hôte = ClientId 0, client = ClientId 1.

1. **Deux acceptations immédiates de l'hôte (≈ 1,2 s d'écart)** : p0 = 19,69 puis 39,23 (deux fois +20 moins une décroissance de 0,2/s), sur les deux instances. Journal : « drink 0,00 → 20,00 », « drink 19,54 → 39,54 ». Aucune demande rejetée.
2. **Acceptation du client juste après celle de l'hôte (≈ 1 s)** : p1 passe de 0 à ≈ 20 (journal « client 1 drink 0,00 → 20,00 ») ; la valeur de p0 n'est pas modifiée par ce gain ; décisions n°3 (hôte) et n°4 (client) acceptées, vues par les deux instances.
3. **Refus** : refus complet de l'hôte puis du client (Refused ×2) sans aucun gain ; refus relâchés à 0,8 s (hôte puis client) : `HoldTooShort`, compteur de décisions inchangé.
4. **Hors portée (13,5 m)** : accept, début et fin de refus forcés depuis le client puis l'hôte : `RejectedOutOfRange`, `RejectedOutOfRange`, `RejectedNoRefusalStarted`, aucune décision ni gain.
5. **Montée à 100 par acceptations réelles (E espacés d'environ 1 s)** : 53 → 72 Ivre → 92 Ivre → **100 Seuil KO** → 100 → 100 (journal « 91,50 → 100,00 (KoThreshold) » puis « 100,00 → 100,00 »).
6. **Observation à Seuil KO** : relevés à 0, 3 et 6 s sur les deux instances : p0 = 100,00 « Seuil KO » à chaque relevé, sur l'hôte et sur le client, pendant que p1 continue de décroître normalement (11,5 → 10,1) : le gel est propre à chaque joueur.
7. **Régression** : à 100, une acceptation supplémentaire est acceptée sans changer la valeur ; mouvement vérifié (touche A, x : 1,50 → −4,87 en 1 s) ; visée, acceptation et refus fonctionnent (étapes 1 à 5).
8. Les valeurs relevées sur l'hôte et le client sont identiques à ±0,1 près (retard de réplication de décroissance) et les paliers sont identiques ; un relevé à 100 est strictement identique.
9. Consoles hôte et client : **0 erreur**, 1 warning sans rapport chacune (tous les passages, dont le dernier).

## Captures du Game view (hôte et client, deux résolutions)

Résolution de l'éditeur fixée à 1920×1080 puis à 1280×720 (`Screen` relu : 1920×1080 puis 1280×720 sur les deux instances) ; captures par `capture_game_view` ; les fichiers sont des temporaires non conservés dans le dépôt ; remise du Game view en « Free Aspect » après les tests. Observations (vues sur les images) :
- **1920×1080, hôte et client** : trois zones sans chevauchement (réseau en haut à gauche, ivresse en haut à droite, invite et feedback en bas au centre). Cartes VOUS (surlignée) et AUTRE distinctes, valeurs, paliers et barres colorées lisibles. Invite unique « E — BOIRE / MAINTENIR CLIC DROIT — REFUSER ». Pendant le refus : « REFUS EN COURS… » et large barre rouge. Feedback « VOUS AVEZ ACCEPTÉ LE VERRE » (client) et « JOUEUR 1 A ACCEPTÉ LE VERRE » (hôte). État Seuil KO : « 100 / 100 », libellé et barre rouges.
- **1280×720, hôte et client** : mêmes zones, texte à l'échelle (≈ 0,67), plus de chevauchement entre « AUTRE · Joueur 0 » et « 100 / 100 » après la correction ; invite et barre de refus entièrement visibles (vérifiés sur les captures hôte et client de l'état KO et du maintien de refus du client).
- Limite de ma vérification : j'ai relu les images clés (feedback, invite, maintien, KO) ; je n'ai pas fait un contrôle pixel par pixel de chaque capture.

## Limites

- Dyllan a validé en jeu deux consommations successives, la montée durable à `Seuil KO`, la lisibilité des panneaux, l'invite et le feedback de deux secondes. L'interface reste volontairement une interface de debug et son habillage n'est pas représentatif de l'interface finale.
- Le refus complet et le relâchement anticipé disposent de preuves techniques injectées ; aucune observation humaine distincte n'a été consignée pour leur feeling après P1-013a.
- Dans mes tests, la vraie souris de la machine a parfois perturbé le jeu (elle remplace mon état de bouton injecté et peut incliner la caméra), ce qui m'a fait refaire deux scénarios ; les résultats retenus sont ceux des passes où la visée était vérifiée.
- Aucune API de « modification explicite » de l'ivresse à 100 n'existe encore (réveil = P1-020 ou plus tard) ; l'ivresse figée à 100 ne peut donc plus baisser dans cette scène.
- Pas de test Relay, de build autonome, de mauvaise connexion, ni de rejoin d'un client après la correction (le rejoin avait été validé avant P1-013a ; le code d'état répliqué n'a pas changé).
- NGO recrée `Assets/DefaultNetworkPrefabs.asset` à la compilation ; supprimé après chaque passe (non référencé).
- Le HUD relit les joueurs toutes les 0,5 s ; un joueur qui rejoint peut apparaître avec ce délai.

## Changements non commités

Tout ce qui est listé ci-dessus. Aucun commit, aucun push.

## Validation de Dyllan

Le 13 octobre 2026, Dyllan a confirmé :

- deux consommations immédiates successives fonctionnent ;
- la montée jusqu'au `Seuil KO` fonctionne et reste affichée durablement sur les deux écrans ;
- les panneaux, l'invite et le feedback sont lisibles à sa résolution ;
- la durée de feedback de deux secondes est acceptable pour ce prototype.

## Piste gameplay à arbitrer ultérieurement

Dyllan propose qu'un joueur ayant trop bu puisse perdre la possibilité de refuser un verre. Cette idée n'est pas implémentée dans P1-013 : elle modifie directement l'agence du joueur et devra être prototypée séparément avec un seuil réglable, un feedback explicite et une vérification qu'elle reste amusante plutôt que punitive. Elle pourra être évaluée avec les effets d'ivresse ou les profils de personnage après stabilisation de la boucle principale.
