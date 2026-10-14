# P1-050 Maison simple

## Intention

Remplacer la zone verte abstraite par une maison greybox qui donne un sens concret au départ et au retour de la nuit, sans ajouter encore les clés, le portefeuille, le téléphone ou le taxi.

## Hypothèse de niveau

Construire la maison autour de l'actuelle `ReturnZone`, centrée vers `(20 ; 0 ; 16)`, afin de préserver le chemin validé depuis la sortie est du bar. La porte principale fait face au chemin existant. Les anciens points de départ à proximité de `(0 ; 1 ; 0)` ne sont plus utilisés pendant une manche normale.

Cette implantation est réversible et ne remplace aucun asset artistique définitif.

## Comportement observable

- Les deux joueurs commencent chaque manche à l'intérieur de la maison, sans chevauchement.
- Une porte large et un chemin lisible conduisent vers le bar.
- Au début, la maison affiche l'état `QUITTEZ LA MAISON` et ne peut pas déclencher de victoire.
- L'hôte mémorise séparément que chaque joueur connecté est sorti au moins une fois.
- Lorsque tous les joueurs de la manche sont sortis au moins une fois, le retour devient actif une seule fois et le HUD affiche brièvement `RETOUR ACTIVÉ`.
- Ensuite, revenir ensemble dans la maison avant zéro déclenche la victoire selon les règles existantes.
- Un partenaire KO porté dans la maison compte toujours comme présent avec son porteur.
- Une relance replace les joueurs dans la maison et désactive à nouveau le retour jusqu'à leur prochaine sortie.

## Maison greybox

Créer dans `BarGreybox.unity` une maison simple et immédiatement reconnaissable :

- sol intérieur distinct de l'extérieur ;
- quatre murs et un plafond ou toit simple qui ne gêne pas la caméra ;
- porte principale d'au moins 2,5 m, compatible avec le portage ;
- volume intérieur assez large pour deux joueurs, un joueur porté et les futurs objets domestiques ;
- éclairage et couleurs de greybox distincts, sans asset tiers obligatoire ;
- ancien portique/repère vert réutilisé comme signal de maison ou retiré s'il devient redondant ;
- trigger de retour séparé des colliders solides et couvrant uniquement l'intérieur utile.

Éviter les couloirs, escaliers, portes animées et pièces supplémentaires. Une pièce unique suffit.

## Règle d'activation

Extraire une règle pure minimale qui suit, pour chaque joueur de la manche, s'il a déjà quitté le volume de la maison. L'hôte seul écrit l'état d'activation partagé.

- Tant que le retour n'est pas activé, aucun joueur n'est marqué rentré et `NightOutcome` ne peut pas conclure à une victoire.
- Un joueur peut sortir puis revenir avant son partenaire : sa sortie reste mémorisée, mais l'activation attend toujours que chaque joueur soit sorti au moins une fois.
- L'activation est irréversible pendant la manche.
- La déconnexion reste régie par P1-040 ; ne pas créer une seconde politique de roster.
- À la relance, vider les sorties mémorisées et l'état de retour avant de replacer les joueurs.

Ne pas envoyer de RPC client pour déclarer une sortie : l'hôte utilise les positions et le volume existant.

## Points de départ

Adapter l'attribution par emplacement de P1-040 pour fournir deux poses situées dans la maison. Ne pas réintroduire de calcul fondé directement sur le `ClientId`. Les positions restent déterministes après reconnexion et relance.

## Interface

Réutiliser le HUD existant :

- avant activation : `MAISON · QUITTEZ LES LIEUX` ;
- à l'activation : feedback `RETOUR ACTIVÉ` pendant environ deux secondes ;
- après activation : statut actuel `ENCORE DEHORS` / `RENTRÉ` conservé.

Ne pas ajouter de tutoriel complet ni refaire l'interface finale.

## Tests automatiques

Ajouter des tests de logique pure pour :

- aucun joueur sorti ;
- un seul joueur sorti ;
- tous les joueurs sortis ;
- sortie puis retour anticipé d'un joueur ;
- activation irréversible ;
- remise à zéro à la relance ;
- emplacements de départ dans la maison avec des `ClientId` élevés/non consécutifs.

## Validation demandée

1. Hôte et client apparaissent dans la maison sans être marqués rentrés et sans victoire immédiate.
2. Un seul joueur sort puis revient : retour toujours inactif.
3. Le second sort : activation unique et cohérente sur les deux instances.
4. Les deux reviennent : victoire normale.
5. Un joueur est mis KO dans le bar, porté jusqu'à la maison : victoire normale.
6. Relance : joueurs replacés dedans, retour de nouveau inactif, aucun état résiduel.
7. Reconnexion puis relance : deux emplacements corrects dans la maison.
8. Parcours clavier/souris réel : maison → bar → maison, sans collision bloquante ni étranglement.

## Critères d'acceptation

- Aucun départ ne produit de victoire immédiate.
- La maison est identifiable comme lieu de départ et de retour sans asset final.
- La porte et le trajet fonctionnent avec un joueur porté.
- L'activation est autoritaire, partagée et remise à zéro entre les manches.
- Toutes les règles P1 existantes restent fonctionnelles.
- Compilation sans erreur, tests pertinents réussis et consoles sans nouvelle erreur bloquante.

## Hors périmètre

Clés, portefeuille, téléphone, taxi, porte verrouillée, inventaire, interaction avec des meubles, plusieurs pièces, sauvegarde, décoration finale, cycle jour/nuit visuel et chargement d'une scène séparée.

## Compte rendu attendu

Créer `Docs/Reports/P1-050-simple-home-report.md` avec : commit de départ, implantation et dimensions, objets/scène modifiés, règle d'activation, points de départ, autorité réseau, tests, parcours avec et sans portage, captures minimales, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
