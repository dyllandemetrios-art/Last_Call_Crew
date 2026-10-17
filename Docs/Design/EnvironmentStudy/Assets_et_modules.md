# Assets disponibles et modules proposés

Inspection du 14 octobre 2026, en lecture seule sur `D:/Assets/LastCallCrew`. **10 ZIP ouverts, listes de fichiers et licences lues, 500 OBJ mesurés**, aperçu de six packs relu visuellement. Aucun import Unity, achat ou extraction dans le jeu. Les deux anciennes captures P1-010 ont été repérées mais ne servent pas de preuve sur la scène actuelle.

**Précision de Dyllan, révision 2 :** la disponibilité des assets ne conditionne pas la construction. Réutiliser ce qui convient et garder des placeholders greybox pour tout manque. Une capsule peut représenter un PNJ, un cube une offre et un volume un futur lieu : l'interaction et la circulation sont prioritaires. Les manques ci-dessous concernent uniquement les archives inspectées ; ils ne prouvent pas l'absence de ressources ailleurs et ne déclenchent aucune recherche ou acquisition obligatoire.

![Aperçus fournis dans les archives](preuves/apercus_assets.png)

Preuves détaillées : [inventaire et empreintes SHA-256](preuves/inventaire_archives.json), [textes des licences locales](preuves/licences_locales.txt), [dimensions brutes OBJ](preuves/dimensions_obj.json). Les aperçus ci-dessus sont ceux des archives, pas des rendus testés dans l'URP du jeu.

## Contenu réellement disponible

| Archive | Contenu constaté | Emploi proposé / limite |
|---|---|---|
| `kenney_city-kit-roads.zip` | City Kit Roads **2.2** ; 95 modèles, chacun FBX/GLB/OBJ, palettes PNG, aperçus. Droites, demi-droites, T, croix, virages, sorties, lampadaires, barrières, benne, poteaux et panneaux. | Habillage de la route taxi, repères et limites. Les profils de quelques raccords concordent dans les OBJ ; les largeurs de trottoir et l'échelle ne correspondent pas directement à la greybox. Voir mesures ci-dessous. |
| `kenney_city-kit-suburban_20.zip` | Suburban **2.0** ; 40 modèles FBX/GLB/OBJ, dont 21 bâtiments `building-type-a` à `u`, clôtures, chemins, arbres et jardinière. | Maisons fermées, jardin et arrière-plan résidentiel. Aspect pavillonnaire ; pas un kit de façades mitoyennes françaises ni une preuve d'intérieurs visitables. La coloc jouable conserve sa coque greybox. |
| `kenney_furniture-kit.zip` | Furniture **2.0** ; 140 modèles, FBX/GLB/OBJ/DAE/STL. `kitchenBar`, `kitchenBarEnd`, `stoolBar`, `table`, `tableRound`, `chair`, `loungeSofa`, `sideTable`, `lampWall`, `speaker`, murs et embrasures. | Coloc, comptoir et quelques tables. Plusieurs pivots sont en coin, échelle différente des routes. Les embrasures décoratives ne fixent jamais la largeur jouable. |
| `kenney_food-kit.zip` | Food **2.0** ; 200 modèles FBX/GLB/OBJ. `glass`, `mug`, `cocktail`, `wine-red`, `wine-white`, `soda-bottle`, `barrel`, nourriture et vaisselle. | Boisson P1, décoration du comptoir. Nourriture utilisable plus tard aux stands ; aucune mécanique de consommation supposée. |
| `kenney_blocky-characters_20.zip` | 18 personnages FBX/GLB/OBJ. GLB `character-a` inspecté : 6 meshes, 0 skin, 27 animations de nœuds dont marche, attente, interaction, conduite et mort. | Candidat pour Marcel et silhouettes ultérieures. Ce n'est pas la preuve d'un rig Humanoid compatible avec Quaternius ; ne pas remplacer le prefab joueur réseau à cette étape. |
| `kenney_impact-sounds.zip` | 130 OGG, dont `footstep_concrete_000`…`004`, bois, herbe et impacts verre. | Pas et petits impacts candidats. Non auditionnés ici ; pas de preuve d'ambiance de foule ou de circulation complète. |
| `kenney_interface-sounds.zip` | 100 OGG. | Feedback d'interaction ultérieur, sans remplacer les signaux spatiaux. Mix et niveau sonore non évalués. |
| `kenney_input-prompts_1.5.zip` | Input Prompts **1.5A** ; 3056 PNG, 1504 SVG, polices et tables. `Keyboard & Mouse/Default/keyboard_e.png`, `keyboard_f.png`, `keyboard_escape.png`, `mouse_right.png` présents. | Rappel des touches si demandé ; pas un pack de signalétique urbaine française ou d'icônes métier complètes. Aucun changement de HUD dans la fondation. |
| `Realistic Car Pack - Nov 2018-20261013T152945Z-1-001.zip` | Quaternius ; **7 modèles** FBX/OBJ/BLEND, dont `Taxi`, `SUV`, `NormalCar1`, `NormalCar2`, `Cop`, `SportsCar`, `SportsCar2`. | Taxi disponible, malgré le nom « Realistic » : aperçu low-poly compatible en intention. Le taxi jaune et la voiture de police évoquent un autre contexte national ; recoloration et signal TAXI français à prévoir. Conduite, sièges, ouverture des portes et réseau non fournis par cette preuve. |
| `Universal Animation Library[Standard].zip` | Quaternius ; deux FBX sous `Unity/`, deux GLB sous `Unreal-Godot/`. Les GLB contiennent chacun 43 clips sur un squelette skinné. Le README distingue version sans root motion et `_RM` avec root motion. | Marche, attente, dialogue, conduite et mort utiles plus tard. Pas de clip explicitement nommé portage/réveil dans les GLB inspectés. Aucun test de retargeting sur Kenney ; la liste GLB ne valide pas le découpage/import des clips FBX dans Unity. |

