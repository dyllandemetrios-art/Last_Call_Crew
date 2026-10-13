# Rapport P1-030 / P1-030a — Chronomètre autoritaire de nuit

**Statut : PASS** (hôte + client, Multiplayer Play Mode). Lisibilité acceptée par Dyllan le 13 octobre 2026.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `ab5c657`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Night/LastCallCrew.Night.asmdef` (référence `LastCallCrew.Config`) et `NightTimerRules.cs` : règles pures (départ, échéance, temps restant, expiration, format `MM:SS`, dix dernières secondes).
- `Scripts/Runtime/Network/NightTimer.cs` : composant réseau.
- `Tests/EditMode/NightTimerRulesTests.cs` : 19 cas (9 tests dont un paramétré).
- `.meta` correspondants.

Modifiés :
- `TuningConfig.cs` : `NightDurationSeconds` (champ `_nightDurationSeconds`), validation strictement positive. `TuningConfig.asset` **non modifié** : le champ absent de l'asset prend sa valeur par défaut 300 (le test d'asset le vérifie).
- `Scenes/NetworkTest.unity` : GameObject `NightTimer` (`NetworkObject` placé en scène + `NightTimer`, `TuningConfig` assigné), ajouté par l'éditeur (+75 lignes, aucune suppression).
- `IntoxicationHud.cs` : chronomètre centré en haut ; le bandeau « VOUS PORTEZ » descend sous lui.
- `TuningConfigTests.cs`, `LastCallCrew.Tests.EditMode.asmdef`.

## Valeur provisoire

`NightDurationSeconds = 300` s, réglable dans le `TuningConfig`. Aucune autre valeur littérale hors affichage (seuil des 10 dernières secondes : constante d'affichage `FinalCountdownSeconds`).

## Modèle réseau

Deux `NetworkVariable` écrites par l'hôte uniquement : `NightState` (Waiting / Running / Expired) et l'échéance en temps serveur (`double`). Chaque instance calcule `RemainingSeconds` à partir de `NetworkManager.ServerTime` ; aucune valeur n'est répliquée par image ni par seconde, aucun RPC. Un nouvel arrivant reçoit l'état et l'échéance par les `NetworkVariable`.

## Démarrage

Dans `Update` de l'hôte : `NightTimerRules.ShouldStart(Waiting, ConnectedClientsIds.Count)` (≥ 2). Écriture de l'échéance puis de l'état `Running`, une seule fois (la garde exige `Waiting`). Observé : hôte seul → `Waiting`, 300,00 s affichés indéfiniment ; arrivée du client → `Running`, un seul journal `night started`.

## Expiration

`ShouldExpire` : `Running` et temps serveur ≥ échéance → `Expired`, état terminal. Le temps restant est borné à 0 ; côté affichage, un chronomètre `Running` dont l'échéance est passée montre déjà « TEMPS ÉCOULÉ » (pas de flash à 00:00 en attendant la réplication). Observé sur les deux instances : `Expired remaining=0,00` stable plus de 25 s après l'échéance, jamais négatif. Aucun contrôle bloqué.

## Déconnexion et rejoin

- Client déconnecté (`Shutdown`) : le chronomètre continue côté hôte (213,30 → 197,64 s sur 15,7 s de temps serveur) ; l'hôte ne réécrit rien.
- Reconnexion pendant la partie : le client reçoit l'état `Running` sans RPC de rattrapage ; échéance reconstituée identique (`servertime + remaining` = 316,33 sur l'hôte et sur le client) ; aucun second départ (un seul `night started` pour la session).
- Limite : le rejoin a été fait avec le même éditeur virtuel (nouveau ClientId), pas avec un 3ᵉ participant ni un build autonome.

## Comparaison hôte / client

Quatre relevés espacés de 15 s (≈ 60 s) : `servertime + remaining` identique à 0,01 s sur les deux instances (316,32 contre 316,33), donc écart d'affichage bien inférieur à 1 s. Les valeurs brutes diffèrent seulement parce que les deux lectures ne sont pas simultanées (~2,5 s d'écart d'exécution des commandes).

## Test accéléré

Sans toucher à l'asset : l'échéance a été réécrite côté hôte par réflexion (`_deadline = ServerTime + 14` puis `+ 9`) ; le temps a atteint zéro et l'état `Expired` est apparu sur les deux instances. Aucun changement de `TuningConfig.asset` (vérifié par `git status`).

## Après expiration

Contrôles et systèmes vérifiés après l'état `Expired` : boire (5 verres injectés) → KO du client ; portage (demande acceptée) ; lâcher ; réveil à l'eau par F maintenu (`WokenByWater (held 3,00 s)`, ivresse 50). Aucune régression observée.

## Coût réseau

Par conception : deux écritures de `NetworkVariable` au total par partie (départ : état + échéance ; expiration : état), plus la synchronisation initiale d'un client qui rejoint. Pas de trafic continu lié aux secondes affichées. **Non mesuré** avec un profiler réseau : preuve par le code et par les journaux (un `started`, un `expired`).

## Interface (première version, remplacée par P1-030a)

Captures 1280×720 relues : « EN ATTENTE DU PARTENAIRE » (hôte seul), `04:54` (client), `00:03` en orange (dix dernières secondes, simple virage de couleur, sans clignotement), « TEMPS ÉCOULÉ » en rouge (client). Un premier essai avait le chiffre collé au bord droit du cadre : corrigé (texte centré). Pas de capture à 1920×1080.

## Tests

- Compilation : 0 erreur.
- **122 tests EditMode sur 122 réussis** (103 précédents + 19 nouveaux), relancés après la dernière modification.
- Consoles hôte et client : aucune erreur de jeu après correction. Un premier essai avait produit des erreurs NGO (« NetworkPrefab hash was not found », objet de scène non synchronisé) : le joueur virtuel avait gardé l'ancienne scène en mémoire ; après `open_scene` sur son projet, plus d'erreur.

## Erreurs rencontrées

- Voir ci-dessus (scène périmée côté joueur virtuel). Les erreurs `capture_game_view` d'une session précédente restent dans l'historique de la console de l'éditeur (chemin hors projet), sans lien avec le jeu.

## Limites

- Aucune condition de victoire/échec, de relance ni de pause (hors périmètre).
- Pas de test Relay, de build autonome, de 3ᵉ participant ni de mauvaise connexion.
- Si l'hôte quitte, la partie s'arrête avec lui (pas de migration d'hôte).
- Le chronomètre n'utilise pas l'heure réelle : il suit `ServerTime`, donc `Time.timeScale` hôte peut le faire dériver pendant les tests locaux.

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés. Aucun changement de `ProjectSettings`.

## P1-030a — Lisibilité du chronomètre

**Retour de Dyllan (playtest à trois joueurs)** : tout fonctionne, y compris le portage (aucune action possible quand on est porté) ; le chronomètre est fonctionnel mais peu visible, il faut une UI comme les autres cartes, et l'interface doit désormais s'étoffer au fur et à mesure des systèmes.

Modifications (présentation uniquement) :
- `IntoxicationHud.cs` : carte **NUIT** centrée en haut (même fond, marges et typographie que les autres panneaux, 560 × 156 unités) avec titre, `MM:SS` en grand format, barre de proportion de temps restant. États :
  - attente : « EN ATTENTE DU PARTENAIRE » en ambre, barre pleine atténuée ;
  - en cours : blanc, barre bleue ;
  - ≤ 10 s : orange ; ≤ 5 s : rouge (barre de même couleur), sans clignotement ;
  - expiré : carte teintée rouge, « TEMPS ÉCOULÉ » sur toute la carte, barre vide.
  Les seuils 10 s / 5 s sont des constantes d'affichage du HUD, pas des règles.
- Placement : le bandeau « VOUS PORTEZ » se place sous la carte (`_timerBottom`) ; le message « VOUS ÊTES KO » aussi, et sa largeur est bornée pour ne jamais recouvrir les panneaux Ivresse (droite) et réseau (gauche) ; son titre passe de `Huge` à `Display` pour tenir dans cette largeur. (Avant cette correction, le message KO recouvrait la carte du joueur dans le panneau Ivresse à 1280×720.)
- `DebugGuiKit.cs` : deux styles ajoutés, `Display` (68) et `Banner` (50).
- `NightTimer.cs` : propriété en lecture seule `DurationSeconds` (pour la barre). Rien d'autre.

**Aucune règle réseau modifiée** : ni démarrage, ni échéance, ni `NetworkVariable`, ni `NightTimerRules`, ni `TuningConfig` ; la comparaison avec la première version montre uniquement HUD, `DebugGuiKit` et la propriété de lecture.

Vérifications (captures relues, Multiplayer Play Mode, hôte et client) :
- 1280×720 : attente, en cours, ≤ 5 s (rouge), expiré, expiré + « VOUS PORTEZ » + « E — LÂCHER », KO (client) avec « RÉVEIL AUTOMATIQUE » et chronomètre.
- 1920×1080 : en cours (04:44), expiré, expiré + portage, KO porté puis KO à terre avec chronomètre (00:03).
- Aucun chevauchement avec Ivresse, réseau, portage, invite de lâcher ni message KO aux deux résolutions. Deux premiers essais avaient révélé « TEMPS ÉCOULÉ » coupé au bord de la carte et le message KO recouvrant le panneau Ivresse : corrigés (style `Banner`, largeur du message KO).
- Valeurs : les deux instances calculent la barre et le `MM:SS` depuis le même `ServerTime` ; l'hôte et le client montraient 04:44/04:40 aux mêmes instants que les relevés de la section Comparaison.
- **Trois joueurs** : seulement deux instances étaient disponibles ici (hôte + un joueur virtuel). Le panneau Ivresse à trois cartes grandit seulement en hauteur, dans la colonne de droite ; la carte NUIT et le message KO restent centrés et bornés à gauche du panneau (1280 : carte 423–855 px, panneau dès 897 px ; 1920 : carte 635–1283, panneau dès 1345). Vérifié par géométrie, pas par capture à trois joueurs ; Dyllan a vu le jeu à trois, sans retour sur ce point.
- Non capturé : l'état orange (10–5 s) avec la nouvelle carte ; seul le rouge (≤ 5 s) l'a été.
- Tests : compilation sans erreur, 122 tests EditMode sur 122 réussis ; console sans erreur de jeu (le HUD n'a pas de test automatisé).

## Changements non commités

Tous les fichiers listés (P1-030 et P1-030a ensemble, `DebugGuiKit.cs` en plus). Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan lance une partie à deux, vérifie la lisibilité du chronomètre et règle `NightDurationSeconds` si besoin ; ensuite fiche suivante.
