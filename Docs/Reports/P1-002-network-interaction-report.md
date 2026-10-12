# Rapport P1-002 — Interaction réseau générique

**Statut : PASS** (preuve hôte + client en Multiplayer Play Mode ; entrées injectées par l'Input System, feeling non validé).

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `b0ba836`, arbre propre.
- Unity 6000.3.8f1, NGO 2.13.3 (aucun package ajouté).

## Fichiers

Créés :
- `Assets/_Project/Scripts/Runtime/Network/InteractiveCube.cs` : `NetworkVariable<bool>` (écriture serveur) ; le visuel (couleur rouge OFF / verte ON via `MaterialPropertyBlock`) découle uniquement de cette variable, appliqué au spawn puis à chaque `OnValueChanged`.
- `Assets/_Project/Scripts/Runtime/Network/PlayerInteractor.cs` : raycast court depuis la caméra du propriétaire, demande RPC, validation hôte, retour de refus, réticule et invite `[E] Activer` en IMGUI.
- `.meta` correspondants.

Modifiés :
- `Assets/_Project/Prefabs/Player.prefab` : composant `PlayerInteractor` (portée 3 m, tolérance serveur 1 m, réglables dans l'Inspector).
- `Assets/_Project/Scenes/NetworkTest.unity` : `InteractiveCube` (cube 1×2×1 en (1,5 ; 1 ; 4), `NetworkObject` de scène + `InteractiveCube`).
- `Assets/InputSystem_Actions.inputactions` : l'action existante `Player/Interact` (déjà liée à `E`) avait l'interaction `Hold` ; supprimée pour obtenir un appui simple. Le futur « maintenir pour refuser » devra remettre un `Hold` (sur l'action ou sur un binding dédié).
- `PlayerController.cs` et `NetworkTestHud.cs` : ré-enregistrés en UTF-8 ; seuls les commentaires corrompus (« Entr�e script�e », « script� », « �diteur ») sont restaurés, aucun changement de code (diff : 1 ligne chacun).

## Flux d'autorité

1. Le propriétaire lit `Player/Interact` (copie locale de l'asset d'actions) ; `WasPressedThisFrame()` donne un seul envoi par appui.
2. Le raycast (3 m) depuis `CameraRoot` doit toucher un `InteractiveCube`, sinon rien n'est envoyé.
3. `RequestInteractRpc(NetworkObjectReference)` `[Rpc(SendTo.Server)]` : seul le propriétaire du joueur peut l'appeler (propriété exigée par défaut).
4. L'hôte revérifie : référence résolue en `NetworkObject` portant un `InteractiveCube` ; distance de `CameraRoot` (position déduite du joueur synchronisé) au point le plus proche du collider ≤ portée + tolérance. Si valide, `ServerToggle()` ; sinon refus.
5. `NetworkVariable` répliquée → même visuel sur toutes les instances. Un refus renvoie `ResultRpc(false)` au propriétaire (message « Interaction refusée » 1,5 s).
6. L'hôte journalise `[P1-002] host accepted/rejected …` pour chaque demande.

## Procédure de test

1. Ouvrir `NetworkTest.unity` dans les deux éditeurs (le clone MPPM garde sa scène en mémoire : relancer `open_scene` dessus après modification), Play.
2. Host sur l'instance principale uniquement, Client sur Player 2.
3. À la main : se placer à moins de 3 m face au cube (réticule visible, invite `[E] Activer`), appuyer sur E.
4. Test automatisé réalisé (via Unity CLI) :
   - joueurs téléportés en (1,5 ; 1 ; 1,5) et (1,2 ; 1 ; 1,5), orientés vers +z ;
   - 12 appuis alternés hôte/client sur `E`, injectés comme événements d'état clavier dans l'Input System (`InputSystem.QueueStateEvent`) sur chaque instance, 0,3 s d'appui puis 0,8 s d'attente ; état et couleur relus sur les deux instances après chaque appui ;
   - hors portée : joueurs à z = −10 (13,5 m), appui sur E (aucune demande, raycast de 3 m) puis appel forcé du RPC depuis le client puis depuis l'hôte, pour contourner le filtre client et éprouver la validation hôte.

Note : `simulate_key` du CLI déplace le joueur mais ne déclenche pas `WasPressedThisFrame()` ; l'injection par événement d'état a été utilisée à la place.

## Résultats observés

- État initial ON/ON (cube laissé ON par un essai préliminaire). Les 12 appuis alternés ont produit 12 bascules exactement, ON→OFF→ON…, état et couleur identiques sur hôte et client après chacun (rouge 0,8/0,15/0,15 ; vert 0,2/0,85/0,25).
- Journal hôte : 12 lignes `host accepted client=0|1` alternées, une par appui (un appui = un changement).
- Hors portée : appui avec visée trop lointaine → aucune demande, aucun changement ; RPC forcé client → `host rejected client=1: hors de portée (13,50 m)` ; RPC forcé hôte → `host rejected client=0: hors de portée (13,50 m)` ; cube inchangé sur les deux instances.
- Consoles à la fin : 0 erreur, 0 warning (hôte comme client). Compilation : 0 erreur.

## Limites

- Entrées injectées, pas de saisie humaine ; feeling, lisibilité de l'invite et du réticule à valider par Dyllan.
- Le message de refus (`ResultRpc`) n'a pas été vérifié visuellement.
- La position serveur du joueur est celle envoyée par le client (NetworkTransform en autorité propriétaire) : la vérification de distance n'empêche pas un client modifié de mentir sur sa position. Acceptable pour le prototype coopératif ; à reconsidérer si l'anti-triche devient un besoin.
- L'hôte ne revérifie pas la direction de visée (le tangage de la caméra n'est pas synchronisé) ; seule la portée est contrôlée.
- La bascule est un simple toggle serveur ; pas de file ni de cooldown.
- Le clone MPPM doit recharger la scène (`open_scene`) après chaque modification de la scène par l'éditeur principal.
- NGO recrée `Assets/DefaultNetworkPrefabs.asset` à chaque sauvegarde de prefab réseau ; non référencé par la scène (qui utilise la liste de `_Project/Prefabs`), supprimé avant la fin de la tâche.
- Pas de test Relay ni de build autonome.

## Changements non commités

`InputSystem_Actions.inputactions`, `Player.prefab`, `NetworkTest.unity`, `PlayerController.cs`, `NetworkTestHud.cs` (modifiés) ; `InteractiveCube.cs`, `PlayerInteractor.cs` et leurs `.meta` (créés). Aucun commit, aucun push. Scripts jetables de construction/test retirés du dépôt.

## Prochaine action recommandée

Dyllan valide à la main le ressenti (portée, invite, couleurs). Ensuite P1-003 (Unity Cloud et Relay, action de Dyllan requise) ou P1-011 selon ses priorités.