Les dix archives possèdent chacune un `License.txt` déclarant **CC0**. Les huit Kenney autorisent explicitement les usages personnels, éducatifs et commerciaux et rendent le crédit facultatif ; les deux Quaternius déclarent CC0 1.0 Universal. Conserver ces textes avec les futurs imports et créditer les auteurs par choix éditorial. Ce constat porte sur les copies locales inspectées ; il ne rend pas libres les plans touristiques ou photographies de référence.

## Cohérence et raccords : ce qui est démontré

Direction proposée : formes simples, aplats peu saturés, petit nombre de couleurs d'orientation, éclairage nocturne lisible. Kenney forme une base cohérente dans les aperçus. Le véhicule Quaternius possède davantage de facettes et plusieurs matériaux ; il faudra harmoniser sa palette. Les maisons suburbaines évoquent surtout des pavillons : les aligner seules ne produira pas un centre ancien français crédible.

| Échantillon OBJ | Dimensions brutes X × Y × Z | Conséquence |
|---|---|---|
| `road-straight` | 1 × 0,02 × 1 | Géométrie centrée en XZ, base à y=0. Unité brute ≠ taille routière finale. |
| `road-straight-half` | 0,5 × 0,02 × 1 | Demi-longueur sur X. Il faut respecter l'orientation du raccord. |
| `road-intersection`, `road-crossroad` | ≈ 1 × 0,02 × 1 | Profils de bord à X = ±0,5 identiques à ceux des droites inspectées ; possibilité de raccord, pas une validation de tous les virages. |
| `building-type-a` | 1,30 × 0,834 × 1,028 | Un agrandissement est nécessaire ; mesurer porte, étage et parcelle séparément. |
| `building-type-u` | 1,428 × 1,138 × 1,087 | Variantes de hauteur réelles, mais pas des modules interchangeables de même silhouette. |
| `chair` | 0,20 × 0,47 × 0,20 | Pivot en coin (X négatif, Z positif), pas au centre de la chaise. |
| `table` | 0,841 × 0,327 × 0,447 | Recalibrer selon hauteur de table cible ; le facteur du bâtiment ne convient pas automatiquement. |
| `Taxi` | 1,807 × 1,310 × 4,221 | Proportions déjà proches d'une voiture ; cabine/sièges et position des roues à vérifier dans Unity. |
| `character-a` | 1,60 × 2,70 × 0,80 | Hauteur supérieure à la capsule actuelle de 2 m : redimensionnement visuel et vérification des épaules nécessaires. |

**Routes : incompatibilité à résoudre, pas à masquer.** Dans la droite OBJ, la surface entre les bordures s'étend de Z = −0,4 à +0,4, les bandes latérales jusqu'à ±0,5. À l'échelle ×10 : dalle de 10 m, surface centrale de 8 m et bandes de 1 m ; ce n'est pas le profil choisi de **6 + 3 + 3 = 12 m**. Piste réversible pour l'habillage : échelle uniforme ×7,5, surface centrale de 6 m, puis ajout de 2,25 m de trottoir de chaque côté des bandes existantes de 0,75 m. Quatre dalles couvrent 30 m. Les segments résiduels et raccords de carrefours exigent des pièces d'adaptation ; aucun collage automatique n'est promis. La greybox conserve ses primitives tant que cet essai n'est pas validé.

La dalle brute a deux niveaux visibles y=0,01 et y=0,02. Ces reliefs deviennent purement visuels pour P1 : la collision du trajet reste plate. Les bornes mesurées sont celles des OBJ, et **ne garantissent pas les unités/pivots du FBX importé**. Un futur essai d'import isolé devra comparer une capsule de 2 m, une porte de 3 m, une table et une dalle.

