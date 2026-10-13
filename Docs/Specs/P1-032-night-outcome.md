# P1-032 Victoire et échec

## Intention

Conclure autoritairement la nuit : victoire si tous les joueurs de la session sont enregistrés rentrés avant ou à l'échéance, échec lorsque le temps expire alors qu'au moins un joueur manque.

## Règles

- État initial : `Pending`.
- Victoire : au moins deux joueurs participent et tous possèdent un instant de retour inférieur ou égal à l'échéance du `NightTimer`.
- Échec : le chronomètre est expiré et la condition de victoire n'est pas satisfaite.
- La comparaison utilise les instants serveur de P1-031, pas l'ordre d'exécution des `Update`.
- Le premier résultat terminal gagne et ne change plus.
- Un joueur porté déjà marqué rentré compte exactement comme un joueur debout.
- P1 reste conçu pour deux joueurs. Une troisième instance présente avant le résultat doit néanmoins être incluse dans le test « tous les joueurs connectés » ; les changements de roster et reconnexions restent P1-040.

## Autorité réseau

- Ajouter une responsabilité réseau dédiée, par exemple `NightOutcome`, écrite uniquement par l'hôte.
- Aucun client ne demande ni victoire ni échec.
- Répliquer l'état terminal et l'instant serveur de décision ; aucun trafic continu.
- Un client qui rejoint après la décision reçoit immédiatement le résultat.
- Tant que le roster reste stable, l'hôte évalue tous les `PlayerReturn` présents.

## Présentation P1-032

- Étendre le HUD existant avec un bandeau central clair : `VICTOIRE — TOUT LE MONDE EST RENTRÉ` ou `ÉCHEC — QUELQU'UN EST RESTÉ DEHORS`.
- Conserver visibles le chronomètre et les cartes joueur pour comprendre le résultat.
- Ne pas ajouter encore de bouton de relance, écran récapitulatif ou blocage des contrôles ; cela appartient à P1-033.

## Critères d'acceptation

- Deux joueurs rentrés avant l'échéance produisent une victoire identique sur toutes les instances.
- Un joueur manquant à l'expiration produit un échec identique partout.
- Un retour après l'échéance ne transforme pas l'échec en victoire.
- Un dernier retour horodaté exactement à l'échéance est accepté.
- Un partenaire porté rentré avant l'échéance compte pour la victoire.
- Le résultat ne change jamais après sa première décision.
- Un troisième joueur connecté avant la décision doit aussi être rentré pour gagner.
- Un rejoin après résultat reçoit l'état terminal sans RPC de rattrapage.
- Les règles existantes continuent à fonctionner après le résultat jusqu'à P1-033.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

1. Victoire des deux joueurs avant zéro.
2. Échec avec un seul joueur rentré.
3. Retour du second après zéro : résultat toujours échec.
4. Victoire en transportant le partenaire KO.
5. Course contrôlée autour de l'échéance avec instants avant, égaux et après.
6. Si une troisième instance est disponible : vérifier qu'elle doit également rentrer.
7. Rejoin après victoire puis après échec.

## Limite de session

Une déconnexion avant le résultat peut supprimer le `PlayerReturn` correspondant et n'est pas résolue ici. Ne pas créer d'identité persistante ou de roster complexe dans P1-032 ; consigner le cas pour P1-040.

## Hors périmètre

Blocage des contrôles, écran de bilan, statistiques, relance, changement de scène, persistance d'identité, déconnexion de l'hôte, Relay et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-032-night-outcome-report.md` avec : commit de départ, fichiers modifiés, règles exactes, autorité, gestion de la course à l'échéance, scénarios victoire/échec/portage/troisième joueur/rejoin, coût réseau, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
