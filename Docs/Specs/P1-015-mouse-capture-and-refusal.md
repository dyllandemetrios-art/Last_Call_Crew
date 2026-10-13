# P1-015 Capture souris et refus

## Intention

Rendre les playtests confortables avant la greybox : le pointeur ne doit pas s'échapper de la Game View pendant le contrôle de la caméra, et le clic droit maintenu doit de nouveau permettre de refuser un verre.

P1-014 reste validé. Cette tâche est une correction ciblée d'entrée et de capture, sans refonte des interactions.

## Point de départ observé

Lors du playtest réel de Dyllan en Multiplayer Play Mode :

- le pointeur se déplace hors de la Game View pendant le regard ;
- le clic droit maintenu ne déclenche pas le refus ;
- le dialogue `E`, puis `1/2/3`, fonctionne.

Le lien entre les deux symptômes n'est pas prouvé. Commencer par les reproduire et identifier si le défaut vient du focus de fenêtre, du verrouillage du curseur, du binding Input System ou de la logique de refus.

## Comportement attendu

- Quand une instance locale possède son joueur et que sa Game View est active, le regard à la souris reste fluide sans que le pointeur sorte de la zone de jeu.
- Une action volontaire et évidente permet de reprendre la capture après l'avoir perdue ou après avoir changé de fenêtre.
- L'éditeur ne doit pas devenir impossible à quitter : `Échap` ou la convention Unity appropriée doit libérer la capture lorsqu'aucun dialogue n'utilise cette touche.
- Si un dialogue est ouvert, `Échap` ferme d'abord le dialogue sans produire de réaction.
- Maintenir le clic droit sur une proposition de boisson valide affiche la progression puis refuse après la durée configurée.
- Relâcher trop tôt annule toujours le refus.
- Le clic droit fonctionne pour l'hôte et pour le client propriétaire, sans déclencher d'action chez l'autre joueur.

## Contraintes

- Réutiliser le Player Input et les actions existantes ; ne pas ajouter de package.
- Ne pas changer les commandes validées (`E`, clic droit, `F`, `1/2/3`).
- Ne pas forcer le verrouillage à chaque frame : respecter le focus et permettre une sortie volontaire.
- Distinguer le comportement nécessaire en build de l'adaptation éventuelle à Multiplayer Play Mode.
- Aucun changement de règles réseau, de durée de refus, de dialogue ou de HUD hors feedback indispensable.

## Critères d'acceptation

- Cause du clic droit non fonctionnel identifiée et consignée.
- Hôte et client peuvent chacun effectuer un refus complet avec une vraie entrée souris.
- Un relâchement anticipé ne produit aucune décision.
- Le regard fonctionne pendant au moins une minute par instance sans fuite involontaire du pointeur.
- Changer d'instance puis reprendre la capture fonctionne de façon compréhensible.
- `Échap` ferme correctement un dialogue ouvert et permet encore de reprendre ou libérer la souris.
- Boire, porter, réveiller et choisir `1/2/3` ne régressent pas.
- Compilation, tests et consoles restent sans nouvelle erreur.

## Validation demandée

Tester d'abord dans Multiplayer Play Mode avec de vraies entrées, pas uniquement des événements injectés :

1. regard et capture sur l'hôte pendant une minute ;
2. passage au client, reprise de capture et regard pendant une minute ;
3. refus complet puis refus relâché trop tôt sur chaque rôle ;
4. dialogue ouvert puis `Échap`, reprise du regard et nouveau refus ;
5. contrôle rapide de `E`, `F` et `1/2/3`.

Si Multiplayer Play Mode impose une limite impossible à corriger proprement, vérifier un build Windows avant de conclure et documenter précisément la différence.

## Hors périmètre

Refonte de caméra, sensibilité avancée, manette, rebinding, interface finale, greybox, nouveaux dialogues, Relay et effets visuels d'ivresse.

## Compte rendu attendu

Créer `Docs/Reports/P1-015-mouse-capture-and-refusal-report.md` avec : cause, fichiers modifiés, comportement éditeur/build, protocole de capture, tests réels hôte/client, régressions, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne rien commit ni push.
