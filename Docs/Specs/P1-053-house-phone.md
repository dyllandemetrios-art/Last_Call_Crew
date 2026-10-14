# P1-053 Téléphone

## Intention

Ajouter le téléphone à la préparation collective et terminer la checklist de départ `clés + portefeuille + téléphone`. Le téléphone préparera l'appel du taxi, sans créer encore le véhicule, le trajet ou la ville.

## Décision de prototype

Le téléphone appartient au groupe pendant la manche, comme les clés et le portefeuille. Une personne le prend ; l'état est partagé, persiste malgré un KO ou une déconnexion et se réinitialise à la relance.

Aucun menu de téléphone ni appel n'est ajouté dans cette fiche. Le futur taxi vérifiera la présence du téléphone pour être appelé et celle du portefeuille pour être payé.

## Comportement observable

- Un téléphone greybox est posé dans la maison à un troisième emplacement identifiable, sans bloquer la porte.
- Invite : `E — PRENDRE LE TÉLÉPHONE`.
- Après validation par l'hôte, il disparaît sur toutes les instances.
- Feedback partagé : `TÉLÉPHONE RÉCUPÉRÉ`.
- La barre de préparation affiche clairement les trois états : clés, portefeuille et téléphone.
- Quand les trois sont pris, afficher brièvement `PRÊTS À PARTIR` et un état compact `DÉPART · PRÊT`.
- Ce statut n'empêche pas les joueurs de sortir s'ils ont oublié un objet ; il informe seulement.

## Architecture

Suivre les conventions validées de P1-051 et P1-052 : interaction propriétaire, validation complète par l'hôte, état répliqué indépendant, visuel et collider dérivés de cet état.

Le troisième objet justifie de supprimer uniquement la duplication devenue évidente dans les règles ou l'affichage, si cela peut être fait avec un petit changement sûr. Ne pas transformer ces trois drapeaux en inventaire générique, base de données d'objets ou framework d'items.

## Validations hôte

Refuser : mauvais émetteur, mauvaise cible, hors portée, manche gelée, joueur KO, joueur porté, joueur portant un partenaire et téléphone déjà pris. Une seule demande concurrente peut réussir.

## Cycle de manche

- Relance : clés, portefeuille et téléphone redeviennent disponibles avant la reprise du contrôle.
- Late join : réception immédiate des trois états.
- Déconnexion du demandeur : téléphone toujours pris.
- Les trois objets restent indépendants ; prendre ou réinitialiser l'un ne doit pas modifier les autres hors remise à zéro complète de manche.

## Tests et validation

1. Prise du téléphone par l'hôte puis par le client sur une nouvelle manche.
2. Concurrence et principaux rejets sans transition parasite.
3. Clés et portefeuille restent indépendants.
4. Le feedback `PRÊTS À PARTIR` apparaît une seule fois au passage de deux à trois objets pris.
5. Late join après préparation complète.
6. Déconnexion du joueur ayant pris le téléphone.
7. Relance : les trois objets réapparaissent et `DÉPART · PRÊT` disparaît.
8. Boire et prendre les deux autres objets restent fonctionnels.

Les demandes peuvent être injectées. Ne pas demander à Dyllan de rejouer toute la boucle.

## Critères d'acceptation

- Téléphone synchronisé et validé par l'hôte.
- Trois états indépendants, lisibles dans une checklist compacte.
- Transition `PRÊTS À PARTIR` unique et remise à zéro propre.
- Aucun taxi, monnaie, échange ou nouvelle zone ajouté.
- Compilation sans erreur, tests pertinents réussis et aucune nouvelle erreur bloquante.

## Hors périmètre

Interface de téléphone, appel, batterie, contacts, taxi, trajet, ville, monnaie, échange avec les PNJ et inventaire général.

## Compte rendu attendu

Créer `Docs/Reports/P1-053-house-phone-report.md` avec : commit de départ, fichiers/scène, réutilisation éventuelle, autorité, indépendance des trois objets, checklist complète, relance, reconnexion, tests, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push après l'implémentation.