Les archives ne contiennent pas de `.prefab`, `.mat`, `.unity` ou contrôleur Unity prêt à brancher. Les images et matériaux source ne prouvent donc ni une conversion URP réussie, ni les colliders, ni les performances. Choisir les FBX pour l'essai initial évite d'introduire une dépendance GLB ; garder des matériaux URP simples partagés. Sur le taxi, le MTL nomme six matériaux (`Black`, `Grey`, `Headlights`, `TailLights`, `Windows`, `Yellow`) ; les phares ne sont pas une lumière Unity et l'émission n'est pas configurée par ce nom.

## Correspondance zones → assets → manques

| Zone | Utilisable après essai | Manque concret / traitement P1 |
|---|---|---|
| Coloc | Furniture : canapé, table, meuble, lampe ; Suburban pour des voisins fermés. | Maison accessible à échelle FPS : conserver la coque 10 × 10 et les objets de gameplay existants. |
| Résidentiel | Bâtiments a–u, clôtures, arbres, chemins. | Adresse française lisible, seuils/portails cohérents : panneaux simples et primitives. |
| Liaison | Routes : lampadaires, benne, barrières ; Suburban : végétation. | Façades mitoyennes/ateliers, mobilier public type banc urbain, ambiance distante : volumes sobres, audio différé. |
| Route et taxi | Droites/virages candidats, Taxi.fbx. | Arrêts, lumineux français, adaptation trottoir, comportement de transport et caméra : réservations greybox maintenant. |
| Bars compacts et place | Furniture : `kitchenBar`, tables, tabourets ; Food : `mug`, `glass`, `cocktail`, `glass-wine`, vins, soda, canettes, citron et orange. | Diviser l'ancienne emprise 17 × 16 en B1 Bières et B2 Shooters de 8,5 × 16 ; B3 Cocktails fait 10 × 12. Tireuses, verres à shot et shaker : placeholders jusqu'à confirmation d'un modèle dédié. |
| Ruelles / réserves | Benne, barrières, caisse et quelques accessoires. | Véritable stand, façade de boîte, échoppe/vendeur et vitrine : volumes fermés, aucune construction détaillée pour P1. |
| Repères et limite | Lampadaires et clôtures candidats. | Repère vertical unique, garde-corps de canal, portes fictives fermées : primitives. Aucun shader d'eau requis. |

## Contrat minimal de modularité

1. **Parcelles.** Trame de 10 m pour dimensionner les lots, sous-grille de 0,5 à 1 m pour l'implantation, les circulations et portes. Habitation 20 × 20 m contenant la pièce de 10 × 10 ; deux cellules de bar de 8,5 × 16 dans l'ancienne coque, bar futur de 10 × 12 ; gros lieu futur 20 × 20 ou réunion de parcelles. Les petits rectangles X3/X4 sont des emprises utiles à l'intérieur de lots réservés, pas des standards de module. Les objets existants sont autorisés à déroger à la grille ; on ne les déforme pas pour les aligner.
2. **Raccords.** Chaque module possède deux repères vides nommés `Entry` / `Exit` ou N/E/S/W : centre du passage, même hauteur y=0, orientation opposée et largeur explicite. Types simples : route 12 m (6+3+3), promenade 6 m, rue nocturne 8 m, ruelle 4 m. Une transition de largeur est une pièce dédiée, pas deux bords qui se chevauchent.
3. **Longueurs.** Segments simples de 10 ou 20 m en greybox ; pièces terminales ajustées aux coordonnées du plan. Pour l'habillage routier à ×7,5, blocs visuels de 30 m avec pièces d'adaptation. Les contrats de passage priment sur les dimensions du pack.
4. **Entrées.** Point de porte sur le bord de parcelle et aire libre de 3 × 3 m de chaque côté ; largeur utile 3 m, hauteur utile au moins 2,6 m. Chemin principal intérieur ≥3 m. La porte doit ouvrir sur un chemin, jamais sur une autre façade ou un volume de spawn.
5. **Collisions.** Colliders simples correspondant aux murs visibles ; pas un MeshCollider non convexe comme seule barrière : le contrôleur actuel l'ignore. Regrouper les petites décorations sans collision, réduire les obstacles au passage. Sa requête locale utilise un tampon de 16 colliders : éviter les coins encombrés et les clôtures composées de dizaines de colliders superposés.
6. **Limites, mise à jour du 17 octobre.** Respecter la ville ouverte validée dans le projet : collision seulement sur les bâtiments, clôtures et limites visibles, pas sur tout un îlot abstrait. Le canal reste derrière un garde-corps. Fermer localement le fond et les côtés des impasses et les entrées des lieux non visitables ; garder les autres espaces libres explorables.
7. **Préparer le futur sans générateur.** Un parent statique par morceau géographique et des repères de raccord suffisent. Garder les systèmes réseau existants distincts du décor ; ne pas ajouter un NetworkObject à chaque banc. Les futures permutations devront préserver au moins deux chemins piétons, les largeurs et les distances de retour. Aucune graine, algorithme ou nouveau framework maintenant.

Le premier essai d'habillage, après validation de la circulation, pourra se limiter à **une dalle, une façade fermée, un meuble et un taxi**. Rien n'a été importé pour cette étude.
