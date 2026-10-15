# P1-056a — Caméra taxi extérieure et ville ouverte

## Retour de playtest

Dyllan valide l'utilisation des assets, mais refuse deux éléments de la première passe :

- la vue intérieure du taxi, car le modèle ne possède pas d'intérieur exploitable ;
- les couloirs dessinés par les grands blocs de collision, car l'exploration libre et la possibilité de se perdre font partie du jeu.

Cette correction termine P1-056. Elle ne change pas les règles du taxi ni la disposition générale de la ville.

## Caméra pendant le taxi

Pendant un trajet avec passagers, la caméra du propriétaire passe en troisième personne derrière et au-dessus du taxi. Elle montre le véhicule et une partie du décor parcouru.

- Point de départ recommandé : environ 7 m derrière, 3 m au-dessus, regard vers le toit ou légèrement devant le véhicule.
- Exposer distance, hauteur, point visé et vitesse de suivi dans l'Inspector.
- Suivre après le déplacement et le placement des passagers afin d'éviter les tremblements. Un composant local de caméra exécuté après `CollectiveTaxi` et `PlayerKnockout` convient.
- Aucun état réseau supplémentaire : chaque propriétaire déduit la pose depuis le véhicule déjà synchronisé par le temps serveur.
- Les entrées de regard restent bloquées pendant le trajet.
- À l'arrivée, à l'annulation ou à la relance, restaurer exactement la pose locale précédente de la caméra.
- Si le propriétaire était KO et porté avant le trajet, restaurer sa vue KO après l'arrivée ; ne pas forcer la première personne.
- Une caméra ne doit jamais être modifiée pour les joueurs non propriétaires.

Éviter un nouveau gestionnaire global. Étendre proprement le système existant avec un petit composant de présentation sur le prefab joueur ou une API locale étroite entre `CollectiveTaxi` et `PlayerController`.

## Ouverture du monde

Supprimer la fonction de couloir des volumes abstraits sous `City/Blocks`.

- Désactiver ou retirer leurs `BoxCollider` lorsqu'ils n'ont pas d'équivalent visible.
- Conserver la limite extérieure du monde et les collisions existantes de la coloc et du bar.
- Ajouter des `BoxCollider` simples correspondant aux empreintes visibles des maisons et aux clôtures placées.
- Les arbres peuvent rester sans collision pour cette étape, sauf si leur tronc crée une incohérence flagrante.
- Les pelouses, trottoirs, rues, chaussées et lots libres deviennent accessibles à pied.
- La route du taxi peut être traversée. Le taxi reste sans collider : il ne pousse ni ne blesse les joueurs.
- Les quais, bornes et zones d'embarquement restent accessibles et fonctionnels.
- R1–R3 restent des emplacements intéressants, mais le joueur n'est pas obligé de passer par la ruelle.
- Toute nouvelle collision doit être expliquée par un objet visible. Aucun mur invisible intérieur.

Ne pas ajouter maintenant les jeux, vendeurs, substances ou événements évoqués par Dyllan. La scène doit seulement leur laisser de l'espace pour de futures fiches.

## Conservation des acquis

- Ne pas déplacer la coloc, le bar, les arrêts, les objets domestiques ou la zone de retour.
- Conserver les assets et licences importés en P1-056.
- Ne pas modifier l'autorité, la durée de 12 s, les états, les validations ou le trafic réseau du taxi.
- Conserver les repères vert, bleu et ambre, utiles même si le joueur peut se perdre.
- Aucun package supplémentaire.

## Validation

1. Compilation sans erreur, aucun `Missing Script`, prefab ou matériau manquant.
2. Hôte et client effectuent un trajet A → B avec la caméra extérieure sur chaque propriétaire ; aucun tremblement notable, coque traversée ou vue intérieure.
3. Arrivée, trajet retour, appel à vide, expiration et relance restaurent la bonne caméra et les contrôles.
4. Propriétaire KO porté : caméra extérieure pendant le trajet, puis retour à la caméra KO à l'arrivée.
5. Depuis la coloc, un joueur peut rejoindre le quartier nocturne en traversant les espaces ouverts et la route, sans suivre promenade ou ruelle.
6. Le joueur ne traverse pas les maisons, clôtures, murs du bar, murs de la coloc ou limite extérieure visibles.
7. Aucun grand volume invisible ne forme encore de couloir intérieur. Fournir une liste des colliders restants sous `City` et leur justification visible.
8. Le taxi, les bornes, l'embarquement collectif, le portage et les points de dépose restent fonctionnels en hôte/client.
9. Deux captures maximum : caméra extérieure pendant le trajet et vue montrant un espace auparavant fermé désormais traversable.
10. Dyllan juge le cadrage de la caméra et la sensation d'ouverture. Ne pas lui demander de rejouer les refus réseau déjà prouvés.

Les règles pures du taxi ne changent pas : ne relancer tous les tests EditMode que si elles ou un script qu'elles couvrent sont modifiés. Compiler et faire les contrôles hôte/client ciblés après la modification du prefab, de la scène et de la caméra.

## Critères de fin

- Le modèle de taxi est observé de l'extérieur pendant tous les trajets avec passagers.
- La pose de caméra correcte revient dans chaque état de sortie, KO compris.
- La ville est librement traversable entre les obstacles visibles.
- Les acquis P1-054, P1-055 et les assets P1-056 restent fonctionnels.
- Rapport : `Docs/Reports/P1-056a-open-world-and-taxi-camera-report.md`.

Ne pas commit ni push après la correction.
