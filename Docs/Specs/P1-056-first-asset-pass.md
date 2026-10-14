# P1-056 — Première passe d'assets

## Intention

Remplacer les volumes visuels les plus évidents de la ville validée par les assets gratuits déjà téléchargés. La scène doit commencer à ressembler à un quartier habité tout en conservant exactement les chemins, colliders simples et comportements réseau prouvés en P1-054 et P1-055.

## Packs concernés

Sources locales : `D:/Assets/LastCallCrew`.

Importer uniquement les éléments utiles de ces trois archives :

1. `Realistic Car Pack - Nov 2018-20261013T152945Z-1-001.zip`
   - `Realistic Car Pack - Nov 2018/FBX/Taxi.fbx`
   - `Realistic Car Pack - Nov 2018/License.txt`
2. `kenney_city-kit-roads.zip`
   - FBX nécessaires à la route existante : droites, virages et pièces de raccord réellement utilisées ; lampadaires ou panneaux seulement s'ils améliorent un repère déjà prévu ;
   - `Models/FBX format/Textures/colormap.png` ;
   - `License.txt`.
3. `kenney_city-kit-suburban_20.zip`
   - une petite sélection de bâtiments, clôtures, arbres et jardinières réellement placés dans le résidentiel ;
   - `Models/FBX format/Textures/colormap.png` ;
   - `License.txt`.

Ne pas extraire toutes les variantes ou tous les formats. FBX uniquement pour les modèles retenus. Ne pas importer le mobilier, la nourriture, les personnages, les sons, les animations ou les invites de touches dans cette fiche.

## Organisation Unity

Conserver les sources tierces sous :

- `Assets/ThirdParty/Quaternius/RealisticCarPack/`
- `Assets/ThirdParty/Kenney/CityKitRoads/`
- `Assets/ThirdParty/Kenney/CityKitSuburban/`

Chaque dossier contient la licence correspondante. Conserver les noms d'origine des modèles. Les matériaux adaptés au projet et les prefabs assemblés appartiennent à :

- `Assets/_Project/Materials/Environment/`
- `Assets/_Project/Prefabs/Environment/`

Ne déplacer aucun fichier après que Unity lui a attribué son `.meta`. Ne copier aucun ZIP dans `Assets/`.

## Taxi

- Remplacer les enfants visuels en primitives du `Vehicle` par un prefab utilisant `Taxi.fbx`.
- Conserver la racine `CollectiveTaxi`, son `NetworkObject`, ses scripts, son chemin, ses sièges et points de dépose.
- Le modèle importé n'a aucun rôle réseau et aucun collider de gameplay.
- Régler échelle, orientation et hauteur dans le prefab d'adaptation, sans modifier la trajectoire pour compenser un mauvais pivot.
- Harmoniser les matériaux avec l'URP et conserver une silhouette jaune avec un signe taxi lisible.
- Garder les primitives actuelles désactivées ou les supprimer seulement après comparaison visuelle et validation du modèle dans les deux directions.

## Route et quartier résidentiel

- Habiller la chaussée réservée avec les modules Kenney correspondant à ses segments et virages.
- Les modules sont décoratifs : les bordures, zones d'embarquement et colliders simples validés restent responsables de la circulation.
- Éviter z-fighting avec les bandes greybox. Masquer seulement les rendus remplacés, sans supprimer les volumes servant de limites ou de repères.
- Placer un nombre limité de maisons suburbaines dans les îlots résidentiels, avec quelques clôtures et arbres. Les entrées restent fermées et non interactives.
- Préserver la porte verte de la coloc, le carrefour bleu et les deux arrêts comme repères plus visibles que le décor.
- Ne pas employer les maisons suburbaines pour simuler le centre ancien ou remplacer le bar : leur rôle est le quartier résidentiel et l'arrière-plan.

## Rendu et import

- Vérifier les importateurs FBX, les normales, les matériaux et les textures dans Unity 6000.3.8f1 avec URP 17.3.0.
- Aucun matériau rose, référence manquante ou texture floue par erreur d'import.
- Désactiver `Read/Write` et l'import d'animations sur les modèles statiques lorsqu'ils ne sont pas nécessaires.
- Les bâtiments décoratifs n'ajoutent pas de `MeshCollider`. Réutiliser les `BoxCollider` de la greybox.
- Ne pas ajouter de post-traitement, cycle jour/nuit ou éclairage artistique général dans cette fiche.

## Comportement observable

- Le véhicule ressemble immédiatement à un taxi et suit le trajet P1-055 sans décalage visible entre ses roues et la route.
- La route A–B possède une continuité visuelle aux raccords et reste inaccessible à pied hors des quais.
- Depuis la coloc, le quartier résidentiel paraît construit, mais la porte de départ et l'arrêt A restent faciles à trouver.
- Depuis B, le joueur distingue la route, la place et la direction du bar.
- Les joueurs, le partenaire porté et la caméra ne traversent pas visuellement le toit ou les côtés du nouveau taxi.

## Validation

1. Compilation et console sans nouvelle erreur.
2. Aucun `Missing Script`, matériau rose ou référence d'asset cassée.
3. Hôte et client effectuent au moins un trajet injecté A → B : positions, portage, contrôles et arrivée identiques à P1-055.
4. Un appel à vide vérifie que seul le modèle se déplace et qu'aucun joueur n'est embarqué.
5. Vérifier les quais et la chaussée avec la capsule de collision : aucun nouveau raccourci ou blocage.
6. Vérifier la scène à 960×540 depuis la caméra joueur : taxi identifiable, repères conservés et bâtiments sans obstruction majeure.
7. Conserver au maximum trois captures utiles : taxi en vue joueur, résidentiel depuis la coloc et raccord routier problématique s'il existe.
8. Dyllan valide ensuite l'échelle et la cohérence visuelle. Il n'a pas à rejouer les règles de taxi déjà validées si aucune régression n'est observée.

Ne relancer l'ensemble des tests EditMode que si du code ou un asset référencé par des tests change. Une modification de scène et d'import exige surtout compilation, inspection des références et trajet hôte/client ciblé.

## Critères de fin

- Les trois sources tierces sont rangées avec leurs licences et uniquement les fichiers utilisés.
- Le taxi Quaternius remplace visuellement la voiture en primitives sans changer P1-055.
- La route et le résidentiel utilisent des modules Kenney en conservant les collisions greybox.
- Aucun nouvel élément de gameplay n'est ajouté.
- Rapport : `Docs/Reports/P1-056-first-asset-pass-report.md`.

Ne pas commit ni push après l'implémentation.
