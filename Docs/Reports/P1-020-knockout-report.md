# Rapport P1-020 — KO autoritaire (avec correction P1-020a : caméra KO)

**Statut : PASS** — validation technique hôte/client et validation manuelle de Dyllan le 13 octobre 2026, caméra KO comprise.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `a9f99e2` (fiche P1-020), arbre propre. P1-013 est commité dans `1dd9ffb`.
- Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Assets/_Project/Scripts/Runtime/Intoxication/KnockoutRules.cs` : règle pure `ShouldEnterKnockout(valeur, tuning, déjàKO)`.
- `Assets/_Project/Scripts/Runtime/Network/PlayerKnockout.cs` : état KO répliqué et visuel couché.
- `Assets/_Project/Tests/EditMode/KnockoutRulesTests.cs` : 9 tests.
- `.meta` correspondants.

Modifiés :
- `PlayerIntoxication.cs` : déclenchement du KO ; `[RequireComponent(PlayerKnockout)]`.
- `PlayerController.cs` : aucun déplacement ni rotation de caméra (auto-walk compris) si KO.
- `PlayerInteractor.cs` : entrées bloquées si KO, refus en cours abandonné, rejet côté hôte, `ServerCancelRefusal()`.
- `Drink/DrinkRules.cs` : nouvelle issue `RejectedKnockedOut`.
- `IntoxicationHud.cs`, `DebugGuiKit.cs` : extension de l'interface existante (voir plus bas), style `Huge` mis en cache avec les autres.
- `Prefabs/Player.prefab` : le mesh de la capsule passe de la racine à un enfant `Visual` ; composant `PlayerKnockout` ajouté (avec, depuis P1-020a, la référence à `CameraRoot` et les réglages de caméra KO). La racine garde son `CapsuleCollider` et son `NetworkTransform`. `NetworkTest.unity` n'est pas modifiée.

## Modèle d'autorité

- `PlayerKnockout` porte `NetworkVariable<bool> _isKnockedOut` (écriture serveur, lecture par tous). Responsabilité distincte de l'ivresse.
- Seul l'hôte déclenche et écrit. Aucun RPC ne permet au client de se mettre KO ou de se relever.
- Un client qui rejoint reçoit l'état par la `NetworkVariable` ; le visuel est appliqué sans transition à l'apparition.

## Chemin exact du déclenchement

Entrée E (ou refus validé) → `AcceptRequestRpc` sur l'hôte → validations P1-012 → `offer.ServerDecide(Accepted)` → `PlayerIntoxication.ServerAddDrink()` → `_serverValue` borné à 100 → `KnockoutRules.ShouldEnterKnockout(_serverValue, tuning, _knockout.IsKnockedOut)` → `PlayerKnockout.ServerEnterKnockout()`.

`ServerEnterKnockout()` est idempotent : sans effet et sans log si le joueur est déjà KO. Il appelle aussi `PlayerInteractor.ServerCancelRefusal()` qui efface côté hôte un éventuel début de refus (sinon un futur réveil pourrait « finir » un ancien refus). La décroissance étant gelée à 100 (P1-013a), le KO ne peut venir que d'un gain.

## Actions bloquées

Côté propriétaire :
- `PlayerController.Update` : retour immédiat si KO (déplacement, regard, auto-walk).
- `PlayerInteractor.Update` : si KO, `_aimedOffer = null` (le HUD n'affiche plus l'invite), `_holding = false` (refus abandonné sans envoyer de RPC) et plus aucune lecture d'acceptation ni de refus.

Côté hôte :
- `ServerResolve` (commun aux trois RPC `AcceptRequestRpc`, `RefuseStartRpc`, `RefuseEndRpc`) rejette avec `RejectedKnockedOut` toute demande émise par un joueur KO, après le contrôle de l'identité de l'émetteur et avant la portée. Ni la proposition ni l'ivresse ne sont modifiées.

## Présentation

- Le mesh est un enfant `Visual` ; en KO il est tourné de 90° autour de Z et descendu de 0,5 m (position locale (0, −0,5, 0) : couché sur le sol, rayon 0,5). La transition (0,25 s) est locale à chaque instance et dérivée de la valeur répliquée ; elle fonctionne dans les deux sens.
- La racine réseau (rotation 0,0,0, Y = 1,00) et le `CapsuleCollider` (direction Y, hauteur 2, rayon 0,5) ne changent pas, vérifiés sur les deux instances.
- Propriétaire KO : voile sombre léger plein écran dessiné avant les panneaux (le panneau Ivresse reste lisible) et grand message central « VOUS ÊTES KO ». Les cartes d'ivresse affichent « KO · À TERRE » (rouge) pour le joueur KO sur toutes les instances. Pas d'animation, pas de nouvel écran concurrent.

## Tests automatiques

- Compilation : 0 erreur.
- `unity command run_tests --mode EditMode --filter LastCallCrew.Tests` : **63 tests, 63 réussis** (54 précédents + 9 `KnockoutRulesTests` : entrée au seuil, au-dessus, sous le seuil, jamais de re-déclenchement si déjà KO, NaN, cinq verres plus verres supplémentaires = une seule entrée, seuil lu depuis la config).

## Validation hôte + client (Multiplayer Play Mode, pilotée par Unity CLI)

**Injectés, pas des saisies humaines** : touches E, A, bouton droit et delta souris envoyés comme événements d'état à l'Input System de chaque instance ; ivresse accélérée par appels directs `ServerAddDrink()` sur l'hôte (3 verres injectés au départ de l'hôte, et pour monter le client à ≥ 80 avant le test de maintien interrompu), `ServerExitKnockout()` pour le test de la transition inverse. Les acceptations qui amènent l'hôte à 100 sont des E injectés réels. Une première passe avait des scénarios invalides (le KO n'était pas atteint quand je testais les blocages) ; seule la passe corrigée ci-dessous est retenue.

1. **KO de l'hôte** : après 3 verres injectés (≈ 60) et 3 E réels, p0 = 100,0 « Seuil KO », KO = True sur l'hôte et sur le client (observateur). Une seule ligne `[P1-020] host: client 0 is knocked out`.
2. **Stabilité** : KO maintenu plus de 10 s sur les deux instances ; toujours une seule ligne `[P1-020]` ; consoles à 0 erreur, 1 warning sans rapport.
3. **Hôte KO tente d'agir** : touche A 1 s, delta souris, E, bouton droit 2,6 s : position (1,50 ; 1,00 ; 1,50), yaw 0, pitch 0 identiques avant et après, nombre de décisions inchangé (3), ivresse inchangée. Appels forcés des trois RPC depuis l'hôte KO : `RejectedKnockedOut` ×3 ; ni proposition ni ivresse modifiées.
4. **Client non KO pendant que l'hôte est KO** : A 1 s (x : 1,20 → −5,96) et souris (yaw 6°, pitch 2°) ; E : p1 passe de 0 à 19,7 (décision n°4) ; refus complet : décision n°5 « Refused » ; l'hôte reste à 100.
5. **Verre supplémentaire à 100 sur un joueur déjà KO (injecté)** : « 100,00 → 100,00 », toujours une seule ligne `[P1-020]`, pas de nouvelle écriture visible.
6. **Maintien interrompu par le KO** : client monté à 96 (injecté), bouton droit maintenu (début de refus reçu par l'hôte, `IsHolding = true`), verre injecté à 0,8 s → p1 = 100, KO ; `IsHolding = false` juste après ; relâchement ; **aucune décision** (compteur resté à 5) et aucun `refuse-end` accepté ; deuxième ligne `[P1-020] … client 1 is knocked out`.
7. **Client KO tente d'agir** : A, souris, E, bouton droit : position, yaw et pitch identiques ; décisions inchangées ; appels forcés depuis le client KO : `RejectedKnockedOut` ×3.
8. **Deux joueurs KO** : états KO = True sur les deux instances, visuels identiques.
9. **Rejoin tardif** : le client quitte puis rejoint (devient le ClientId 2) ; 1 s après `StartClient`, il voit déjà p0 « Seuil KO », KO = True, et le visuel couché, sans RPC de rattrapage ; son propre joueur est à 0, non KO.
10. **Transition inverse (injectée, non déclenchée par le jeu)** : `ServerExitKnockout()` sur l'hôte → KO = False, visuel (0,0,0), mouvement à nouveau possible (A : x 1,50 → −4,78).
- Valeurs de la racine et du collider en KO, relevées sur les deux instances : `rootRot = (0,0,0)`, `rootY = 1,00`, `collider(dir=1, h=2,0, r=0,5)` ; visuel `visRot = (0,0,90)`, `visPos = (0,00 ; −0,50 ; 0,00)`.
- Consoles hôte et client à la fin : **0 erreur**, 1 warning sans rapport chacune.

## Captures du Game view (relues)

- 1920×1080, hôte KO : voile sombre, message « VOUS ÊTES KO » centré, carte « VOUS · Joueur 0 — 100 / 100 — KO · À TERRE » en rouge, carte « AUTRE · Joueur 1 — 17 / 100 — SOBRE » lisible, aucun chevauchement, plus d'invite boire/refuser.
- 1920×1080, client observateur : la capsule de l'hôte apparaît couchée sur le sol, carte « AUTRE · Joueur 0 — KO · À TERRE », carte « VOUS » normale.
- 1280×720, hôte avec les deux joueurs KO : message et deux cartes KO lisibles, panneaux sans chevauchement.
- Le Game view des deux éditeurs a été remis en « Free Aspect » après les captures ; les images temporaires ne sont pas conservées.

## Correction P1-020a — caméra KO

**Retour de Dyllan (playtest)** : le KO fonctionne et se lit bien, mais le propriétaire ne ressent pas la chute : la caméra restait en première personne et le message masquait le fait que son corps est au sol. L'observateur voyait déjà correctement la capsule couchée. (La mécanique « impossible de refuser à forte ivresse » n'appartient pas à P1-020 : reportée.)

**Correction** (`PlayerKnockout.cs`, `IntoxicationHud.cs`, prefab) :
- Au passage en KO, la caméra du **propriétaire uniquement** recule progressivement vers une vue derrière et au-dessus du corps couché, orientée vers lui, en `LateUpdate` (pour prendre le pas sur `PlayerController` pendant le retour). La caméra reste enfant du joueur ; seule `CameraRoot` bouge : jamais la racine réseau, le collider ni le `NetworkTransform`. Aucune collision caméra, aucun Cinemachine, aucune nouvelle dépendance.
- Aucun contrôle de caméra pendant le KO : `PlayerController` ne la pilote plus et `PlayerKnockout` réécrit la pose finale à chaque frame, donc elle ne dérive pas.
- À la sortie (`ServerExitKnockout()`), la caméra revient en `0,35 s` à la pose première personne mémorisée au début de la chute (position et inclinaison), puis `PlayerController` reprend la main avec le même pitch : aucune cassure.
- Un joueur qui rejoint pendant son KO démarre directement à la pose KO (sans transition).
- Message « VOUS ÊTES KO » remonté dans le tiers haut de l'écran et réduit en hauteur (120 unités) pour ne pas cacher le corps ; voile, HUD et texte conservés.

**Paramètres retenus** (sérialisés sur `PlayerKnockout`, réglables dans l'Inspector) : position locale de la caméra KO `(0 ; 1,8 ; −3,4)`, rotation locale `(32°, 0°, 0°)`, durée `0,35 s`, interpolation `SmoothStep`.

**Preuves hôte + client** (Multiplayer Play Mode, injections de test : ivresse par `ServerAddDrink()`, touches, souris, `ServerExitKnockout()` ; le client est ClientId 1) :
- Avant KO : toutes les caméras en `(0 ; 0,60 ; 0)`, rotation `(0,0,0)` sur les deux instances.
- Progression du recul : pour la rendre observable, la durée a été ralentie à 3 s par réglage runtime (non enregistré) ; relevés successifs de la caméra de l'hôte : `(0 ; 0,62 ; −0,07)` → `(0 ; 0,80 ; −0,58)` → `(0 ; 1,10 ; −1,42)` → `(0 ; 1,43 ; −2,34)` → pose finale `(0 ; 1,80 ; −3,40)` avec inclinaison 32°. La durée réelle (0,35 s) a ensuite été rétablie.
- État final avec la durée réelle : caméra de l'hôte KO en pose KO ; la caméra du même joueur vue depuis le client (non propriétaire) reste en `(0 ; 0,60 ; 0)` ; la caméra du client est inchangée ; racine `(1,50 ; 1,00 ; 1,50)` et rotation `(0,0,0)` identiques avant et après sur les deux instances, collider inchangé (direction Y, hauteur 2, rayon 0,5), visuel couché `(0,0,90)` / `(0 ; −0,5 ; 0)`. Pas de saut de la racine.
- Contrôles tentés pendant le KO (A 1 s, souris, E, bouton droit) : position, yaw et pitch (32°) de l'hôte identiques avant et après ; caméra identique.
- Le client devient KO à son tour : sa caméra passe en pose KO `(0 ; 1,80 ; −3,40)` (vue sur sa propre instance), alors que la copie de sa caméra sur l'hôte reste en première personne.
- **Sortie de KO injectée** : hôte : caméra revenue à `(0 ; 0,60 ; 0)` rotation `(0,0,0)`, déplacement (x : 1,50 → −4,88) et souris (yaw 6°, pitch 2°) de nouveau possibles ; client : caméra revenue à `(0 ; 0,60 ; 0)` rotation `(0,0,0)`.
- Consoles hôte et client : 0 erreur, 1 warning sans rapport chacune ; 63 tests EditMode sur 63 réussis (aucun test ajouté : la correction est visuelle et non testable en isolation).
- **Captures relues (Game view)** : 1920×1080 et 1280×720 côté hôte KO, 1920×1080 côté client KO : le corps couché du propriétaire est entièrement visible au centre, vu de derrière et au-dessus, le message est au-dessus du corps sans le couvrir, les deux panneaux restent lisibles. Observateur (1920×1080) : la capsule couchée reste visible. Après sortie : retour en première personne. Dans la capture hôte 1920×1080, une seconde capsule debout (celle du client, placée presque au même endroit pour les besoins du test) chevauche visuellement le corps de l'hôte : c'est un artefact du placement, pas un défaut de la caméra.

**Retour manuel de Dyllan** : essai à la main avec un autre joueur placé derrière le joueur KO, sans problème de collision de la caméra avec ce joueur.

**Limites P1-020a** : pas de collision caméra (la caméra pourrait traverser un mur si le joueur tombe contre un obstacle, aucun mur dans la scène de test) ; pose réglée à l'œil sur une capsule symétrique et un sol plat. Dyllan accepte ce rendu pour le prototype.

## Limites

- Dyllan a validé manuellement le KO et la caméra reculée. La capsule couchée n'a pas d'orientation propre (symétrique).
- Caméra KO : provisoire, enfant du joueur, sans collision caméra (voir P1-020a) ; aucun effet de caméra élaboré.
- Le joueur KO reste KO indéfiniment ; `ServerExitKnockout()` existe pour P1-022 mais n'est appelé par aucun code de jeu.
- La vraie souris de la machine peut interférer avec mes entrées injectées ; les scénarios retenus ont été vérifiés par les positions et les journaux.
- Pas de test Relay, de déconnexion de l'hôte ou de build autonome. NGO a recréé `Assets/DefaultNetworkPrefabs.asset` ; supprimé (non référencé).

## Changements non commités

Tous les fichiers listés. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan joue la séquence KO à la main (hôte puis client), juge la présentation, puis fiche P1-021 (portage) et P1-022 (réveil).
