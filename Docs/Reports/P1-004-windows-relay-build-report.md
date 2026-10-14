# Rapport P1-004 — Build Windows à deux joueurs via Relay

**Statut : PASS** pour le build et la boucle testée hors éditeur (deux processus distincts, Relay DTLS, vraies entrées clavier/souris pilotées par script d'entrée système). Dyllan valide toujours le ressenti, la saisie et la lisibilité des fenêtres. Un défaut d'interaction a été trouvé (voir « Anomalies ») : il n'invalide pas le critère « arrêt propre » mais impose un contournement.

## Départ

Branche `feature/p1-001-network-foundation`, commit `3fb9cd0`. Tous les changements P1-003 / P1-003a non commités conservés. Unity 6000.3.8f1, aucun nouveau package.

`Assets/DefaultNetworkPrefabs.asset` + `.meta` : **doublon NGO confirmé** (contenu identique à `Assets/_Project/Prefabs/DefaultNetworkPrefabs.asset`, une seule entrée vers le prefab joueur `64d669df…`, GUID différent `6caa043d…` référencé par aucune scène ni asset). Supprimé (et à nouveau après chaque Play/arrêt).

## Fichiers modifiés par P1-004

- `Assets/_Project/Scripts/Runtime/Network/NetworkTestHud.cs` : étiquette d'identification en bas à gauche, `LCC v{version} · build {8 premiers caractères de Application.buildGUID} · DEV`.
- Aucun autre fichier du dépôt. `ProjectSettings/UnityConnectSettings.asset` (`m_Enabled 0 → 1`, activation du lien Unity Services) a été modifié par l'éditeur pendant ce travail : non voulu par l'étape, laissé tel quel pour décision de Dyllan, comme les fichiers Unity Cloud.

## Build

- Commande : `unity cmd build --target StandaloneWindows64 --outputPath D:/REPOSITORY/LCC_TestBuild/P1-004/LastCallCrew.exe --scenes ["Assets/_Project/Scenes/BarGreybox.unity"] --options ["Development"]` (validation à blanc puis build). Les Build Settings n'ont pas été modifiés : la liste explicite ne contient que `BarGreybox`, qui est donc la scène de démarrage (les Build Settings contiennent aussi SampleScene en 0, NetworkTest et BarGreybox).
- Résultat : `Succeeded`, 0 erreur, 486 avertissements, 58 s, 219,9 Mo (211 Mo sur disque).
- Livrable : `D:/REPOSITORY/LCC_TestBuild/P1-004/LastCallCrew.exe` (x86_64, Development, hors Git). Identifiant affiché : **build `b4718523`**, `v0.1.0`.

## Lancement

Deux processus du même exécutable, fenêtrés, positionnés côte à côte :
`LastCallCrew.exe -screen-fullscreen 0 -screen-width 960 -screen-height 540 -lcc-profile p1-004-host -logFile …/host.log` et `… -lcc-profile p1-004-client -logFile …/client.log`.
Journaux conservés dans `D:/REPOSITORY/LCC_TestBuild/P1-004/` : `host-run1.log`, `client-run1.log` (premier cycle), `host.log`, `client.log` (second cycle).

## Protocole réel et résultats

Entrées envoyées à l'OS (déplacement de la souris, clic, Ctrl+V, touches par scancode), fenêtres ramenées au premier plan avant chaque série.

| Étape | Observé |
|---|---|
| Clic réel sur `HÉBERGER EN LIGNE` (hôte) | `[P1-003] host: Relay session created, code P89RLM, protocol DTLS`, presse-papiers système = `P89RLM` (copie automatique P1-003a, « CODE COPIÉ ») |
| Client : clic sur le champ, **Ctrl+V réel**, clic `REJOINDRE` | `client: joined Relay session P89RLM, NGO client=True`, `HÔTE ID 0 · 2 joueurs` / `CLIENT ID 1 · 2 joueurs` : identités NGO distinctes ; la jonction à une session de deux places par deux profils (`p1-004-host`, `p1-004-client`) prouve des identités UGS distinctes (identifiants non journalisés) |
| Déplacement / regard réels | `W` maintenu sur le client : sa position journalisée (0 ; 1 ; 0 → 11,9 ; 1 ; 9,0) est identique dans les deux journaux ; souris relative = regard ; `D` puis `W` sur l'hôte jusqu'au comptoir : invite « E — BOIRE / MAINTENIR CLIC DROIT — REFUSER » |
| Boisson (`E` réel) | hôte : `accept -> Accepted (2,27 m)`, ivresse 20 ; journal client : `decision #1: Accepted by client 0` ; capture des deux fenêtres : **20 / 100** affiché identiquement ; décroissance observée 20 → 14 → 11 → 6 → 3 → 1 |
| Chaque processus contrôle son joueur | `owner=0`/`owner=1` distincts dans chaque journal ; l'hôte voit le client bouger, le client voit la capsule de l'hôte à la porte |
| Fin de manche + `RELANCER` (clic réel) | `Failure` à l'échéance (hôte et client dehors), puis `restart request from client 0 -> Restarted`, `round 2 started`, retour aux points de départ, `04:51` |
| Stabilité | une même session a duré plus de 20 minutes (jonction à 00:16, arrêt vers 00:40, trois manches dont deux échecs d'échéance) ; aucune erreur réseau dans `host-run1.log` ; une seule ligne d'avertissement dans `client-run1.log` |
| Arrêt propre | pendant le bilan (curseur libre), clic réel sur `ARRÊTER` du client puis de l'hôte : `[P1-003] session stopped, NGO listening=False` dans les deux journaux, HUD `HORS LIGNE` |
| Fermeture et relance des deux processus | `CloseMainWindow` ×2, 0 processus restant ; relance avec les mêmes exécutables et profils |
| Ancien code `P89RLM` (collé par Ctrl+V) | `could not rejoindre: SessionException: session not found` → message « Code introuvable ou session fermée » (provoqué) ; aucune session fantôme |
| Frappe d'un code au clavier AZERTY | `8`/`9` sans Maj donnent `_`/`ç` : « Code invalide : lettres et chiffres uniquement. » (cas invalide réel, provoqué) |
| Nouvelle session | code `G9WJD8` (≠ `P89RLM`), jonction par Ctrl+V réel, `2 joueurs`, positions de départ propres (0 ; 1 ; 0) et (3 ; 1 ; 0), aucun état de l'ancien run |

## Journaux

- `host-run1.log` : 3543 lignes, 0 erreur. `client-run1.log` : `GameObjectsNetcodeNetworkHandler.StopAsync: Failed to stop session: session was never started` (avertissement du package à l'arrêt, sans conséquence).
- `host.log` : 355 lignes, 0 erreur. `client.log` : les erreurs sont celles provoquées (code périmé : `SessionException: session not found` et sa trace).
- Dans tous les journaux : `d3d12: failed to query info queue interface (0x80004002)` (message du moteur, sans lien) ; aucune exception non gérée, aucune erreur Netcode.

## Anomalies

1. **Après `Échap`, un clic gauche sur `ARRÊTER` ne l'actionne pas** : `PlayerController` (P1-015) reprend la capture à l'appui, le curseur est verrouillé avant le relâchement que le bouton IMGUI attend. Le bouton ne fonctionne qu'avec la souris déjà libre (bilan de nuit). Contournement utilisé : arrêt pendant le bilan. Tâche de correction proposée à Dyllan (non appliquée ici : modification de la capture, hors périmètre de la fiche).
2. Le message d'erreur rouge du panneau est tronqué à la largeur du panneau (pas de retour à la ligne) ; les boutons `HÔTE LOCAL` / `CLIENT LOCAL` sont tronqués dans une fenêtre 960×540.
3. Un `SetCursorPos` envoyé pendant que la souris est capturée tourne le regard : artefact de l'automatisation, pas de défaut du jeu.
4. Pour piloter proprement : scancodes nécessaires pour les touches (les codes virtuels seuls n'atteignent pas l'Input System).

## Limites

- Deux processus sur la même machine et le même réseau : le trajet Relay est réel mais pas une preuve entre deux foyers différents ; pare-feu d'un autre foyer hors périmètre.
- Les entrées sont envoyées par un script système, pas par la main de Dyllan : ressenti, fluidité, lisibilité des fenêtres et saisie au clavier AZERTY restent à valider par lui.
- Victoire collective non rejouée dans le build (échecs d'échéance observés) ; la boucle complète portage/victoire a été validée dans l'éditeur (P1-010, P1-031a).
- Timer de manche de 5 minutes : un client rejoignant avec l'identifiant 4 ou plus démarre toujours hors de la zone de départ (limite P1-040).
- Pas de test du build sur une machine tierce, ni en plein écran.

## Git

Modifications non commitées : celles de P1-003/P1-003a + `NetworkTestHud.cs` (étiquette de build) + ce rapport + `ProjectSettings/UnityConnectSettings.asset` (voir plus haut). Le build et les journaux restent hors dépôt. Aucun commit, aucun push. Prochaine action : essai de Dyllan avec deux fenêtres du build, décision sur l'anomalie 1.

## P1-015a — Curseur libre et boutons du HUD

**Statut : PASS** (build Windows, deux processus, vraies entrées). Anomalie 1 de ce rapport corrigée.

- **Cause** : `PlayerController` reprenait la capture sur l'appui du clic gauche, avant le relâchement attendu par le bouton IMGUI.
- **Correction** (`Network/PlayerController.cs` seul) : `Échap` bascule la capture (capturé → libère, sauf si cet `Échap` vient de fermer un dialogue ; libre ou verrou perdu au changement de fenêtre → recapture). Plus aucune reprise au clic gauche. Le regard n'est lu que si le curseur est réellement verrouillé. Message d'aide : « Échap : reprendre la souris ». Dans l'éditeur, la bascule suit l'état voulu (l'éditeur relâche lui-même le verrou sur `Échap`). Bilan de manche : libération puis recapture à la relance, inchangées. Aucun RPC, flux Relay ni règle de jeu modifié.
- **Build** : `D:/REPOSITORY/LCC_TestBuild/P1-015a/LastCallCrew.exe` (Development, 0 erreur), identifiant affiché `build 1b321d01` ; journaux `host.log` / `client.log` dans ce dossier (P1-004 laissé intact). Profils `p1-015a-host` et `p1-015a-client`, session `QBKGG7` (DTLS).
- **Résultats réels** : après `Échap` (message d'aide visible), un clic dans la scène **ne recapture pas** ; `Échap` recapture et le mouvement relatif de la souris fait tourner le regard ; **`Échap` puis un seul clic sur `ARRÊTER`** (client) : `session stopped, NGO listening=False` dès la première tentative ; hors ligne, clic dans le champ, `Ctrl+V`, un clic sur `REJOINDRE` : `joined Relay session QBKGG7` ; fin de manche : un clic sur `RELANCER` → `Restarted`, `round 2 started`, souris recapturée (aucun message d'aide) et regard fonctionnel ; `Échap` puis un clic sur `ARRÊTER` (hôte) : `session stopped`.
- **Journaux** : hôte 1960 lignes et client 2140 lignes, 0 erreur ni exception ; 190/190 tests EditMode ; compilation sans erreur.
- **Non rejoué** : `Échap` qui ferme un dialogue (logique `ConsumedCancelThisFrame` inchangée, validée en P1-015) ; le message rouge tronqué et les boutons locaux tronqués restent ouverts (hors périmètre).
- **Doublon NGO** : `Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) confirmé identique à `_Project/Prefabs/DefaultNetworkPrefabs.asset`, GUID `6caa043d…` référencé nulle part ; supprimé après le dernier test. Aucun commit, aucun push.
