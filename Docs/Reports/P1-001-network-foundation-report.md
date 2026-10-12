# Rapport P1-001 — Fondation réseau locale

**Statut : PASS**

Dyllan a confirmé le test manuel après la passe automatisée : hôte et client fonctionnels, contrôle propriétaire et synchronisation validés pendant cinq minutes.

## Départ

- Branche `main`, commit de départ `da14d7e`, arbre propre.
- Unity 6000.3.8f1, Éditeur déjà ouvert sur le projet, piloté via Unity CLI (`com.unity.pipeline`).

## Packages installés (via `package_add`)

| Package | Version |
| --- | --- |
| `com.unity.netcode.gameobjects` | 2.13.3 |
| `com.unity.multiplayer.playmode` | 2.0.2 |

Dépendances résolues automatiquement (ex. `com.unity.transport`) : voir `Packages/packages-lock.json`. Aucun Relay, Lobby, Matchmaker, Vivox, ECS ni Netcode for Entities.

## Fichiers créés

- `Assets/_Project/Scripts/Runtime/Network/PlayerController.cs` : NetworkBehaviour ; seul le propriétaire lit l'Input System (actions `Player/Move` et `Player/Look` de `Assets/InputSystem_Actions.inputactions`, clonées à l'exécution) ; caméra et AudioListener activés uniquement pour le propriétaire ; position de départ décalée selon `OwnerClientId`. Option de test `-lcc-autowalk` (marche en cercle, entrée scriptée, pas une entrée humaine).
- `Assets/_Project/Scripts/Runtime/Network/NetworkTestHud.cs` : boutons IMGUI Host / Client / Shutdown ; arguments `-lcc-role host|client` ; log toutes les 5 s de la position des deux joueurs (`[P1-001] local=… player owner=… pos=…`).
- `Assets/_Project/Prefabs/Player.prefab` : capsule + NetworkObject + NetworkTransform (autorité propriétaire) + PlayerController + enfant `CameraRoot` (Camera + AudioListener).
- `Assets/_Project/Scenes/NetworkTest.unity` : sol, lumière, caméra d'ensemble (depth −10), `NetworkManager` + `UnityTransport` (127.0.0.1:7777 par défaut), prefab joueur assigné, `NetworkTestHud`.
- `Assets/_Project/Prefabs/DefaultNetworkPrefabs.asset` : liste de prefabs réseau référencée par la scène de test.
- Tous les `.meta` correspondants.

## Fichiers modifiés par effet de bord (à relire avant tout commit)

- `Packages/manifest.json`, `Packages/packages-lock.json` (voulu).
- `ProjectSettings/EditorBuildSettings.asset` (clé `com.unity.dt.app-ui`), `ProjectSettings/ProjectSettings.asset` (define `APP_UI_EDITOR_ONLY`, autres lignes), `Assets/Settings/PC_RPAsset.asset`, `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` (flags de préfiltrage URP ré-écrits par le build) : modifiés automatiquement par l'Éditeur/les packages IA, **pas par une décision de ma part**. Non-versionnés à ce jour : `ProjectSettings/SceneTemplateSettings.json`, `ProjectSettings/VirtualProjectsConfig.json`.

## Commandes et résultats observés

- Compilation (`recompile` + `recompile_status`) : terminée, 0 erreur, console Unity : 1 warning sans rapport (« Account API did not become accessible… »).
- Build Windows autonome de la scène `NetworkTest` vers `D:/REPOSITORY/LCC_TestBuild/` (hors dépôt) : Succeeded, 0 erreur (2 builds).
- `open_scene NetworkTest` : OK.
- `editor_play` puis `eval` (`StartHost`) : **échec** — « main thread operation timed out ». Dès lors tous les appels CLI (`editor_status`, `editor_focus`…) expirent. Le processus Unity est « Responding » mais inactif (CPU stable) : thread principal bloqué ou en attente, cause non déterminée. Aucune boîte de dialogue visible via UI Automation.

