# P1-055 — Taxi collectif dans la scène continue

## Intention

Permettre au groupe préparé de rejoindre rapidement l'autre quartier par un trajet de taxi visible d'environ douze secondes, sans changement de scène et sans conduite par les joueurs.

Décision de Dyllan : le taxi est collectif. Il ne part que lorsque tous les joueurs connectés sont présents à l'arrêt. Un partenaire KO doit être porté pour compter comme présent.

## Comportement observable

- Un taxi greybox est visible à l'arrêt A au début de chaque manche.
- Chaque arrêt possède une zone d'embarquement accessible depuis le trottoir, sans ouvrir la chaussée comme raccourci piéton.
- À proximité de la borne : `E — APPELER LE TAXI` si le véhicule se trouve à l'autre arrêt, puis un retour lisible pendant son approche.
- Lorsque le taxi est présent : `E — PARTIR VERS LA VILLE` à A ou `E — RENTRER À LA COLOC` à B.
- L'appel exige le téléphone. Le départ exige aussi le portefeuille.
- Sans l'un des objets : feedback clair `TÉLÉPHONE OUBLIÉ` ou `PORTEFEUILLE OUBLIÉ`.
- Si tous les joueurs ne sont pas dans la zone : `EN ATTENTE DU PARTENAIRE`.
- Un joueur KO au sol ne compte pas comme prêt. S'il est porté par un joueur présent dans la zone, les deux comptent.
- Pendant le trajet, le taxi suit automatiquement la route réservée en environ 12 secondes. Les passagers ne peuvent ni se déplacer, ni regarder, ni interagir.
- Les joueurs voient le trajet depuis le taxi. À l'arrivée, leurs contrôles reviennent et un joueur porté reste porté.
- Aucun argent n'est retiré. Le portefeuille représente seulement la capacité à payer pour P1.

## Déplacement et autorité

L'hôte valide l'appel et le départ. Il décide de l'état du taxi, de l'arrêt de départ, de la destination et des temps serveur de départ/arrivée.

Utiliser un petit état répliqué, par exemple :

- `AtA`
- `AtB`
- `EmptyToA`
- `EmptyToB`
- `PassengersToA`
- `PassengersToB`

Le mouvement visuel est calculé localement à partir des mêmes temps serveur et du même chemin sérialisé. Ne pas écrire la position dans une `NetworkVariable` à chaque frame. Une synchronisation ponctuelle de l'état et des instants suffit.

Le trajet suit l'axe réservé de la carte, avec des virages adoucis. La vitesse peut varier légèrement pour les virages, mais la durée totale reste un paramètre Inspector. Commencer à 12 secondes.

Pendant un trajet avec passagers, chaque instance place les joueurs connectés sur des points de siège stables du taxi. Suspendre temporairement leur synchronisation de déplacement selon le modèle déjà utilisé par le portage. À l'arrivée, seul le propriétaire réactive et téléporte son `NetworkTransform` vers sa position de dépose.

Ne pas modifier les règles d'ivresse, de KO, de portage, de réveil, de retour ou de résultat de nuit.

## Arrêts et accès

- Conserver l'arrêt A près de `(0 ; 9)` et ajouter un passage piéton court vers la promenade.
- Conserver B au sud de la place et ajouter une zone d'embarquement reliée à la rue nocturne.
- Les passages donnent accès aux zones d'embarquement, pas à la chaussée entière.
- Chaque zone doit contenir deux joueurs et un porteur avec son partenaire, sans chevauchement avec les colliders du taxi.

La position exacte des bornes et des points de dépose peut être ajustée dans l'éditeur pour la lisibilité. Documenter l'écart sans déplacer les axes principaux de la ville.

## Appel du véhicule

Si le taxi est à l'autre arrêt, une demande valide lance un trajet à vide vers l'arrêt du demandeur. Aucun passager ne se téléporte pendant cette approche.

