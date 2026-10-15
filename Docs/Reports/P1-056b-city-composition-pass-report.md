# Rapport P1-056b — Recomposition visuelle de la ville P1

**Statut : prêt à être présenté à Dyllan** (composition faite, contrôles techniques passés). Le jugement final sur la densité, l'échelle et le sentiment de petite ville lui appartient. Rien n'est commité ni poussé ; tout P1-056 et P1-056a est préservé.

## Départ

Branche `feature/p1-001-network-foundation`, commit `05e1928`. Changements P1-056/P1-056a non commités conservés. Aucun script, règle réseau, taxi, point fonctionnel ni asset importé modifié.

## Comparaison scène ↔ carte (avant recomposition)

| Carte (`P1-city-map.png`) | Scène après P1-056a | Écart |
| --- | --- | --- |
| Quartier nocturne encadré : bar entouré de façades, place de 32 × 20 m, ruelle bordée d'îlots, boîte/bar futurs, stands | bar isolé au bord d'un sol gris ; ruelle entre deux vides | pas de façades, pas de place, pas de bordure de ruelle |
| Liaison piétonne : ateliers fermés, promenade encadrée | promenade teal sur terrain gris | aucun bâtiment le long de la promenade |
| Résidentiel : maisons non visitables en îlots | une rangée de maisons à l'extrême est | quartier réduit à une bande, rien autour de la coloc nord/ouest |
| Route du taxi : anneau sud avec arrêts A/B | route Kenney rectiligne de 130 m, sol gris de chaque côté | grande perspective sans événement |
| Sol : plusieurs surfaces (rue, trottoirs, jardins, place) | un seul gris (lots plats gris + sol) | pas de lecture des secteurs |
| Espaces R1–R3 et réserves X1–X4 | poches violettes sans voisinage | réserves non intégrées |

## Proposition de composition (emprises principales, coordonnées scène)

- **Résidentiel (x > −72)** : lots verts ; maisons de part et d'autre de la promenade (est, ouest, nord de la coloc), clôtures de jardin, arbres, lampadaires.
- **Transition (x −70…0)** : deux rangées d'ateliers fermés au nord de la promenade (z ≈ 48,6 et 62,3), une rangée de maisons au sud (z ≈ 32,4), alignements d'arbres, **petite place** `x −62…−38, z 12…26` à mi-route.
- **Centre nocturne (x < −72)** : lots pavés ; place du bar `x −140…−106, z 20…36` ; rue en L en tuiles Kenney ; façades à l'ouest, au nord, au sud du bar ; îlots de part et d'autre de la ruelle ; trois emplacements « stands » (X4) clos de clôtures ; poches R1–R3 conservées entre les bâtiments.
- **Route A–B** : coupée par des façades marquantes côté îlot intérieur (`z = 5,6`), une rangée d'arbres, des lampadaires tous les 16 m, la place intermédiaire et le carrefour bleu/rue de la ville nocturne.

## Réalisé

| Élément | Quantité | Détail |
| --- | ---: | --- |
| Maisons / bâtiments (prefabs `Env_House*`) | 49 | rotations selon le front de rue ; bords du monde habillés |
| Clôtures (`Env_Fence`) | 32 | jardins sur la promenade, enclos des trois emplacements de stands |
| Arbres (grands et petits) | 74 | alignements le long de la route, de la promenade nord et de la place, arbres isolés, bord est et sud-est |
| Lampadaires | 19 | route, rue nocturne, promenade, quais |
| Tuiles de rue Kenney | 6 | rue nocturne en L (virage, droites étirées, deux extrémités) |
| Surfaces | 5 + lots | places pavées (`Env_Plaza`), emplacements `Env_StandPad`, lots verts (`Env_Lawn`) ou pavés (`Env_Paving`) selon le secteur |

Tout est sous `City/Dressing/` (`Transition`, `Nightlife`, `Streets`, `Landscape`, `Surfaces`) ; les prefabs sont ceux de `Assets/_Project/Prefabs/Environment/` (aucun modèle brut, aucune nouvelle importation, aucun package). Trois matériaux plats ajoutés : `Env_Paving`, `Env_Plaza`, `Env_StandPad`. Les lots plats de `City/Blocks` sont recolorés, sans collider.

## Conservation

Poses de départ, `ReturnZone`, arrêts A/B, zones d'embarquement, points de dépose, `DrinkOffer`, Marcel, portes du bar, murs de la coloc et du bar : **non déplacés**. Primitives désactivées du taxi non supprimées. Un test de recouvrement (82 boîtes de décor contre bandes, quais, bornes, zones, dépose, `DrinkOffer`, Marcel, bar et coloc) a révélé une maison posée par erreur sur le quai B ; elle a été retirée. Restent deux recouvrements de 0,1 m avec la promenade nord et deux maisons accolées au mur sud de la coloc (inchangé depuis P1-056).