## Tests réalisés (2e passe, après retour de Dyllan)

Cause des erreurs « port 7777 déjà utilisé » : Host cliqué sur les deux instances MPPM (le clone tentait aussi de bind 7777). Cause du client qui ne se connectait pas : `Application.runInBackground` était à `false`, l'instance non focalisée ne ticke plus le réseau. Correctif : `PlayerSettings.runInBackground = true` (ProjectSettings, à valider).

Test réel : éditeur principal = **Host**, clone MPPM (`Library/VP/mppm…`, Player 2) = **Client**, pilotés via Unity CLI (`StartHost` / `StartClient`, touches via `simulate_key` sur chaque instance).
- Identités distinctes : hôte `ClientId 0` (spawn x=0), client `ClientId 1` (spawn x=3). Chaque instance voit 2 objets joueur, `IsOwner` vrai uniquement pour le sien.
- Déplacement : client W 3 s → z 0→16,04 ; hôte D 2 s → x 0→11,09. Positions identiques (à ~0,1 m) sur les deux instances ; chaque touche ne déplace que sa propre capsule.
- Durée : session maintenue ~310 s (relevés à 51, 102, 154, 205, 256, 309 s), `IsConnectedClient=True` partout, positions cohérentes des deux côtés. (Deux scripts de soak ont tourné en parallèle par erreur, d'où des touches croisées ; la cohérence hôte/client reste observée.)
- Console client : 0 erreur. Console hôte : 9 erreurs, toutes `Failed to handle /api/exec request … timed out` (outil Pipeline pendant le blocage de la 1re passe), aucune erreur Netcode/Transport.
- Sortie du Play Mode : `editor_stop` OK, éditeur à nouveau `stopped` et réactif.

Non fait : test Relay, build autonome (le build existe mais n'a pas été joué), saisie humaine clavier/souris et feeling (à valider par Dyllan).

## Procédure de test à exécuter

1. Fermer toute boîte de dialogue éventuelle dans l'Éditeur ; si l'Éditeur reste figé, le redémarrer (aucun travail non sauvegardé de ma part : tout est sur disque).
2. Ouvrir `Assets/_Project/Scenes/NetworkTest.unity`.
3. Option A — Multiplayer Play Mode : `Window > Multiplayer > Multiplayer Play Mode`, activer *Player 2* (virtuel), Play ; sur l'instance principale cliquer **Host**, sur Player 2 cliquer **Client**. (La config MPPM n'a pas pu être créée par script ; manipulation manuelle.)
4. Option B — build : `D:/REPOSITORY/LCC_TestBuild/LCC.exe -lcc-role client -lcc-autowalk` en plus d'un Play dans l'Éditeur avec clic sur **Host**.
5. Vérifier : HUD « joueurs 2 », deux capsules visibles, WASD/souris déplace uniquement sa propre capsule, positions visibles sur les deux instances (logs `[P1-001]`), tenir 5 minutes, console sans erreur, Shutdown puis quitter le Play Mode proprement.

## Erreurs / limites restantes

- Blocage de l'éditeur lors de la 1re passe : cause non identifiée, ne s'est pas reproduit.
- Interface Host/Client : cliquer Host sur les deux instances provoque l'erreur de port ; procédure : Host sur l'instance principale uniquement.
- Spawn décalé par `OwnerClientId * 3` (côté propriétaire) : positions correctes observées sur les deux instances.
- `runInBackground=true` ajouté aux ProjectSettings (nécessaire pour MPPM).
- Relay, reconnexion et conditions réseau réelles ne sont pas encore validés.

## Historique

Implémentation initiale commitée sur `feature/p1-001-network-foundation` au commit `e8f7235`. Le doublon racine et le script de construction jetable ont ensuite été supprimés.

## Prochaine action recommandée

P1-002 : interaction réseau générique autorisée par l'hôte et répliquée aux deux joueurs.
