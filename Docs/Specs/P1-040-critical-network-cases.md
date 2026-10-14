# P1-040 Cas réseau critiques

## Intention

Donner un résultat clair et sûr aux incidents réseau les plus probables du prototype : mauvais code, session pleine, départ d'un client et départ de l'hôte. Aucun incident ne doit créer une fausse victoire, laisser une partie jouable dans un état incohérent ou obliger à redémarrer l'application pour réessayer.

## Décision gameplay validée

Si un joueur se déconnecte pendant une manche active, l'hôte conclut immédiatement la manche en **échec** avec la raison `PARTENAIRE DÉCONNECTÉ`. Le premier résultat terminal reste immuable.

Cette règle est volontairement stricte pour P1 : pas de délai de grâce, reconnexion transparente ou migration d'hôte.

## Comportements attendus

### Code invalide ou session inexistante

- Le joueur reste hors ligne.
- Un message français lisible explique que le code est invalide ou la session indisponible.
- Le champ reste utilisable et une nouvelle tentative valide fonctionne sans redémarrage.

### Session pleine

- P1 reste limité à deux joueurs.
- Un troisième processus est refusé avec `SESSION COMPLÈTE` ou un message équivalent clairement identifiable.
- L'hôte et le client déjà connectés ne sont ni interrompus ni modifiés.

### Départ du client

- Avant le démarrage de la manche : retour à l'attente du partenaire, sans victoire ni échec.
- Pendant une manche active et avant tout résultat : échec autoritaire immédiat, raison `PARTENAIRE DÉCONNECTÉ`, chronomètre et gameplay gelés comme pour les autres résultats.
- Après une victoire ou un échec déjà décidé : le résultat existant ne change pas.
- Aucun joueur déconnecté ne disparaît silencieusement de l'évaluation pour permettre une victoire avec le joueur restant.

### Reconnexion après le départ du client

- La session de l'hôte et son code restent utilisables.
- Le client reconnecté reçoit le résultat d'échec existant et le bilan.
- L'hôte peut relancer lorsque deux joueurs sont de nouveau connectés.
- La nouvelle manche repart avec les deux joueurs aux points de départ prévus, indépendamment de la croissance des `ClientId` NGO. Ne plus calculer une position directement avec `3 × ClientId` lorsque cela peut placer un joueur hors de la greybox ; attribuer les emplacements selon l'ordre déterministe des joueurs actuellement connectés.

### Départ de l'hôte

- La session Relay est terminée ; le client revient hors ligne.
- Le client voit `HÔTE DÉCONNECTÉ` ou `SESSION TERMINÉE`, sans rester dans une fausse partie locale.
- Le client peut ensuite héberger ou rejoindre une nouvelle session sans redémarrer.
- Aucune migration d'hôte n'est tentée.

## Autorité et état

- L'hôte observe les connexions/déconnexions NGO et décide du résultat.
- Étendre le résultat de nuit avec une raison minimale répliquée seulement si nécessaire au HUD ; ne pas déduire la raison d'un texte local fragile.
- Le résultat et sa raison sont écrits une seule fois. Une déconnexion simultanée à l'échéance respecte la règle existante du premier résultat terminal décidé par l'hôte.
- Nettoyer tout portage ou maintien faisant référence à un joueur qui disparaît afin de ne conserver aucune référence réseau invalide.
- Les callbacks ajoutés doivent être abonnés/désabonnés une seule fois sur spawn/despawn ou enable/disable selon l'architecture existante.

## Interface

- Afficher la raison de déconnexion dans le bilan commun.
- Les erreurs de connexion doivent revenir à la ligne et rester lisibles à 960×540 et 1280×720.
- Ne pas reconstruire l'interface ni ajouter un menu final.

## Tests automatiques

Ajouter des tests de règles purs pour :

- départ avant démarrage ;
- départ pendant une manche ;
- départ après résultat ;
- immuabilité du premier résultat ;
- attribution de deux emplacements avec des `ClientId` élevés ou non consécutifs.

Les tests purs ne remplacent pas les essais à plusieurs processus.

## Validation demandée

Utiliser le build Windows et de vrais processus/profils UGS :

1. mauvais code, puis code valide sans redémarrage ;
2. hôte + client connectés, troisième processus refusé car session pleine ;
3. client quittant avant le départ : hôte en attente ;
4. client quittant pendant une manche : échec `PARTENAIRE DÉCONNECTÉ` identique sur l'hôte ;
5. client rejoignant à nouveau avec le même code : bilan reçu, relance autorisée, deux positions de départ correctes ;
6. client quittant après un résultat déjà décidé : résultat inchangé ;
7. hôte quittant : client hors ligne avec message clair, puis nouvelle connexion possible ;
8. au moins un départ pendant un portage ou maintien si le scénario est réalisable sans élargir le périmètre, afin de vérifier le nettoyage des références.

## Critères d'acceptation

- Aucun scénario ne produit de fausse victoire ni d'état contradictoire.
- L'échec de déconnexion est identique pour tous les clients encore présents et reste immuable.
- Un client reconnecté peut participer à une nouvelle manche depuis un point de départ valide.
- Un troisième joueur refusé ne perturbe pas la partie existante.
- Après le départ de l'hôte, le client peut créer ou rejoindre une nouvelle session.
- Compilation sans erreur, tests pertinents réussis et journaux sans nouvelle exception non gérée.

## Hors périmètre

Délai de grâce, reprise exacte de l'identité ou de l'état d'un joueur, migration d'hôte, reconnexion automatique, Lobby, Matchmaker, serveur dédié, persistance de compte et réseau dégradé simulé.

## Compte rendu attendu

Créer `Docs/Reports/P1-040-critical-network-cases-report.md` avec : commit de départ, fichiers modifiés, règles de chaque départ, ordre des callbacks, course déconnexion/échéance, nettoyage des références, attribution des points de départ, essais à deux et trois processus, journaux, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
