# Last Call Crew

Last Call Crew est le titre de travail de *Rentrer Vivant*, un jeu coopératif comique en ligne dans lequel deux amis doivent rentrer avant la fin de la nuit sans abandonner celui qui tombe en chemin.

Le prototype P1 doit prouver une boucle courte : rejoindre une partie, se déplacer à la première personne, accepter ou refuser un verre, subir une ivresse progressive, aider un partenaire KO et rentrer ensemble avant la fin du chronomètre.

## Piliers

- **Tout le monde rentre** : la victoire est collective.
- **Le chaos se partage** : l'état d'un joueur crée une situation pour l'autre.
- **Une nuit courte à rejouer** : 3 minutes de préparation puis 12 minutes dehors à terme.
- **Fait pour être regardé** : les conséquences restent lisibles et produisent des anecdotes.

## Périmètre P1 indispensable

- coop en ligne à deux joueurs, avec hôte et code de connexion ;
- déplacement FPS et interaction contextuelle ;
- une carte greybox ;
- boire, refuser et ivresse ;
- KO, portage et réveil ;
- chronomètre, retour collectif et bilan simple.

La maison détaillée, les objets personnels et le taxi sont conditionnels. La voix de proximité, la police, les missions, l'économie, les autres substances et le jeu à quatre sont reportés après P1.

## Technique

- Unity `6000.3.8f1` LTS ;
- Universal Render Pipeline `17.3.0` ;
- Input System `1.18.0` ;
- cible : PC Windows ;
- réseau prévu : Netcode for GameObjects, Multiplayer Services, Relay et Multiplayer Play Mode.

Les packages réseau ne sont pas encore installés. La première tâche technique consiste à créer une connexion locale hôte plus client avec deux capsules synchronisées, puis à ajouter Relay après validation de cette base.

## Documentation

- [Contexte Unity](Docs/AI/UnityProjectContext.md)
- [Backlog P1](Docs/Backlog.md)
- [Première tâche Claude Code](Docs/Specs/P1-001-network-foundation.md)

## Échéance

Prototype P1 jouable visé pour le **30 novembre 2026**. Dyllan Démétrios valide le gameplay, les priorités et le feeling.
