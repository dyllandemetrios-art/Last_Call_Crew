# P1-054 — Squelette mesurable de la ville

## Intention

Construire dans `BarGreybox.unity` le trajet fixe coloc → quartier nocturne → bar → coloc afin de mesurer son rythme à deux joueurs avant d'importer les assets ou de développer le taxi.

Référence de conception : `Docs/Design/P1-environment-foundation.md` et sa carte.

## Périmètre

Un seul incrément : conserver la coloc, déplacer le bar existant, construire deux chemins piétons, trois poches de ruelle et tracer l'emprise de la future route du taxi.

- Conserver la maison, sa porte, `ReturnZone`, les spawns et les clés, le portefeuille et le téléphone.
- Réemployer le bar actuel avec Marcel, `DrinkOffer`, le mobilier, les portes et les lumières. Ne pas créer un second bar ni dupliquer ses `NetworkObject`.
- Déplacer le bar comme un ensemble. Translation de départ proposée pour son sol : `(−131,5 ; 0 ; +28)`. Vérifier ses coordonnées réelles dans l'éditeur avant l'opération.
- Étendre le sol plat et fermer les îlots par des volumes visibles avec colliders simples.
- Construire la promenade et le raccourci selon la carte. Aucun passage diagonal à travers les îlots.
- Ajouter trois renfoncements R1, R2 et R3 de 6 × 6 m, ouverts sur la ruelle. Conserver 4 m libres dans le passage.
- Poser trois repères simples : maison verte existante, carrefour bleu et entrée de bar ambre.
- Tracer les arrêts A/B et l'emprise de la route avec des primitives.

Les assets, les PNJ supplémentaires, les substances, l'économie, le taxi mobile et la génération procédurale sont hors périmètre.

## Axes indicatifs en X/Z

- Promenade : `(15,16) → (0,16) → (0,40) → (−70,40) → (−70,60) → (−110,60) → (−110,30) → (−130,30) → (−130,36)`.
- Raccourci : `(15,16) → (0,16) → (0,40) → (−70,40) → (−110,40) → (−110,30) → (−130,30) → (−130,36)`.
- Sortie est du bar : `(−121.5,44) → (−110,44)`, puis raccord à la ruelle.
- Route réservée : A `(0,16) → (0,−4) → (−130,−4) → (−130,16)` B.
- Poches : R1 `x −104…−98, z 32…38` ; R2 `x −94…−88, z 42…48` ; R3 `x −82…−76, z 32…38`.

Si les coordonnées réelles rendent un raccord incohérent, adapter localement la géométrie et documenter l'écart. Ne pas modifier les règles réseau ou le timer pour faire correspondre les durées.

## Comportement observable

- Deux joueurs quittent la coloc et atteignent le bar par la promenade sans saut ni blocage.
- Ils reviennent par la ruelle et reconnaissent la direction de la maison grâce aux repères.
- Deux joueurs peuvent s'arrêter dans chaque poche, déposer et reprendre un partenaire, tandis qu'un autre joueur conserve le passage central.
- La sortie latérale du bar reste utilisable avec un partenaire porté.
- La maison conserve ses règles de départ, de retour collectif et de relance.
- La route du taxi est compréhensible comme une réserve, sans véhicule fonctionnel.

## Validation

1. Compilation sans nouvelle erreur et références de scène intactes.
2. Hôte et client parcourent maison → entrée principale du bar → sortie latérale → maison avec de vraies entrées. Ne pas remplacer cette preuve par une téléportation.
3. Mesurer une fois chaque trajet, seuil à seuil et sans pause volontaire. Attentes : promenade ≈ 45 s, raccourci ≈ 37 s, sortie latérale ≈ 33 s. Un écart supérieur à 15 % doit être expliqué.
4. Provoquer un KO au bar, porter le partenaire à travers la sortie et un virage, puis vérifier lâcher et réveil sans blocage. Noter le réveil automatique s'il survient.
5. Vérifier R1–R3 avec une capsule inerte : arrêt à deux, dépôt/reprise et passage libre de 4 m.
6. Vérifier qu'aucune victoire n'arrive au spawn, que le retour exige une présence simultanée et que la relance restaure joueurs et objets domestiques.
7. Conserver une vue du dessus et au maximum une capture d'un problème utile. Ne pas multiplier les captures.

Le jugement de Dyllan portera ensuite uniquement sur l'orientation et le rythme : retrouver la coloc sans carte, identifier le bar et le futur taxi, puis signaler les portions vides ou toute hésitation de plus de cinq secondes.

## Critères de fin

- La géométrie forme une scène continue et empêche les raccourcis involontaires.
- Les deux chemins sont parcourables à deux et avec les interactions d'entraide existantes.
- Les durées réelles et les écarts sont consignés.
- Aucun gameplay de taxi, import artistique ou nouvelle règle de partie n'est ajouté.
- Le rapport `Docs/Reports/P1-054-city-route-greybox-report.md` distingue preuves automatiques, observations hôte/client et feeling restant à valider.

Ne pas commit ni push après l'implémentation.
