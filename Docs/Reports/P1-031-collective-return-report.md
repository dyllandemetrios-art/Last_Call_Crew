# Rapport P1-031 — Retour collectif

**Statut : PASS** (hôte + client, Multiplayer Play Mode, validation manuelle de Dyllan le 13 octobre 2026). Aucun calcul de victoire ou d'échec.

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `1c4a043`, arbre propre. Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers

Créés :
- `Scripts/Runtime/Night/ReturnRules.cs` (assembly `LastCallCrew.Night` existante) : règles pures (sentinelle, enregistrement unique, libellé).
- `Scripts/Runtime/Network/PlayerReturn.cs` : instant de retour répliqué du joueur.
- `Scripts/Runtime/Network/ReturnZone.cs` : zone de sortie, détection côté hôte.
- `Tests/EditMode/ReturnRulesTests.cs` : 8 cas.
- `.meta` correspondants.

Modifiés :
- `Prefabs/Player.prefab` : composant `PlayerReturn` (+14 lignes, via l'éditeur).
- `Scenes/NetworkTest.unity` : objet `ReturnZone` (+502 lignes, ajouts uniquement, via l'éditeur).
- `PlayerCarry.cs` : accesseur `TryGetCarriedPlayer` (lecture de l'état P1-021, aucune logique de portage changée).
- `IntoxicationHud.cs`, `DebugGuiKit.cs` (style `Tag`) : étiquette de retour et feedback.
- Non modifiés : `TuningConfig`, règles du chronomètre, KO, portage, réveil.

## Modèle d'autorité

- `PlayerReturn` porte une `NetworkVariable<double>` écrite uniquement par l'hôte, une seule fois (`ServerMarkReturned` est idempotent et refuse hors serveur ou si déjà rentré).
- `ReturnZone` n'agit que sur l'instance hôte et ne lit aucune demande client.

## Représentation de l'instant

Temps serveur (`NetworkManager.ServerTime.Time`), donc la même horloge que l'échéance du `NightTimer` : P1-032 pourra comparer directement. Sentinelle `ReturnRules.NotReturned = -1` (le temps serveur est ≥ 0 ; un retour à t = 0 est testé). Un booléen seul n'existe pas : `IsReturned` est dérivé de l'instant.

## Zone et trigger

`ReturnZone` (scène, position (0 ; 0 ; 18)) : enfant `Trigger` (`BoxCollider` en trigger, 8 × 3 × 8 m) distinct de l'enfant `Visual` (dalle, deux piliers, linteau, sans collider, verts, colorés par `MaterialPropertyBlock`). Les joueurs ne sont pas des corps physiques et le collider d'un joueur porté est désactivé, donc des événements trigger ne sont pas fiables : l'hôte teste à chaque frame la position de chaque joueur connecté dans le volume (`ClosestPoint == point`).

## Retour individuel

Observé : hôte à (0 ; 1 ; 18) → `returned=True t=26,269` ; client à (0 ; 1 ; 19) → `t=57,464`. Même état et même instant sur les deux instances. Un seul journal `returned` par joueur.

## Entrées répétées

L'hôte est sorti à (0 ; 1 ; 0) puis rentré à (1 ; 1 ; 17) : l'instant reste 26,269, aucun nouveau journal.

## Retour porté

Nouvelle partie : client KO porté par l'hôte, hôte hors zone → aucun retour. Hôte téléporté dans la zone : `p0 t=33,984` et `p1 t=33,984` (même instant), le porté étant à (0 ; 1,80 ; 15,05), collider désactivé, KO. Second essai plus strict : porteur à z = 14,4 (dans la zone), partenaire à z = 13,45 (hors du volume) : les deux sont quand même marqués au même instant (29,564), preuve que le partenaire est résolu par l'état P1-021 et non par sa position.

## KO non porté

Client KO placé dans la zone, hôte dehors : seul le client est marqué (`t=22,370`).

## Rejoin

Le client s'est déconnecté puis reconnecté (nouveau ClientId 2) : il voit l'hôte `returned=True t=26,269` identique, sans RPC de rattrapage ; le nouveau joueur est `ENCORE DEHORS`. (L'ancien joueur 1 disparaît avec sa déconnexion : son retour n'est pas conservé après départ.)

## Interaction avec le chronomètre

Retours avant expiration : 26,27 et 57,46 pour une échéance à 308,98 s. Échéance réécrite à 102,04 (par réflexion, sans toucher à l'asset) → `Expired` ; le nouveau client retourne après expiration : `t=117,229`, enregistré normalement, aucun résultat déclenché. P1-032 pourra donc comparer `ReturnServerTime` à l'échéance (instants 117,23 > 102,04 : « après » ; 26,27 < 102,04 : « avant »). Le chronomètre continue pendant tous les retours.

## Interface

Captures 1280×720 (hôte) et 1920×1080 (client) relues : « ENCORE DEHORS » en gris puis « RENTRÉ » en vert sur chaque carte joueur ; carte NUIT, bandeau « VOUS PORTEZ », « E — LÂCHER » et message KO sans chevauchement ; la sortie verte est lisible. Le feedback bref (« VOUS ÊTES RENTRÉS ENSEMBLE », « VOUS ÊTES RENTRÉ », « JOUEUR N EST RENTRÉ ») est codé mais non vu sur capture (il dure 2 s, plus court que le délai de capture).

## Tests

- Compilation : 0 erreur. **130 tests EditMode sur 130 réussis** (122 + 8).
- Consoles : aucune erreur de jeu. Boire, KO, portage, réveil et chronomètre : rien observé en régression ; les contrôles ne sont pas bloqués.

## Limites

- Pas de test à trois joueurs ni de build autonome ; rejoin testé avec le même éditeur virtuel.
- Un joueur qui se déconnecte perd son état (nouveau ClientId à la reconnexion) : décision à prendre pour P1-032.
- Pas de capture de la zone depuis le point de vue d'un client à l'arrivée ; le trigger détecte par position, donc une très grande vitesse (téléportation) est prise en compte au frame suivant.
- Le feedback de retour n'a pas été capturé visuellement.
- Greybox seulement : pas de porte, de maison ni de chargement de scène (hors périmètre).

## Nettoyage

`Assets/DefaultNetworkPrefabs.asset` (recréé par NGO) et `Assets/Temp` (captures) supprimés. Aucun changement de `ProjectSettings`.

## Changements non commités

Tous les fichiers listés. Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan teste la sortie à la main (seul, puis en portant un partenaire) et ajuste taille et position de la zone si besoin ; ensuite P1-032.
