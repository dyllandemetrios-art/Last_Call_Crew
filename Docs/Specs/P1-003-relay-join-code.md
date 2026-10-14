# P1-003 Connexion Relay par code

## Intention

Permettre à deux joueurs de rejoindre la même partie par Internet sans exposer leur adresse IP, tout en conservant l'autorité hôte et la boucle P1 déjà validée.

## Point de départ

- Unity `6000.3.8f1`, NGO `2.13.3` et Unity Transport déjà fonctionnels en local.
- Le projet est lié à Unity Cloud (`Last_Call_Crew`, organisation `dyllandemetrios`).
- Préserver les modifications automatiques déjà présentes dans `ProjectSettings/ProjectSettings.asset` et `Assets/Settings/DefaultVolumeProfile.asset` ; ne pas les annuler ni les mélanger à une refonte graphique.
- Branche de travail : `feature/p1-001-network-foundation`, commit publié `3fb9cd0`, avec deux fichiers Unity Cloud non commités au départ.

## Comportement observable

- L'hôte choisit `HÉBERGER EN LIGNE` et reçoit un code court affiché et copiable.
- Le second joueur saisit ce code puis choisit `REJOINDRE`.
- Après connexion, les deux joueurs retrouvent la même scène et toute la boucle P1 existante.
- Une erreur de service ou un code invalide produit un message lisible et permet une nouvelle tentative sans redémarrer le jeu.
- `ARRÊTER` ferme proprement NGO et la session Relay locale.

## Choix technique

Installer le package unifié `com.unity.services.multiplayer` compatible proposé par Package Manager pour Unity 6000.3.8f1. Ne pas installer l'ancien package Relay autonome, déprécié pour Unity 6. Utiliser l'authentification Unity anonyme, une allocation Relay et `UnityTransport` en `dtls`.

Conserver `NetworkManager` et l'autorité hôte existants. Ajouter un composant de connexion à responsabilité courte, puis adapter le HUD technique existant. Ne pas introduire Lobby, Matchmaker, compte joueur, serveur dédié ni framework de menus.

## Travail demandé à Claude Code

1. Lire `AGENTS.md`, cette fiche, `Docs/AI/P1-core-loop-handoff.md` et le rapport P1-001.
2. Vérifier Git et préserver les deux modifications Unity Cloud déjà présentes.
3. Confirmer dans l'éditeur que le projet lié correspond au `cloudProjectId` versionné avant tout appel au service.
4. Installer `com.unity.services.multiplayer` depuis Unity Registry et consigner la version exacte ainsi que ses dépendances directes ajoutées.
5. Initialiser Unity Services une seule fois, puis effectuer une connexion anonyme idempotente. Empêcher les doubles clics pendant une opération asynchrone.
6. Hôte : créer une allocation pour **un client** en plus de l'hôte, obtenir le code, configurer `UnityTransport` en DTLS, puis démarrer NGO comme hôte. En cas d'échec, libérer l'état local et afficher une erreur exploitable.
7. Client : normaliser le code saisi (`Trim`, majuscules), rejoindre l'allocation, configurer le transport en DTLS, puis démarrer NGO comme client.
8. Adapter `NetworkTestHud` au prototype : état de connexion, champ de code, boutons héberger/rejoindre/arrêter, code de l'hôte clairement lisible. Garder l'interface simple et cohérente avec `DebugGuiKit` ; aucun menu final.
9. À l'arrêt, à la destruction et après une erreur, désabonner les callbacks ajoutés et permettre une nouvelle tentative propre. Ne pas promettre de migration d'hôte.
10. Ajouter des tests EditMode seulement pour la logique pure extraite (normalisation du code et transitions locales utiles). Les mocks ne doivent pas être présentés comme une preuve Relay.

## Autorité et sécurité

- Relay transporte la connexion ; il ne change aucune règle de gameplay ni aucune autorité existante.
- Aucun secret, token, identifiant de compte ou fichier local sensible ne doit entrer dans Git.
- Les clients ne décident ni du résultat de connexion ni d'un état de gameplay.
- Ne pas journaliser de jeton d'authentification. Le code de session peut être affiché pour le test.

## Critères d'acceptation

- Unity compile sans nouvelle erreur.
- L'hôte obtient un code Relay visible.
- Une seconde instance rejoint avec ce code et NGO compte deux joueurs.
- Chaque joueur contrôle uniquement sa capsule et voit l'autre se déplacer.
- Boire une fois et afficher l'ivresse synchronisée fonctionne après connexion Relay.
- Un code invalide affiche une erreur claire puis une nouvelle tentative valide réussit.
- Arrêter puis recréer une session dans les mêmes instances ne laisse ni callback dupliqué ni connexion fantôme.
- Les consoles ne contiennent aucune erreur réseau bloquante pendant cinq minutes.

## Validation demandée

1. Hôte + client dans deux instances distinctes avec un vrai code Relay.
2. Code invalide, puis code valide sans redémarrage.
3. Déplacement dans les deux sens et une interaction de boisson.
4. Arrêt des deux côtés, nouvelle session et nouvelle connexion.
5. Relevé exact des erreurs de service éventuelles et des consoles.

Multiplayer Play Mode est accepté pour cette fiche si les deux instances passent réellement par Relay. Le build Windows séparé reste P1-004.

## Hors périmètre

Build autonome, Lobby, Matchmaker, liste publique de parties, reconnexion robuste, migration d'hôte, session pleine, départ d'un joueur en cours de manche, UI finale et modification des règles de gameplay.

## Compte rendu attendu

Créer `Docs/Reports/P1-003-relay-join-code-report.md` avec : commit de départ, package et version, fichiers modifiés, état initial des changements Unity Cloud, flux hôte/client/arrêt, protocole de test réel, résultats, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