Une seconde demande pendant un trajet ne crée pas un deuxième taxi et ne change pas sa destination. À l'arrivée à vide, les joueurs doivent encore demander le départ lorsqu'ils sont tous prêts.

Le téléphone et le portefeuille sont des états collectifs : le joueur qui interagit n'a pas besoin d'être celui qui les a ramassés.

## Validation par l'hôte

Rejeter au minimum :

- mauvais émetteur ou mauvaise borne ;
- joueur hors portée ;
- manche terminée ;
- téléphone absent pour l'appel ;
- portefeuille absent pour le départ ;
- taxi déjà en trajet ;
- moins de deux joueurs connectés ;
- joueur connecté absent de la zone ;
- joueur KO non porté ;
- demandeur KO, porté ou portant un partenaire situé hors de la zone.

Une demande concurrente ne déclenche qu'un seul appel ou départ.

## Cycle de manche et réseau

- À la relance, le taxi revient à A, les trajets en cours sont annulés et aucun joueur ne reste attaché.
- Une déconnexion pendant le trajet conserve la règle P1-040 : échec immédiat. Le passager restant ne doit pas rester bloqué dans l'état taxi au bilan ou à la relance.
- Une expiration pendant le trajet produit le résultat normal de la nuit. Le trajet peut être figé au bilan ; la relance restaure impérativement le taxi et les joueurs.
- Le démarrage d'un taxi ne modifie jamais le chronomètre.
- Un client reçoit l'état courant du taxi sans RPC de rattrapage.

## Interface

Réutiliser le style du HUD de debug existant : invite centrale contextuelle, retour court pour un refus et petit état `TAXI · EN ROUTE` avec destination et temps restant pendant le trajet.

Ne pas ajouter de menu, carte, compteur d'argent ou inventaire général.

## Assets et périmètre

Construire le taxi avec des primitives identifiables et une couleur dédiée. Ne pas importer encore le pack Quaternius. La validation porte sur le trajet, la lisibilité et le réseau ; le remplacement visuel viendra après.

Aucun trafic, chauffeur autonome, physique de véhicule, porte animée, siège sélectionnable, collision routière, prix variable ou offre de substance dans cette fiche.

## Tests et QA

1. Hôte et client prennent téléphone et portefeuille, rejoignent A, partent ensemble et arrivent à B après la durée configurée.
2. Retour B → A avec les mêmes règles.
3. Le taxi appelé depuis l'arrêt opposé arrive à vide ; aucun joueur n'est déplacé avant le départ collectif.
4. Téléphone absent, portefeuille absent, partenaire absent et partenaire KO non porté : refus approprié, aucun trajet parasite.
5. Partenaire KO porté : départ, trajet, arrivée et portage toujours cohérent sur les deux instances.
6. Appels/départs concurrents : une seule transition.
7. Pendant le trajet : déplacement, regard et interactions bloqués sur chaque propriétaire.
8. Déconnexion et expiration pendant le trajet : résultat correct, aucun contrôle durablement bloqué.
9. Relance pendant le bilan : taxi à A, joueurs à la coloc, contrôles restaurés, objets domestiques réinitialisés.
10. Au moins un aller-retour avec de vraies entrées pour juger caméra, durée et compréhension des deux arrêts. Les injections peuvent couvrir les rejets et courses réseau.

Tests EditMode pour les règles pures de validation et les transitions d'état. Compilation sans erreur, console sans nouvelle exception et preuve hôte/client.

## Critères de fin

- Un unique taxi collectif effectue un aller-retour visible dans la scène continue.
- L'hôte décide de chaque appel et départ ; tous les clients voient le même état et la même progression.
- Téléphone, portefeuille, présence du groupe et partenaire KO porté sont correctement pris en compte.
- Le trajet ne produit pas de flux réseau par frame et ne casse pas portage, fin de nuit ou relance.
- Dyllan valide le feeling d'un aller-retour réel.

Créer `Docs/Reports/P1-055-collective-taxi-report.md`. Ne pas commit ni push après l'implémentation.
