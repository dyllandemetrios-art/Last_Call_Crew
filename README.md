# Rentrer Vivant

*Last Call Crew* est le titre provisoire de **Rentrer Vivant**, un jeu coopératif comique en ligne dans lequel deux amis doivent rentrer avant la fin de la nuit sans abandonner celui qui tombe en chemin.

Le prototype P1 doit prouver une boucle courte : rejoindre une partie, se déplacer à la première personne, accepter ou refuser un verre, subir une ivresse progressive, aider un partenaire KO et rentrer ensemble avant la fin du chronomètre.

## Piliers

- **Tout le monde rentre** : la victoire est collective.
- **Le chaos se partage** : l'état d'un joueur crée une situation pour l'autre.
- **Une nuit courte à rejouer** : une boucle compacte, chaotique et immédiatement relançable.
- **Fait pour être regardé** : les conséquences restent lisibles et produisent des anecdotes.

## Prototype P1

Le prototype local permet déjà à deux joueurs de :

- se déplacer à la première personne dans une greybox de bar ;
- boire, refuser et subir une ivresse synchronisée ;
- dialoguer avec un premier PNJ ;
- tomber KO, porter ou réveiller un partenaire ;
- rentrer ensemble avant la fin du chronomètre ;
- obtenir un résultat partagé et relancer une nouvelle manche sans déconnexion.

La prochaine étape réseau est l'intégration de Unity Relay avec un code de connexion, puis la validation de deux builds Windows.

La maison détaillée, les objets personnels et le taxi sont conditionnels. La voix de proximité, la police, les missions, l'économie, les autres substances et le jeu à quatre sont reportés après P1.

## Technique

- Unity `6000.3.8f1` LTS ;
- Universal Render Pipeline `17.3.0` ;
- Input System `1.18.0` ;
- Netcode for GameObjects `2.13.3` ;
- Multiplayer Play Mode `2.0.2` ;
- cible : PC Windows ;
- Unity Cloud lié ; Relay reste à intégrer.

L'hôte reste autoritaire sur les états importants : ivresse, KO, portage, temps, retour et résultat de manche.

## Documentation

- [Relais technique compact](Docs/AI/P1-core-loop-handoff.md)
- [Backlog P1](Docs/Backlog.md)
- [Spécifications](Docs/Specs/)
- [Rapports de validation](Docs/Reports/)

## Échéance

Prototype P1 jouable visé pour le **30 novembre 2026**. Dyllan Démétrios valide le gameplay, les priorités et le feeling.
