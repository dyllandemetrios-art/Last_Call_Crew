# Rapport P1-021 — Portage d'un partenaire KO

**Statut : PASS** — preuves techniques hôte/client et validation manuelle de Dyllan le 13 octobre 2026.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `eefb5ab` (fiche P1-021, parent `1d883b3` qui contient P1-020 et P1-020a), arbre propre.
- Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Carry/LastCallCrew.Carry.asmdef` (référence `LastCallCrew.Config`) et `CarryRules.cs` : règles pures (validation de la prise, du lâcher, multiplicateur de vitesse, point de dépose).
- `Scripts/Runtime/Network/PlayerCarry.cs` : composant réseau de portage.
- `Tests/EditMode/CarryRulesTests.cs` : 17 tests.
- `.meta` correspondants.

Modifiés :
- `Prefabs/Player.prefab` : enfant `CarryPoint` (position locale `(0 ; 0,8 ; −0,95)`, sur le dos du porteur) et composant `PlayerCarry` (références au `TuningConfig`, à `CarryPoint` et à `CameraRoot`, créées par l'éditeur).
- `PlayerInteractor.cs` : visée du partenaire KO, `E` porter / lâcher, blocage de boire/refuser pendant le portage ; `MaxServerRange` partagé avec le portage.
- `PlayerController.cs` : multiplicateur de vitesse du portage ; `PlayerKnockout.cs` : lâcher à l'entrée en KO du porteur, libération à la sortie de KO du porté ; `Drink/DrinkRules.cs` : issue `RejectedCarrying` ; `IntoxicationHud.cs` : invites et messages ; asmdef des tests.
- Non modifiés : `TuningConfig`, `NetworkTest.unity`, collider et `NetworkTransform` (réglages) du prefab.

## Modèle d'autorité

- `PlayerCarry` (une responsabilité courte, sans framework) porte trois `NetworkVariable` écrites par l'hôte : sur le porteur l'identifiant NetworkObject du porté (+1), sur le porté l'identifiant de son porteur (+1), et la position de dépose du porté. Le client propriétaire n'envoie que deux demandes : `RequestCarryRpc(cible)` et `RequestDropRpc()`.
- Validations hôte (`ServerStart`, puis `CarryRules.EvaluateStart`) : identité de l'émetteur, cible résolue en joueur, joueur ≠ soi-même, porteur debout, ni porté ni déjà porteur, cible KO, ni portée ni porteuse, portée (même portée serveur que l'interaction : 3 m + 1 m). Le lâcher exige que le demandeur porte quelqu'un. Un joueur ne porte qu'un joueur et n'est porté que par un seul (les identifiants uniques le garantissent et la règle les contrôle).
- Les demandes boire/refuser d'un porteur sont rejetées par l'hôte (`RejectedCarrying`) en plus du blocage local.

## Méthode de suivi réseau

Pas de parenting NGO : solution équivalente retenue (la fiche l'autorise) parce que le `NetworkTransform` du joueur est en autorité propriétaire (le porté ne pilote plus rien) et que le suivre via un aller-retour par son propriétaire doublerait la latence.
- Sur **chaque instance** (hôte, porteur, porté, observateur), tant que `carriedBy` est renseigné : le `NetworkTransform` et le collider de la racine du porté sont désactivés localement et sa racine est replacée en `LateUpdate` (`DefaultExecutionOrder(200)`) sur le `CarryPoint` du porteur, avec le lacet du porteur. C'est bien la racine qui suit, pas seulement le mesh ; le visuel KO reste couché comme en P1-020.
- Au lâcher : l'hôte écrit la position de dépose (devant le porteur à 1,2 m, à la hauteur de racine du porteur, donc au sol) puis efface les liens ; chaque instance pose la racine à cette position, réactive collider et `NetworkTransform`, et le propriétaire du porté fait un `Teleport` pour resynchroniser. Le porté reste KO (aucun changement d'état KO ni d'ivresse).
- Colliders : désactivés localement pendant le transport (le porteur n'est pas gêné et un autre rayon ne touche pas le porté), restaurés au lâcher : relevés `col=False nt=False` pendant le portage, `col=True nt=True` après.

## Vitesse

`PlayerCarry.SpeedMultiplier = CarryRules.SpeedMultiplier(IsCarrying, tuning)` : `TuningConfig.CarrySpeedMultiplier` (0,6) pendant le portage, 1 sinon ; `PlayerController` le multiplie à `moveSpeed` sans toucher à la vitesse de base. Mesures (touche A tenue 2 s) : normale −11,38 m (x : −6,86 → −18,24), chargée −6,86 m (0 → −6,86), soit 0,60 ; le client porteur fait −6,87 m, soit aussi 0,60. Le relevé `speedMul` vaut 0,60 en portage et 1,00 sinon sur les deux instances.

## Résultats (Multiplayer Play Mode, pilotés par Unity CLI)

Entrées réelles injectées via l'Input System : E, touche A ; **injectés** (appels de test, non joueur) : `ServerAddDrink()` pour mettre un joueur KO, `ServerExitKnockout()`, téléportation de joueurs, appels forcés des RPC par réflexion.
- **Hôte porte le client KO** : demande acceptée ; état identique sur les deux instances (`p0 carrying=1`, `p1 carriedBy=0`), racine du porté sur `CarryPoint` (0 ; 1,80 ; −0,95). L'hôte marche de x = 0 à −6,86 : le porté suit à (−6,86 ; 1,80 ; −0,95), identique sur hôte et client.
- **Lâcher (E)** : le porté est posé en (−6,86 ; 1,00 ; 1,20) sur les deux instances, stable à 0,5 s et à 3 s, collider et `NetworkTransform` rétablis, toujours KO.
- **Sens inverse** : le client (sorti du KO par injection) porte l'hôte KO, marche (x 0 → −6,87), lâche : mêmes résultats sur les deux instances.
- **Rejets** (journal hôte) : porter un joueur debout (`RejectedTargetNotKnockedOut`), déjà porteur (`RejectedAlreadyCarrying`), soi-même (`RejectedSelf`), demandeur KO (`RejectedRequesterKnockedOut`), cible hors portée à 12 m (`RejectedOutOfRange`), lâcher sans rien porter (`RejectedNotCarrying`), boire en portant (`RejectedCarrying`) ; aucun changement d'état à chaque fois. Le cas « cible déjà portée par un tiers » est couvert par les tests purs : il faut un troisième joueur.
- **KO du porteur** : le client porteur reçoit 6 verres injectés → KO ; le porté est lâché automatiquement à (0 ; 1,00 ; 1,20), plus personne ne porte, états identiques sur les deux instances.
- **Sortie de KO du porté (injectée)** : le porteur le lâche, état cohérent.
- **Déconnexion** : le porté (client) quitte pendant le portage : l'hôte n'a plus aucun lien de portage et aucune erreur.
- **Rejoin tardif** : un 3ᵉ participant (build autonome Windows `-batchmode -nographics`, ClientId 2) rejoint pendant que l'hôte porte le client : son journal affiche, dès l'apparition, `player 1: carriedBy=0` et `player 0: carrying=?` (« ? » car l'objet du porté n'était pas encore apparu à cet instant ; l'identifiant est bien reçu), puis les positions `p1 = (0 ; 1,80 ; −0,95)` = point de portage ; après le lâcher de l'hôte il voit `p1 = (0 ; 1,00 ; 1,20)`. Aucun RPC de rattrapage.
- Interface (captures relues, 1920×1080) : porteur : bandeau « VOUS PORTEZ JOUEUR 1 », invite « E — LÂCHER », carte « KO · PORTÉ » ; porté : « VOUS ÊTES KO » et « PORTÉ PAR JOUEUR 0 », carte « KO · PORTÉ », caméra KO conservée ; invite « E — PORTER JOUEUR N » en visant un partenaire KO. Je n'ai pas fait de capture à 1280×720 pour cette tâche.
- Consoles hôte et client : 0 erreur, 1 warning sans rapport. Compilation : 0 erreur. **80 tests EditMode sur 80 réussis** (63 précédents + 17 `CarryRulesTests`).

## Limites

- Aucune animation ni IK : le porté est un capsule couché en travers du dos du porteur ; pose à régler à l'œil par Dyllan. Le porteur ne voit pas le porté (derrière lui).
- Pas de collision caméra ; pas de test avec un porté passant à travers un obstacle ; pas de niveau pour vérifier que le lâcher ne tombe pas dans un mur (sol plat uniquement).
- Dyllan a validé manuellement le portage et le juge déjà amusant à observer. Aucun test sous mauvaise connexion ni Relay ; le rejoin tardif est vérifié avec un participant en mode headless (sans écran).
- Le porté suit le porteur avec la latence de réplication du porteur (même décalage que pour tout joueur distant).
- Le diagnostic temporaire de rejoin tardif a été retiré avant commit.
- Le build autonome de test (hors dépôt, `D:/REPOSITORY/LCC_TestBuild`) a modifié `ProjectSettings/ProjectSettings.asset` (`preloadedAssets` ajoute les actions d'entrée) : j'ai annulé ce changement. NGO a recréé `Assets/DefaultNetworkPrefabs.asset` ; supprimé.

## Changements non commités

Tous les fichiers listés. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan essaie le portage à la main dans les deux sens et ajuste le `CarryPoint` si besoin ; ensuite fiche P1-022 (réveil).
