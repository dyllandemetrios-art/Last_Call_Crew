# P1-033 Bilan et relance

## Intention

Terminer une partie proprement, montrer le même bilan aux joueurs et permettre à l'hôte de relancer immédiatement une nouvelle nuit dans la même session réseau.

## Comportement observable

- Dès que P1-032 produit victoire ou échec, les actions de gameplay sont gelées.
- Un bilan commun affiche le résultat et l'état final de chaque joueur.
- L'hôte peut choisir `RELANCER` ; les clients voient `EN ATTENTE DE L'HÔTE`.
- La relance remet tous les systèmes dans leur état initial sans déconnexion.
- Avec deux joueurs toujours connectés, le nouveau chronomètre repart une seule fois et une nouvelle partie complète peut être jouée.

## Bilan minimal

Afficher :

- victoire ou échec ;
- temps restant ou `00:00` ;
- pour chaque joueur : rentré/dehors, ivresse finale et KO ou debout ;
- indication de relance adaptée au rôle.

Ne pas ajouter de score, progression, récompense ou statistiques persistantes.

## Gel autoritaire

- Le propriétaire ne lit plus déplacement, regard, boire, refuser, porter, lâcher ou réveiller après le résultat.
- L'hôte rejette également les demandes de gameplay reçues après le résultat ; le blocage client seul ne suffit pas.
- Les états restent visibles et stables jusqu'à la relance.
- La caméra et le HUD restent actifs pour lire le bilan.

## Relance autoritaire

L'hôte seul déclenche une relance. Utiliser une remise à zéro en place, sans recharger la scène et sans couper la session.

Dans un ordre sûr, remettre à zéro :

- portages et maintiens en cours ;
- KO et réveils automatiques ;
- ivresse au minimum ;
- retours enregistrés ;
- positions et rotations des joueurs aux points de départ ;
- résultat de nuit à `Pending` ;
- chronomètre à l'état d'attente, puis nouveau départ lorsque les joueurs requis sont présents ;
- feedbacks temporaires du HUD.

Ajouter un identifiant ou compteur de manche répliqué si nécessaire pour rendre la relance idempotente et permettre aux clients de nettoyer leur présentation une seule fois.

## Critères d'acceptation

- Victoire et échec gèlent les actions sur l'hôte et le client.
- Des demandes RPC forcées après le résultat ne modifient aucun état de gameplay.
- Les deux instances affichent le même résultat et les mêmes données finales.
- Seul l'hôte peut relancer.
- Une relance conserve les connexions et remet chaque système à son état initial.
- Les joueurs réapparaissent debout, sobres, non portés, dehors et contrôlables.
- Le chronomètre redémarre exactement une fois avec sa durée configurée.
- Une seconde partie peut produire un nouveau résultat différent du premier.
- Deux relances successives ne laissent aucun état ancien ni abonnement dupliqué.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

1. Victoire, vérification du bilan et blocage des actions.
2. Relance par l'hôte, puis vérification complète des états initiaux.
3. Échec lors de la deuxième manche et bilan correspondant.
4. Nouvelle relance puis victoire afin de détecter les états persistants.
5. Tentatives RPC forcées après résultat.
6. Client tentant de relancer : refus.
7. Rejoin pendant l'écran de bilan : réception du résultat existant.

## Limite de session

Les déconnexions pendant une manche ou une relance restent P1-040. P1-033 suppose un roster stable d'au moins deux joueurs.

## Hors périmètre

Menu principal final, changement de scène, score, statistiques détaillées, progression, sauvegarde, récompenses, Relay, reconnexion robuste et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-033-summary-and-restart-report.md` avec : commit de départ, fichiers modifiés, données du bilan, blocages locaux et serveur, ordre de remise à zéro, compteur de manche éventuel, scénarios victoire/échec/deux relances/rejoin, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