## Validation

| Contrôle | Résultat |
| --- | --- |
| Compilation, console | aucune erreur ; 327 renderers, **0** matériau manquant ou non URP ; 0 `Missing Script`, 0 prefab manquant |
| Collisions | 89 colliders sous `City` : 49 maisons + 32 clôtures (objets visibles), 4 limites du monde, 2 bornes, 2 triggers de zone ; plus aucun volume abstrait. Hors `City` : murs et mobilier de la coloc et du bar, `Ground` |
| Accès (BFS, capsule) | quais A et B, ruelle, poches R1–R3, rue, place, bar et route atteints ; entrée du bar à ≈ 151 m |
| Marche réelle (contrôleur, pas de 0,1 m) | promenade (222 m), raccourci par la ruelle (183 m), itinéraire hors rues à travers lots et passage du quai B (160 m) et retour du bar vers le quai A à travers les lots (159 m) : atteints |
| Obstacles | clôture de jardin, façades d'ateliers et mur est du bar : arrêt ; porte latérale du bar : franchie ; taxi garé : traversé |
| Hôte + client | trajet A → B (caméra extérieure sur les deux propriétaires, arrivée aux points de dépose attendus) et appel à vide vers A : comportement identique à P1-055/P1-056a, aucune erreur |

Tests EditMode non relancés (aucun script modifié).

## Captures (3)

- `Docs/Reports/img/P1-056b-top-view.png` : vue du dessus.
- `Docs/Reports/img/P1-056b-player-from-home.png` : vue joueur à la sortie de la coloc (taxi au quai A, clôtures, façades, alignement d'arbres).
- `Docs/Reports/img/P1-056b-player-night-district.png` : vue joueur sur la rue nocturne (bar à droite, îlots à gauche, carrefour au fond).

### Comparaison aux captures et à la carte

- **Densité** : le sol gris continu a disparu ; chaque direction montre un bâtiment, une clôture, un arbre ou un lampadaire à moins de 25 m dans les secteurs bâtis.
- **Continuité** : rues encadrées (promenade, ruelle, rue nocturne) ; îlots de part et d'autre ; la route A–B est coupée par des façades, deux rangées d'arbres, une place et le tracé en L de la rue.
- **Repères** : balise verte de la coloc, carrefour bleu, enseigne et balise ambre du bar, bornes jaunes, taxi jaune restent plus saturés que le décor.
- **Échelle** : bâtiments × 7, taxi × 1,4 ; proportions cohérentes entre elles, à confirmer en jeu.

## Limites

- Les secteurs sont composés d'un petit nombre de modèles répétés (4 maisons, 2 arbres, 1 lampadaire) : la répétition se voit sur les rangées longues.
- Le bar et la coloc conservent leur greybox (murs unis) ; la bande en dehors du périmètre reste vide au-delà de la clôture de 2 m.
- Les emplacements de stands, la boîte et le bar futur sont des réserves sans objet ; aucune texture au sol ni mobilier urbain ajoutés.
- Les lots restent plats (contrôleur actuel) ; aucune pente, aucun trottoir surélevé.
- Les quelques passages entre clôtures et maisons de la promenade sont étroits (≈ 1 m) à l'ouest de la promenade P2.
- Aucune caméra de joueur testée à 960 × 540 en plein jeu hors captures ; pas de touche réelle jouée.

## Nettoyage et état Git

`Assets/DefaultNetworkPrefabs.asset` (+ `.meta`) : recréés pendant les essais, même GUID `6caa043d…`, non suivis, non référencés ; supprimés. `ProjectSettings` inchangé. Changements non commités : ceux de P1-056/P1-056a plus `BarGreybox.unity` (recomposée), trois matériaux `Env_*`, trois captures et ce rapport.

### Revue Codex après recomposition

Lecture directe de la scène par Unity MCP : deux objets racine `TmpShot`, chacun composé d'un `Transform` et d'une `Camera` active, avaient été sauvegardés dans `BarGreybox.unity` après les captures. Avec l'accord de Dyllan, Codex les a supprimés via l'éditeur puis a sauvegardé la scène. Aucun autre objet n'a été visé. La revue visuelle juge la densité et la continuité acceptables pour la P1 ; la répétition des modèles, les grands espaces encore nus et les passages d'environ 1 m restent des limites non bloquantes du prototype.

## Prochaine action recommandée

Dyllan parcourt la ville à pied et en taxi ; signaler les zones encore vides ou répétitives pour une dernière passe locale (diversité de modèles, façades du centre) avant l'habillage des intérieurs.
