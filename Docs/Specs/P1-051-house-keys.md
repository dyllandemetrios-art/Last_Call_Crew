# P1-051 Clés de la maison

## Intention

Ajouter le premier objet domestique utile au départ sans construire prématurément un inventaire complet. Les joueurs peuvent prendre les clés de la maison avant de partir et voient tous le même état.

## Décision de prototype

Les clés sont un objectif **collectif** : dès qu'un joueur les prend, le groupe possède les clés pour la manche. Elles ne sont pas attachées à un porteur, ne tombent pas au KO ou à la déconnexion et ne verrouillent pas encore la porte.

Ce choix évite un blocage définitif de la boucle. Une possession individuelle, le transfert, la perte et les serrures seront décidés seulement si le gameplay le justifie.

## Comportement observable

- Un petit objet `Clés` est posé sur une surface clairement visible dans la maison, près de la sortie mais sans bloquer le passage.
- En le visant à courte portée, le propriétaire voit `E — PRENDRE LES CLÉS`.
- Une pression sur `E` demande la prise ; l'hôte vérifie l'identité, la cible, la portée et l'état du joueur.
- Après validation, les clés disparaissent sur toutes les instances et le HUD affiche `CLÉS · PRISES`.
- Une seconde demande ne produit aucun changement.
- Un client rejoignant tard reçoit immédiatement l'état courant.
- À la relance de la manche, les clés réapparaissent et le statut redevient `CLÉS · À PRENDRE`.

## Autorité

- L'hôte seul écrit l'état répliqué `Available` ou `Taken`.
- Réutiliser le raycast et les validations d'interaction existants dans `PlayerInteractor` ; ne pas créer un framework générique d'inventaire.
- Refuser les demandes hors portée, mauvaise cible, mauvais émetteur, joueur KO, joueur porté, joueur portant un partenaire ou manche gelée.
- Aucun résultat client ne doit être accepté sans nouvelle vérification de la cible et de la distance par l'hôte.

## Présentation

- Greybox simple : trousseau ou petit bloc métallique/jaune facilement identifiable, sans asset tiers obligatoire.
- Changement visuel piloté par l'état réseau : visible si disponible, masqué si pris.
- Ajouter le statut des clés dans le HUD existant sans créer un nouveau grand panneau.
- Feedback bref pour les deux joueurs : `CLÉS RÉCUPÉRÉES`.

## Cycle de manche

- La relance P1-033 remet les clés à disposition avant que les joueurs reprennent le contrôle.
- Une déconnexion du joueur ayant pris les clés ne les fait pas réapparaître : l'état appartient à la manche, pas au joueur.
- Victoire, échec, retour à la maison et chronomètre restent inchangés. Les clés ne sont pas encore une condition de victoire.

## Tests automatiques

Extraire seulement la règle pure utile et tester :

- disponible → prise acceptée ;
- déjà prises → aucune seconde transition ;
- demande invalide → état inchangé ;
- remise à zéro de manche → disponibles ;
- déconnexion du demandeur après succès → état collectif conservé.

## Validation demandée

1. Hôte prend les clés : disparition et HUD identiques sur les deux instances.
2. Nouvelle manche : clés de nouveau visibles.
3. Client prend les clés : même résultat.
4. Demandes hors portée, joueur KO, porté, porteur et manche terminée : refus sans disparition.
5. Deux demandes presque simultanées : une seule prise validée.
6. Client rejoignant après la prise : clés absentes et HUD correct.
7. Déconnexion du joueur ayant pris les clés : elles restent prises jusqu'à la relance.
8. Boire, dialogue, portage, réveil et retour restent fonctionnels.

## Critères d'acceptation

- L'état des clés reste identique sur toutes les instances.
- Une seule prise est possible par manche.
- L'hôte valide toutes les conditions importantes.
- La relance restaure proprement l'objet.
- Aucun inventaire général, serrure ou dépendance supplémentaire n'est introduit.
- Compilation sans erreur, tests pertinents réussis et consoles sans nouvelle erreur bloquante.

## Hors périmètre

Porte verrouillée, perte des clés, porteur individuel, échange, dépôt au sol, modèle final, animation de main, son final, portefeuille, téléphone, taxi et inventaire extensible.

## Compte rendu attendu

Créer `Docs/Reports/P1-051-house-keys-report.md` avec : commit de départ, fichiers et scène modifiés, modèle d'autorité, validations hôte, comportement à la relance/reconnexion/déconnexion, tests, régressions, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
