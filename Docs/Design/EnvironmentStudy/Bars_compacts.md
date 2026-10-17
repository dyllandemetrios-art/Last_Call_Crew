# Révision 4 — petits bars thématiques

17 octobre 2026. Décision de Dyllan : remplacer le grand bar générique par plusieurs petits bars, chacun reconnaissable par les boissons posées sur son comptoir. Étude uniquement ; aucune modification du checkout Unity.

![Plan avec trois bars](Carte_P1.png)

## Découpage proposé

L'ancienne emprise du bar, 17 × 16 m soit 272 m², est divisée en deux cellules mitoyennes de 8,5 × 16 m, soit 136 m² chacune. Le bar futur au nord est réduit de 18 × 14 m à 10 × 12 m, soit 120 m². Ces dimensions restent des emprises extérieures de greybox ; murs, réserve et toilettes réduiront la surface réellement accessible.

| Code | Lieu | Statut P1 | Emprise | Identité du comptoir |
|---|---|---|---:|---|
| B1 | **Bar à bières** | Jouable | 8,5 × 16 m | Chopes, verres droits, canettes et bouteilles ; tireuses en primitives si nécessaire. |
| B2 | **Bar à shooters** | Réserve fermée | 8,5 × 16 m | Petits verres alignés, bouteilles colorées et plateaux ; verres à shot en primitives si nécessaire. |
| B3 | **Bar à cocktails** | Réserve fermée | 10 × 12 m | Verres à cocktail, verres à vin, citron, orange et bouteilles ; shaker en primitive si nécessaire. |

La boîte X1 reste une réserve distincte. Les trois bars bordent le même petit secteur : B1 et B2 partagent l'ancienne coque au sud de la rue de la Soif ; B3 leur fait face au nord. Cette concentration donne immédiatement l'impression d'une rue nocturne et laisse chaque intérieur assez petit pour être animé plus tard avec peu de PNJ.

## Implantation intérieure minimale

Pour B1, conserver une entrée de 3 m sur la place, la sortie est existante et un passage libre d'au moins 1,5 m devant le comptoir. Poser le comptoir dans la longueur contre le mur nord ou ouest afin que quatre joueurs puissent se croiser sans transformer le bar en grande salle vide. Deux à quatre petites tables suffisent ; la densité devra être évaluée depuis la caméra réelle.

B2 et B3 restent fermés pour P1 : façade, enseigne, porte close et aperçu du thème par la vitrine ou le comptoir suffisent. Ils ne nécessitent ni interaction, ni carte de boissons, ni intérieur détaillé. Leur ouverture future pourra réutiliser le même module : coque compacte, entrée sur rue, comptoir longitudinal, arrière-bar et petite zone de tables.

Si B1 paraît encore trop vide lors du test, fermer les 4 m du fond en réserve technique sans déplacer sa façade. La zone publique passerait alors à environ 8,5 × 12 m, soit 102 m², tout en conservant la sortie est et l'identité extérieure.

## Assets vérifiés et placeholders

Le Kenney Food Kit contient réellement `mug`, `glass`, `cocktail`, `glass-wine`, `wine-red`, `wine-white`, `soda-bottle`, `soda-can`, `lemon`, `lemon-half`, `orange`, `can` et `barrel`. Ils permettent de distinguer les comptoirs sans achat. Le pack Furniture fournit les tables et sièges génériques.

Les tireuses à bière, verres à shot et shakers dédiés ne sont pas établis par l'inventaire. Utiliser des primitives colorées et étiquetées jusqu'à ce qu'un asset convenable soit confirmé. Les boissons exposées servent d'abord de langage visuel ; cette fiche ne crée aucune recette ni mécanique de commande.

## Première reprise testable pour Claude Code

Ne pas reconstruire le quartier. Dans la scène existante, dupliquer temporairement la coque actuelle avant intervention, puis poser une cloison de greybox au milieu de son emprise : B2 à l'ouest, B1 à l'est. Réduire B3 à 10 × 12 m en préservant la rue de la Soif et ses raccords. Garder B2/B3 fermés et ne rendre jouable que B1.

Contrôle court : quatre joueurs entrent dans B1, se croisent devant le comptoir, atteignent la sortie est et peuvent déposer/reprendre un partenaire sans collision bloquante. Depuis la rue, les trois thèmes doivent être identifiables en moins de trois secondes par l'enseigne et les silhouettes d'objets. Le ressenti reste à valider en jeu ; cette étude ne constitue pas un PASS gameplay.
