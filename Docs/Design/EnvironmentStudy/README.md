# Rentrer Vivant — Les Deux-Lanternes

**Révision 4, 17 octobre 2026 — petits bars thématiques.** Voir [le découpage des bars et son premier test](Bars_compacts.md), puis [les rues et impasses](Rues_et_impasses.md). L'ancienne emprise de 17 × 16 m devient deux cellules de 8,5 × 16 m : bar à bières jouable en P1 et bar à shooters fermé. Un bar à cocktails futur de 10 × 12 m leur fait face. Les plaques des ruelles reprennent les cinq substances fictives du GDD. Étude uniquement ; aucun fichier du jeu ou des archives modifié ici.

![Carte P1](Carte_P1.png)

Version vectorielle : [Carte_P1.svg](Carte_P1.svg). [Assets et modularité](Assets_et_modules.md). [Première fiche pour Claude Code](Fondation_P1.md). [Contexte et preuves](preuves/Contexte_et_verifications.md).

## Proposition à retenir

Une petite périphérie résidentielle mène à un quartier de bars par une promenade bordée d'ateliers. Le centre s'organise autour d'une place et de trois petits bars thématiques : bières, shooters et cocktails. Seul le bar à bières est visitable en P1 ; les deux autres sont des façades fermées qui densifient déjà la rue de la Soif. Une ruelle permet de couper le détour ; une boucle secondaire prépare de futures rencontres. Un axe routier périphérique relie les deux arrêts du taxi sans traverser la place piétonne.

La ville est fixe dans P1, dans une scène continue. Les modules seront assemblés à la main. Les autres bars, stands, vendeurs et la boîte sont des **réserves de parcelles fermées**, pas du gameplay supplémentaire à produire avant novembre. Le taxi garde son rôle validé de gain de temps ; son comportement réseau reste une fiche distincte, et non un prérequis pour tester les distances à pied.

Le parti pris est une **compression de ville française**, pas la reproduction d'une ville réelle : fronts bâtis serrés au centre, petits jardins dans le résidentiel, enseignes lisibles, trottoirs et une limite de canal. Le canal peut rester une bande colorée derrière un garde-corps ; aucun système d'eau n'est nécessaire.

## Direction ajoutée par Dyllan — rencontres et substances

La ruelle courte doit offrir facilement des occasions de rencontre et de consommation de substances dans le jeu. Le taxi pourra également en proposer. À terme, davantage de lieux et de PNJ permettront davantage d'interactions et de missions. Les cinq noms fictifs sont fixés par le GDD ; leurs effets précis, leur coût et leur jalon d'implémentation ne le sont pas dans cette étude. Le premier incrément environnemental prépare leurs emplacements sans ajouter de règles de consommation.

**Révision spatiale concrète : trois poches R1–R3 de 6 × 6 m**, alternées sur les deux côtés des 40 m de ruelle courte. Elles s'ajoutent aux deux poches de la promenade. Le passage central conserve **4 m libres** ; façades et clôtures suivent les nouveaux renfoncements. L'îlot au nord est reculé pour R2. Chaque poche possède une ouverture d'au moins 3 m, de la place pour un PNJ et deux joueurs, et un dégagement pour déposer un partenaire. Les trois poches ne communiquent pas entre elles à travers l'îlot.

| Poche | Emprise X / Z | Usage futur proposé, réversible |
|---|---|---|
| R1 | x −104…−98 ; z 32…38 | Rencontre très visible depuis la sortie du bar ; offre directe. |
| R2 | x −94…−88 ; z 42…48 | Petit groupe en retrait ; conversation, offre ou amorce de mission. |
| R3 | x −82…−76 ; z 32…38 | Dernière rencontre avant la promenade ; offre ou rendez-vous. |

Les centres sont espacés d'environ 10–12 m le long de la ruelle : plusieurs occasions sont perceptibles en quelques secondes de marche. Hypothèse d'interaction : approcher un PNJ clairement visible, ouvrir une proposition simple, accepter ou refuser ; aucun détour complexe ni objet caché requis pour trouver une offre. La fréquence exacte des offres et leurs conséquences restent réglables et à tester. Les PNJ peuvent d'abord être des capsules identifiées ; aucune animation ou façade finale n'est requise pour les rendre jouables.

**Compromis recommandé :** rendre les offres faciles à trouver, tout en laissant traverser sans ouvrir un dialogue. Le raccourci devient un trajet court avec davantage de sollicitations ; le joueur peut s'y arrêter ou continuer. Le taxi doit garder une option de transport immédiatement lisible. Première hypothèse : proposition facultative à l'arrêt, avant le départ ou après l'arrivée ; le trajet n'est pas conditionné à une consommation.

