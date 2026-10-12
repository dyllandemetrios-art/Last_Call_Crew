# P1-001 Fondation réseau locale

## Intention

Prouver la connexion et la synchronisation de deux joueurs sur la base Universal 3D avant de développer le gameplay de Last Call Crew.

## Comportement observable

Un hôte lance une partie locale. Un client la rejoint dans Multiplayer Play Mode. Chaque instance contrôle sa propre capsule à la première personne ou avec une caméra temporaire simple. Les deux instances voient les deux capsules se déplacer.

## Travail demandé

1. Lire `AGENTS.md`, `README.md`, `Docs/Backlog.md` et `Docs/AI/UnityProjectContext.md`.
2. Vérifier l'état Git et préserver tout changement existant.
3. Installer uniquement les packages compatibles avec Unity 6000.3.8f1 nécessaires à ce test : Netcode for GameObjects et Multiplayer Play Mode. Ne pas ajouter Relay, Lobby, Matchmaker, Vivox, ECS ou Netcode for Entities dans cette tâche.
4. Organiser le code du projet sous `Assets/_Project/` avec des dossiers explicites et leurs `.meta`.
5. Créer une scène de test dédiée, un `NetworkManager`, le transport local approprié et un prefab joueur réseau minimal.
6. Utiliser l'Input System existant. Ne pas introduire de framework ou d'abstraction supplémentaire.
7. Configurer Multiplayer Play Mode pour un hôte et un client si cette configuration peut être versionnée proprement. Sinon, documenter les étapes exactes pour Dyllan.
8. Ajouter un test automatique seulement s'il vérifie une logique pure utile ; ne pas fabriquer un test qui prétend prouver la connexion réelle.

## Critères d'acceptation

- Unity compile sans erreur bloquante.
- L'hôte et le client apparaissent avec des identités réseau distinctes.
- Chaque instance contrôle uniquement sa capsule.
- Les deux déplacements sont visibles sur les deux instances.
- La session reste active cinq minutes sans erreur réseau bloquante.
- Quitter le Play Mode ne laisse pas le projet dans un état cassé.
- Aucun service cloud et aucun secret ne sont requis.

## Hors périmètre

Relay, code de connexion, menu final, matchmaking, gameplay d'ivresse, interaction, assets artistiques, architecture de production complète et optimisation.

## Compte rendu attendu

Créer `Docs/Reports/P1-001-network-foundation-report.md` avec : packages et versions, fichiers créés ou modifiés, scène et prefab concernés, procédure de test, résultat réel, erreurs restantes, statut PASS/FAIL/BLOCKED et prochaine action. Ne pas commit ni push sans autorisation explicite.
