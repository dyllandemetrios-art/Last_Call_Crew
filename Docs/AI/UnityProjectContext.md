# Contexte technique Unity

Analyse du 12 octobre 2026.

## État confirmé

- Projet Universal 3D propre dans `D:/REPOSITORY/Last_Call_Crew`
- Unity `6000.3.8f1`
- URP `17.3.0`
- Input System `1.18.0`
- Unity Test Framework `1.6.0`
- scène de démarrage unique : `Assets/Scenes/SampleScene.unity`
- aucun script de gameplay propre au projet détecté
- aucun package Netcode for GameObjects, Multiplayer Services ou Multiplayer Play Mode installé
- projet non lié à Unity Cloud : identifiants de projet et d'organisation vides
- branche `main` sans commit au moment de l'analyse

## Architecture retenue

Le P1 utilisera des MonoBehaviours à responsabilités courtes et Netcode for GameObjects. Le serveur ou l'hôte décide des états importants : ivresse, KO, portage, temps et résultat de partie. Les effets purement visuels restent locaux lorsque cela ne crée aucune divergence de règle.

La connexion sera construite par paliers :

1. hôte et client locaux avec Multiplayer Play Mode ;
2. capsule réseau et déplacement synchronisé ;
3. interaction réseau générique ;
4. Unity Multiplayer Services et Relay avec code de connexion ;
5. build Windows à deux instances.

## Limites actuelles

- aucune compilation de référence consignée ;
- aucun test Play Mode à deux joueurs ;
- aucun service Unity Cloud configuré ;
- aucun MCP Unity actif vérifié dans cette session ;
- le remote Git porte actuellement le nom généré `Last_Call_Crew-2026-10-12_16-30-06`.

## Sources inspectées

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `Packages/manifest.json`
- structure de `Assets/`