Après recentrage de l'entrée sur B1, les axes deviennent 220,75 m / 180,75 m. Les arrêts de rencontre s'ajoutent au temps de marche : trois pauses de 5 s font passer le raccourci d'environ 36 à **51 s**. De même, les 23 s estimées du taxi deviennent `23 s + attente + durée de dialogue choisie`. Le roulage ne doit pas être artificiellement ralenti pour placer une offre. Aucun gel du chronomètre n'est proposé.

**Assets : jouabilité prioritaire confirmée.** Réutiliser un asset qui convient, conserver une primitive identifiable dès qu'il manque ou demande trop d'adaptation. Les manques de l'audit concernent le contenu des dix archives inspectées, pas l'ensemble des ressources disponibles ailleurs. Aucune recherche, acquisition ou finition artistique ne doit bloquer la greybox. Les futurs PNJ, lieux et missions seront ajoutés progressivement ; la carte réserve de l'espace, sans exiger un framework de missions maintenant.

## Trois références complémentaires

Point de départ effectivement consulté : [Rue de la Soif, Wikipédia](https://fr.wikipedia.org/wiki/Rue_de_la_Soif). La page repère notamment Rennes, Tours et Amiens ; elle avertit elle-même d'un manque de sources. Son ancien classement de bars n'est pas utilisé comme mesure actuelle.

| Quartier | Observations documentées | Transposition proposée pour le jeu |
|---|---|---|
| Rennes : Saint-Michel / Sainte-Anne / Penhoët | Saint-Michel concentre les lieux nocturnes près de Sainte-Anne. Penhoët forme un coude et offre un contournement vers les Lices. Le plan public montre la proximité de ces espaces et les axes plus larges en périphérie. | Un axe de bars immédiatement identifiable, puis une ruelle qui évite le détour. Un changement de direction masque le centre jusqu'à sa découverte. Le taxi s'arrête en bordure. |
| Vieux-Tours : Plumereau / Grand Marché / Châteauneuf | Les sources locales décrivent des places à terrasses, deux explicitement piétonnes, avec des monuments ou une sculpture comme repères. Une source de voyage décrit les départs de rues Grand Marché, Briçonnet, Monnaie et Commerce autour de Plumereau. | Une place permet de se retrouver, viser une enseigne et choisir une direction. Les connexions sont courtes ; aucun labyrinthe de cinq minutes. Une silhouette unique suffit comme repère. |
| Amiens : Saint-Leu / quai Bélu / place du Don | La ville décrit des canaux, ponts, ruelles et petites maisons colorées. Le tourisme régional confirme les bars/restaurants du quai Bélu. Une description morphologique secondaire situe l'axe Francs-Muriers–Vanmarcke–Parmentier et la liaison Don–pont de la Dodane. | Une bordure d'eau rend la limite mémorisable et permet un repère lointain. On conserve la séparation entre accès routier et cœur piéton. Les ponts, franchissements et chutes dans l'eau sont exclus de P1. |

Sources consultées le 14 octobre 2026 :

- **Rennes** : [Destination Rennes — rue de Penhoët](https://www.tourisme-rennes.com/decouvrir-rennes/histoire/rue-penhoet/) ; [terrasses, sections Sainte-Anne et Lices](https://www.tourisme-rennes.com/decouvrir-rennes/sortir/terrasses-rennes-bretagne/) ; [Guide découverte, plan p. 9](https://reservation.tourisme-rennes.com/medias/documents/documentation/documentation-ACCESSGUIDE-FR.pdf), téléchargé et relu visuellement. Plan touristique schématique, sans mesure de longueur ni preuve des accès automobiles actuels.
- **Tours** : [Office de tourisme — places et terrasses](https://www.tours-tourisme.fr/blog/faire-la-fete/top-5-des-spots-pour-boire-un-verre-en-terrasse-a-tours/) ; [Perspectives de voyage — Vieux-Tours](https://perspectives-de-voyage.com/visiter-tours/), source secondaire pour les rues. Le vieux PDF du Parcours Lumière renvoie 404 lors de la récupération : aucun plan de Tours n'a été relu visuellement. Accès taxis exacts non établis.
- **Amiens** : [Amiens Métropole — quartier Saint-Leu](https://www.amiens.fr/Je-suis/Touriste/Amiens-en-un-jour) ; [Hauts-de-France Tourisme — quai Bélu](https://www.tourisme-en-hautsdefrance.com/decouvrir/les-villes-des-hauts-de-france/amiens/) ; [Saint-Leu, section Morphologie](https://fr.wikipedia.org/wiki/Saint-Leu_(Amiens)), source secondaire. Le plan touristique 2018 trouvé est devenu inaccessible (404). L'OAP proposée par la recherche n'a pas été lue : fichier trop volumineux pour l'outil. Aucun raccourci réel non documenté n'est présumé public.

**Limite volontaire :** trois quartiers seulement, un plan capturé. Google Maps et OpenStreetMap n'ont pas été nécessaires. Les dimensions, noms fictifs, placements de taxi et raccourcis de la carte sont nos propositions ; les sources ne les prescrivent pas. Pas de copie des plans ou photographies dans les assets du jeu.

## Lieux et circulation

| Élément | Proposition P1 | Extension réservée |
|---|---|---|
| Coloc | Une pièce actuelle de 10 × 10 m ; clés, portefeuille, téléphone et trigger existants conservés. Porte ouest de 3 m, repère vert visible. | Autres pièces et housing hors étude. |
| Résidentiel | Quelques façades fermées et jardins clos ; on voit immédiatement la porte verte et l'arrêt A. | Aucune autre maison visitable. |
| Promenade des Ateliers | Passage de 6 m ; deux poches de 8 × 6 m hors du flux. Une façade industrielle basse accompagne la marche. | Rencontres/événements dans ces poches ; aucun système ajouté maintenant. |
| Rue nocturne et place | Rue de 8 m, place de 32 × 20 m. Trois indices : maison verte, carrefour bleu, enseigne ambre. Place gardée libre sur un axe de 6 m. | Terrasses supplémentaires latérales. |
| Bar P1 | B1, bar à bières de 8,5 × 16 m dans la moitié est de l'ancienne coque. Entrée sud recentrée, sortie est conservée, comptoir et Marcel à recomposer dans ce volume. | B2, bar à shooters mitoyen, fermé ; B3, bar à cocktails de 10 × 12 m, fermé. |
| Ruelles | Raccourci de 4 m avec trois poches ouvertes R1–R3 de 6 × 6 m ; boucle nord de 4 m. Coins élargis et aucune impasse présentée comme la route du retour. | Offres, conversations et missions dans R1–R3 ; X3 reste une réserve distincte de vendeurs au nord. |
| Taxi | Arrêt A près de la coloc, arrêt B au bord sud de la place ; route 6 m et deux trottoirs de 3 m. Réserver un dégagement latéral de 6 × 8 m à chaque arrêt. | Pas de circulation générale, économie ou chauffeur autonome dans cette fiche. |
| Réserves | X1 boîte, B2 shooters, B3 cocktails, X3 vendeurs, X4 stands : volumes fermés, sans interaction. | Des portes et branchements pourront remplacer ces façades après P1. |

Les fonds sombres du plan distinguent les secteurs ; ils ne définissent pas une collision. **La ville reste ouverte à pied hors bâtiments, clôtures et limites visibles**, selon la décision P1-056a reprise dans la fondation du jeu. Des diagonales sont permises et les distances sur axes ne sont pas des minima imposés. Seuls les fonds et côtés des véritables impasses doivent être fermés localement pour conserver leur fonction.

La promenade offre le parcours de découverte, la ruelle le retour appris. Le trottoir routier reste une troisième option, volontairement visible et continue. Aucun événement aléatoire ne bloque les seuls chemins de retour dans P1. Les enseignes des réserves restent éteintes ; seules la coloc et le bar invitent à entrer. En nuit, conserver un éclairage ambiant suffisant pour voir le partenaire KO et le sol.

## Dimensions et rythme

**Valeurs constatées dans les fichiers du jeu :** Unity 6000.3.8f1 ; `Player.prefab` règle `moveSpeed = 5`, capsule de rayon 0,5 et hauteur 2. `TuningConfig.asset`, effectivement référencé par le prefab et `BarGreybox`, règle la nuit à **300 s**, le portage à **0,6 ×**, le réveil à l'eau à **3 s**, le réveil automatique à **45 s**. Donc 5 unités/s seul et 3 unités/s en portage ; convention de conception : une unité Unity = un mètre. Valeurs sur disque, pas une observation de Play Mode.

Emprise indicative **220 × 130 m**, x = −170…50, z = −25…105. Cette emprise comprend réserves, îlots et décor ; ce n'est pas une surface jouable intégralement à meubler. Sol de circulation y = 0, racine joueur actuelle y = 1. Pas d'escalier, de pente, de marche de trottoir collisionnable ou de saut requis : le contrôleur déplace le Transform horizontalement et repousse les obstacles, sans gestion de gravité ou de montée.

| Parcours, seuil à seuil | Distance sur axes | Seul à 5 m/s | Portage constant à 3 m/s |
|---|---:|---:|---:|
| Coloc → entrée B1, promenade | 220,75 m | 44,2 s | 73,6 s |
| Coloc → entrée B1, ruelle courte | 180,75 m | 36,2 s | 60,3 s |
| Sortie est du bar → coloc, ruelle | 164,5 m | 32,9 s | 54,8 s |
| Coloc → entrée principale, trottoir intérieur de la route | 196 m | 39,2 s | 65,3 s |
| Taxi A → B, axe en deux coudes | 170 m | **12 s proposés en véhicule** | Transport KO non spécifié |

Calcul `temps = distance / vitesse`. Les axes complets figurent dans [trajets.json](preuves/trajets.json). Ajouter la préparation, le trajet intérieur du bar, les interactions, les arrondis des virages et les erreurs d'orientation. Le portage constant est une borne de calcul : le réveil automatique continue pendant le portage et libère le porté. Sur 180,75 m, avec KO tout juste survenu : 135 m en 45 s, puis 45,75 m en 9,2 s, soit environ **54,2 s théoriques**, plus prise en charge. Si le KO a commencé plus tôt, le réveil arrive plus tôt.

**Taxi :** accès coloc→A 15 m + B→porte du bar 20 m = 7 s à pied. Avec 12 s de roulage et une enveloppe de 4 s pour montée/descente, total ≈ **23 s**, sans attente. Gain théorique de 14 s sur la ruelle et 22 s sur la promenade. L'axe de 170 m en 12 s correspond à 51 km/h de moyenne avant arrondis : c'est déjà vif avec deux virages. Réserver des virages de rayon 8–10 m et tester la caméra ; ce n'est pas une physique validée. Descendre à 5 s impliquerait environ 122 km/h, déconseillé. Ne pas allonger la ville pour justifier le taxi. Coût, appel, embarquement collectif, KO et attente sont des règles encore à spécifier séparément ; aucune dépendance à leur fonctionnement pour rentrer à pied.

**Budget de nuit proposé, sans changer les 300 s :** préparation 25 s + aller promenade 45 s + interactions/bar 100 s + retour prudent 75 s + marge 55 s = 300 s. C'est une enveloppe de conception, pas une nouvelle règle. Les 75 s de retour comprennent sortie de salle, aide, marche et hésitation ; ils n'exigent pas 75 s de portage continu. Pour le premier test, viser un départ du bar avec au moins 75 s au compteur et vérifier combien de marge reste réellement.

### Conflits à montrer au playtest

1. **Le temps démarre à deux connectés**, y compris dans la coloc : recherche des objets et hésitation consomment les 300 s. L'environnement doit aider à les repérer ; aucun déplacement du départ du timer n'est proposé.
2. **La visite du bar n'est pas obligatoire dans les règles actuelles.** Après la sortie de tous, retour immédiat = victoire possible. Ne pas présenter le parcours dessiné comme une mission imposée et ne pas ajouter de verrou caché.
3. **Le taxi n'est pas automatiquement un choix intéressant.** Une attente supplémentaire de 14 s efface son avantage sur le raccourci pour un joueur seul. Tester le gain complet, pas seulement l'animation du véhicule.
4. **Une longue liaison peut être vide.** Le premier test mesure si 37–45 s d'aller restent acceptables sans rencontres. Si ennuyeux, réduire sa longueur avant d'ajouter des événements ou de changer le timer ; décision à Dyllan.
5. **L'entraide peut changer en route.** Le réveil automatique à 45 s et l'aide à l'eau en 3 s rendent le portage intégral improbable. Garder des dégagements pour déposer/relever, sans imposer le portage comme solution unique.

## Validation attendue

La direction retenue reste **coloc → promenade → petit quartier nocturne, raccourci riche en rencontres et taxi périphérique**. Le projet ayant déjà construit cette base, le chat principal doit intégrer uniquement les ajouts de [Rues_et_impasses.md](Rues_et_impasses.md), sans rejouer l'ancien premier incrément. Les noms proposés, métrages et placements restent réversibles ; lisibilité et feeling seront à éprouver en jeu.
