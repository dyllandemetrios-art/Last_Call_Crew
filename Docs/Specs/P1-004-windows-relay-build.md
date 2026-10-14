# P1-004 Build Windows à deux joueurs via Relay

## Intention

Prouver que la boucle P1 fonctionne hors de l'éditeur dans deux processus Windows connectés par Relay.

## Point de départ

- P1-003 et P1-003a sont validés : Relay DTLS, code de connexion et copie automatique fonctionnent dans Multiplayer Play Mode.
- Utiliser Unity `6000.3.8f1`, la scène `BarGreybox.unity` et les packages déjà présents.
- Préserver tous les changements non commités de P1-003 ; ne pas nettoyer ou réécrire les assets sans rapport avec le build.

## Livrable

Produire un build Windows de développement identifiable dans `D:/REPOSITORY/LCC_TestBuild/P1-004/`, hors du dépôt Git. Un seul exécutable lancé deux fois suffit : première instance hôte, seconde instance cliente.

Le build doit afficher au minimum un identifiant de prototype ou la date/commit de construction dans la fenêtre ou le HUD afin de savoir quelle version est testée. Ne pas ajouter de système de version complet.

## Travail demandé à Claude Code

1. Lire `AGENTS.md`, cette fiche et le rapport P1-003 ; vérifier Git avant toute modification.
2. Vérifier que `BarGreybox.unity` est bien la scène de démarrage du build et que ses références réseau/prefabs sont valides.
3. Créer le build Windows x86_64 en mode Development dans le dossier externe demandé. Ne pas versionner le build.
4. Lancer deux processus distincts avec des profils UGS distincts (`-lcc-profile p1-004-host` et `-lcc-profile p1-004-client`) pour éviter le partage d'identité anonyme.
5. Dans le premier processus, héberger en ligne et relever le code automatiquement copié. Dans le second, rejoindre avec ce code.
6. Tester avec de vraies entrées clavier/souris : déplacement des deux joueurs, regard, boire une fois, synchronisation de l'ivresse, arrêt propre.
7. Fermer les deux processus, les relancer et refaire une connexion avec un nouveau code afin de détecter un état persistant ou une session fantôme.
8. Lire les journaux Player des deux processus et distinguer les erreurs provoquées des erreurs bloquantes.

## Critères d'acceptation

- Le build se termine sans erreur et produit un exécutable lançable.
- Deux processus possèdent des identités UGS et NGO distinctes.
- L'hôte obtient un code Relay ; le client le rejoint.
- Chaque processus contrôle uniquement son joueur et observe l'autre.
- Une boisson produit la même ivresse sur les deux écrans.
- La session reste stable cinq minutes sans erreur réseau bloquante.
- Arrêter et recréer une session avec les mêmes exécutables fonctionne.
- Les journaux et le chemin exact du build sont consignés.

## Validation de Dyllan

Claude Code peut automatiser la construction et préparer les deux processus, mais Dyllan valide le ressenti réel des fenêtres, la saisie, la souris et la lisibilité. Un test injecté ne remplace pas cette validation.

## Hors périmètre

Installateur, signature, distribution publique, Steam, pare-feu d'un autre foyer, cross-platform, Lobby, Matchmaker, reconnexion robuste, migration d'hôte, session pleine et correction des cas P1-040.

## Compte rendu attendu

Créer `Docs/Reports/P1-004-windows-relay-build-report.md` avec : commit de départ, fichiers modifiés, commande ou profil de build, chemin du livrable, arguments de lancement, code Relay testé, comportements observés, journaux, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
