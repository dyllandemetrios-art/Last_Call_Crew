# Fondation environnementale P1 — Les Deux-Lanternes

Décision validée le 14 octobre 2026. La carte P1 est fixe et tient dans la même scène continue que la coloc. Sa construction emploie des morceaux de rues et des parcelles raccordables afin de préparer une recomposition procédurale après P1, sans développer de générateur maintenant.

![Carte P1](img/P1-city-map.png)

## Organisation retenue

- La coloc et la zone de retour restent dans le quartier résidentiel.
- Une promenade lisible conduit au quartier nocturne et au bar P1.
- Une ruelle plus courte offre trois poches de 6 × 6 m pour de futurs PNJ, offres et amorces de missions.
- Une route périphérique relie deux emplacements de taxi dans la même scène.
- Le taxi devra raccourcir le trajet en quelques secondes ; son appel, son paiement et son comportement réseau feront l'objet de fiches séparées.
- Le trajet à pied reste toujours possible et pourra recevoir des événements ultérieurement.
- Un seul bar est visitable pour P1. Les autres bars, la boîte de nuit, les vendeurs et les stands sont représentés par des parcelles fermées.

## Dimensions de travail

Convention : 1 unité Unity = 1 mètre. Le joueur se déplace actuellement à 5 m/s et à 3 m/s lorsqu'il porte continuellement un partenaire.

| Parcours | Distance indicative | Seul | Portage constant théorique |
| --- | ---: | ---: | ---: |
| Coloc → bar par la promenade | 225 m | 45 s | 75 s |
| Coloc → bar par la ruelle | 185 m | 37 s | 62 s |
| Sortie latérale du bar → coloc | 164,5 m | 33 s | 55 s |
| Trajet complet par taxi | 170 m de route | cible totale ≈ 23 s | à définir |

Ces valeurs servent à mesurer le premier blockout. Elles restent modifiables après le playtest et ne changent pas le chronomètre de 300 s.

## Repères et circulation

- Porte verte : coloc et retour.
- Carrefour bleu : repère principal vers le résidentiel.
- Enseigne ambre : bar P1.
- Promenade : 6 m de large.
- Rue nocturne : 8 m de large.
- Ruelle : passage central de 4 m, sans interaction forcée.
- Portes jouables : 3 m minimum.
- Route du taxi : chaussée de 6 m et trottoirs de 3 m.

Les îlots, jardins et façades ferment physiquement les diagonales. Les joueurs doivent suivre les passages prévus. Le sol jouable reste plat car le contrôleur actuel ne gère ni gravité, ni pente, ni marche.

## Assets disponibles

Les archives conservées dans `D:/Assets/LastCallCrew` ont été inspectées en lecture seule. Les packs Kenney fournissent routes, bâtiments suburbains, mobilier, nourriture, personnages, sons et invites de touches. Le pack Quaternius fournit notamment un taxi et une bibliothèque d'animations. Les licences locales des dix archives inspectées indiquent CC0.

La greybox reste prioritaire. Aucun pack n'est importé pendant le premier blockout. Les futurs imports devront conserver les licences et suivre `Assets/ThirdParty/<Auteur>/<Pack>/`. Les primitives restent acceptables lorsqu'un asset manque ou exige trop d'adaptation.

## Références de forme urbaine

La ville fictive combine trois principes observés : rue nocturne dense de Rennes, place centrale du Vieux-Tours et limite lisible par canal inspirée de Saint-Leu à Amiens. Ces références orientent la circulation et les repères ; aucun plan réel n'est reproduit.

Sources de l'étude :

- [Rue de la Soif](https://fr.wikipedia.org/wiki/Rue_de_la_Soif)
- [Destination Rennes — rue de Penhoët](https://www.tourisme-rennes.com/decouvrir-rennes/histoire/rue-penhoet/)
- [Tours — places et terrasses](https://www.tours-tourisme.fr/blog/faire-la-fete/top-5-des-spots-pour-boire-un-verre-en-terrasse-a-tours/)
- [Amiens — quartier Saint-Leu](https://www.amiens.fr/Je-suis/Touriste/Amiens-en-un-jour)

## Limite de cette décision

Cette carte est une base de test, pas un PASS gameplay. Le premier playtest doit décider si les trajets sont intéressants ou trop longs avant tout habillage, taxi fonctionnel ou génération procédurale.
