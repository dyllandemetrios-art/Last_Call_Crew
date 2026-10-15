# P1-056b — Recomposition visuelle de la ville P1

## Retour de Dyllan

La caméra extérieure du taxi et la liberté de déplacement de P1-056a vont dans la bonne direction, mais la ville visible ne correspond pas à la carte ni à l'intention. La scène ressemble encore à un grand terrain gris presque vide, avec des maisons éloignées et une route rectiligne. Ouvrir les anciens couloirs de collision ne suffit pas : il faut maintenant composer une petite ville ouverte, dense et lisible.

## Objectif observable

Depuis la coloc, le taxi et le bar, le joueur doit voir un quartier continu plutôt qu'un vide : rues encadrées, îlots, carrefours, petites places, végétation et repères. Il peut quitter les axes, traverser des lots ouverts et se perdre, mais chaque direction doit montrer un élément urbain proche ou un repère.

Cette passe est uniquement une recomposition visuelle et spatiale avec les assets déjà importés. Elle ne crée aucun nouveau gameplay.

## Travail demandé

- Réutiliser les maisons, routes, trottoirs, clôtures, arbres et mobilier déjà importés en P1-056. Ne pas réimporter les packs et ne pas ajouter de package.
- Rapprocher les bâtiments et les répartir des deux côtés des rues afin de former plusieurs îlots lisibles. Éviter une simple rangée de maisons au loin.
- Densifier particulièrement : la sortie de la coloc, l'arrêt A, le carrefour bleu, l'approche du quartier nocturne, l'arrêt B et l'espace autour du bar.
- Briser la grande perspective A–B avec au moins deux changements visuels : léger coude, carrefour, place, alignement d'arbres ou façade marquante. Le trajet réseau et ses points A/B ne changent pas.
- Remplacer l'impression de sol gris unique par des surfaces cohérentes : chaussée, trottoirs, pelouses, parcelles et petite place. Les surfaces restent plates pour le contrôleur actuel.
- Garder des passages libres entre les îlots et plusieurs raccourcis hors des rues. Monde ouvert ne signifie ni couloir, ni terrain vide.
- Encadrer le bar par des façades ou volumes urbains non visitables afin qu'il appartienne à un centre nocturne. Un seul bar reste visitable.
- Conserver des espaces réservés pour R1–R3 et les futurs stands, vendeurs et jeux, mais les intégrer visuellement au quartier.
- Les collisions doivent correspondre aux objets visibles. Aucun grand mur invisible et aucun nouveau bloc abstrait imposant un parcours.

## Conservation obligatoire

- Ne pas modifier les règles réseau, le chronomètre, le taxi, le portage, le KO, les retours, les objets de la maison ou les dialogues.
- Ne pas déplacer les points fonctionnels : poses de départ, `ReturnZone`, arrêts A/B, zones d'embarquement, points de dépose, `DrinkOffer`, Marcel et portes du bar.
- Si un habillage doit bouger autour d'un point, garder le point fonctionnel à sa position actuelle.
- Préserver tous les changements non commités P1-056 et P1-056a. Aucun reset, aucun commit et aucun push.
- Ne pas supprimer les primitives fonctionnelles désactivées du taxi.

## Méthode attendue

1. Lire Git status/diff, P1-054 à P1-056a et la carte `Docs/Design/img/P1-city-map.png` avant toute mutation.
2. Faire d'abord une proposition de composition en vue du dessus avec les emprises principales. Ne pas passer plusieurs heures à détailler une disposition manifestement vide.
3. Construire par secteurs : résidentiel, transition, centre nocturne. Vérifier chaque secteur depuis une caméra à hauteur de joueur.
4. Réutiliser des prefabs adaptés sous `Assets/_Project/Prefabs/Environment/` plutôt que multiplier des modèles bruts indépendants.
5. Garder une densité raisonnable pour P1 : diversité suffisante pour masquer le vide, sans chercher une ville finale.

## Validation

1. Compilation sans erreur, aucun `Missing Script`, prefab ou matériau manquant.
2. Les acquis P1-055 et P1-056a restent fonctionnels ; ne rejouer que les contrôles directement touchés par la scène.
3. Parcours à pied possible entre coloc, arrêt A, quartier nocturne, bar et arrêt B par plusieurs chemins, sans mur invisible.
4. Maisons, clôtures et murs visibles bloquent ; routes, trottoirs, pelouses, places et passages ouverts sont traversables.
5. Trois captures maximum, toutes utiles : vue du dessus, vue joueur depuis la coloc, vue joueur dans le centre nocturne. Une vue taxi peut remplacer l'une des vues joueur si elle révèle mieux la continuité urbaine.
6. Avant de déclarer PASS, comparer explicitement les captures à la carte et vérifier : densité, échelle, continuité, repères et absence de grand vide gris.
7. La validation finale du rendu et du sentiment de petite ville appartient à Dyllan.

## Critère de fin

P1-056b peut être présenté à Dyllan quand les captures montrent une petite ville continue et ouverte, pas seulement une scène techniquement traversable. Rapport attendu : `Docs/Reports/P1-056b-city-composition-pass-report.md`.
