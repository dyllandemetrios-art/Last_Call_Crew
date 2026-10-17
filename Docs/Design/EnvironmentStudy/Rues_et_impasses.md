# Révision 3 — rues et impasses des Deux-Lanternes

17 octobre 2026. Demande de Dyllan : ajouter des ruelles et impasses, nommer **rue de la Soif** la rue entre le bar P1, le bar futur et la boîte future ; évoquer les substances prévues dans les autres noms. Étude uniquement : aucune modification du checkout Unity.

![Plan révisé](Carte_P1.png)

## Noms et disposition proposés

| Code du plan | Nom | Tracé / fonction |
|---|---|---|
| S | **Rue de la Soif** | Nouvelle rue de 8 m : longe les bars à bières et à shooters au sud, puis le bar à cocktails B3 et la boîte X1 au nord. Elle rejoint la boucle nord. Nom demandé par Dyllan. |
| P | **Ruelle de la Poudre de Perlimpinpin** | Nom du raccourci existant de 4 m, avec les poches R1–R3 conservées. Les offres restent accessibles sans bloquer le passage. |
| M | **Passage des Champignons de Mémé** | Nouvelle ruelle de 4 m reliant le bord ouest de la place à la rue de la Soif, le long du bar P1. Elle offre une deuxième approche du quartier. |
| B | **Ruelle des Bonbons Câlins** | Nom de la boucle nord existante autour de X3, prolongée à l'ouest jusqu'à la rue de la Soif et la boîte. |
| D | **Impasse de la Fumée de Dragon** | Bras de 16 m partant de la rue de la Soif vers l'ouest, sous la boîte. Fond en cour de 6 × 6 m, adapté à un PNJ ou un rendez-vous futur. |
| T | **Impasse de la Tisane de Morphée** | Bras de 28 m en coude depuis l'est de la boucle nord. Cour de 6 × 6 m à l'extrémité ; le fond se découvre après le virage. |

**Statut des substances :** ces cinq noms fictifs viennent du GDD de travail, section « Substances et hallucinations », conservée dans `D:\AI-GameWorkflow\outputs\Last-Call-Crew-Asset-List.md`. La première recherche était trop étroitement limitée au dépôt Unity et les avait manqués. L'affectation d'un nom à une voie reste réversible ; les noms des substances, eux, sont repris tels quels. Le Nocton trouvé dans P1-057 reste la monnaie, pas une substance.

## Coordonnées de dessin et coût de détour

Coordonnées X/Z, mêmes repères que les cartes précédentes. Largeurs utiles : Soif 8 m, ruelles et impasses 4 m. Les distances des impasses vont de leur embranchement au centre de la cour terminale.

| Voie | Axe | Longueur |
|---|---|---:|
| Rue de la Soif | (−110,56) → (−146,56) → (−146,86) | 66 m |
| Passage des Champignons de Mémé | (−146,30) → (−146,56) | 26 m |
| Prolongement des Bonbons Câlins | (−146,86) → (−110,86), raccord à la boucle existante | 36 m supplémentaires |
| Impasse de la Fumée de Dragon | (−146,66) → (−162,66) | 16 m ; aller-retour 6,4 s seul / 10,7 s en portage constant |
| Impasse de la Tisane de Morphée | (−80,68) → (−64,68) → (−64,80) | 28 m ; aller-retour 11,2 s seul / 18,7 s en portage constant |

Calculs à 5 m/s et 3 m/s, valeurs relues sur disque le 17 octobre. Hors conversation, attente, manœuvre et hésitation. Le chronomètre reste à 300 s. Après recentrage de l'entrée sur B1, les parcours de référence promenade/ruelle font 220,75/180,75 m sur leurs axes ; ce ne sont pas des minima imposés dans la ville ouverte. Les nouvelles connexions et diagonales libres demanderont une mesure réelle.

La boucle des Bonbons Câlins conserve ses branches x=−110 et x=−80 entre z=60 et z=86, et son raccord au carrefour vers x=−70. Le prolongement arrive **au nord de B3**, sans traverser son bâtiment. La rue de la Soif passe entre X1 et le nouveau B3 proposé à x=−135…−125/z=64…76. Cette emprise réduit l'ancien X2 de 18 × 14 m relevé dans P1-058 ; elle devra être confrontée à la scène avant déplacement. Les ouvertures de façade dessinées restent des portes fermées pour les lieux futurs.

## Lisibilité et ouverture du monde

- Plaque de rue à chaque embranchement ; pour les impasses, ajouter un pictogramme de voie sans issue visible à l'entrée. Le détour est un choix d'exploration, pas une fausse flèche de retour.
- La rue de la Soif se reconnaît à ses trois petits bars et à la boîte. Garder l'entrée principale du bar à bières P1 sur la place et sa sortie est existante ; B2 et B3 restent fermés.
- Les deux impasses ont un fond et des côtés réellement fermés par façades, murets ou clôtures visibles. Une cour ouverte sur le côté serait une traversée, pas une impasse. Les trois côtés de chaque cour terminale sont matérialisés sur le plan ; leur quatrième côté débouche dans la ruelle.
- Conserver la règle de ville ouverte validée dans P1-056a et la fondation du jeu : les espaces libres restent traversables hors obstacles visibles. Aucun grand bloc abstrait n'enferme les rues pour faire respecter les distances théoriques.
- R1–R3 restent ouverts sur le raccourci. Les nouvelles impasses ajoutent deux destinations potentielles ; elles n'imposent ni PNJ, ni mission, ni offre dans cette révision.
- Des plaques, volumes simples et capsules suffisent pour le premier essai. Les bâtiments existants qui gênent un nouveau passage seront déplacés localement par l'auteur du checkout après revue ; ne pas effacer l'habillage déjà construit.

## Relais ciblé pour le chat principal / Claude

Le projet a avancé depuis la première étude : P1-054 à P1-058 décrivent la construction, le taxi, l'ouverture du monde et la couverture de toute l'emprise. **Ne pas rejouer l'ancienne fiche de translation du bar ni reconstruire la carte depuis zéro.** Cette révision est une proposition d'ajouts aux positions actuelles, à confronter aux colliders existants dans l'éditeur.

Première étape utile : ouvrir les trois liaisons nouvelles (Soif, passage M, prolongement B), poser les deux impasses et les plaques, puis marcher dans chaque nouvelle branche avec un partenaire porté. Contrôler le passage de 4 m, les fonds de cour, la lecture des noms depuis la caméra, les accès à B1 et au taxi, et l'absence d'obstacle sur son trajet. Vérifier que les poches R1–R3 restent accessibles. Mesurer les détours et tester la lecture d'une impasse avant de s'y engager. Aucun nouveau gameplay, import obligatoire ou changement de timer.

Contexte observé : branche `feature/p1-001-network-foundation`, HEAD `1b953f7a1115f36793ad0699859c03425cb7e017`, checkout occupé par des changements P1-056c/P1-057/P1-058, dont scène, scripts, matériaux et documentation. Préserver ces changements. Les rapports locaux servent de contexte ; leurs PASS ne valident pas ces nouvelles rues. Dans cette étude : plan PNG/SVG relu, longueurs calculées ; aucun test Unity ou feeling observé.
