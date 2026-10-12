# Rapport P1-011 — TuningConfig

**Statut : PASS**

## Départ

- Branche `feature/p1-001-network-foundation`, commit de départ `52d12d2`, arbre propre.
- Unity 6000.3.8f1 ; aucun package ajouté.

## Fichiers créés

- `Assets/_Project/Scripts/Runtime/Config/TuningConfig.cs` : `ScriptableObject` (menu `Last Call Crew/Tuning Config`), champs sérialisés privés, propriétés publiques en lecture seule, sections Inspector (`Header`) et tooltips avec unités.
- `Assets/_Project/Scripts/Runtime/Config/LastCallCrew.Config.asmdef` : nécessaire pour que les tests EditMode puissent référencer `TuningConfig` (un assembly de test ne peut pas référencer `Assembly-CSharp`). Seul ce dossier est concerné ; P1-001/P1-002 restent dans `Assembly-CSharp` et compilent sans changement.
- `Assets/_Project/Data/Tuning/TuningConfig.asset` : créé par l'éditeur (`CreateAsset`), avec les dossiers `Data/` et `Data/Tuning/`.
- `Assets/_Project/Tests/EditMode/LastCallCrew.Tests.EditMode.asmdef` (plateforme Editor, `UNITY_INCLUDE_TESTS`) et `TuningConfigTests.cs`.
- Tous les `.meta` correspondants. Aucun fichier existant modifié (P1-001 et P1-002 intacts, `git diff` vide sur les fichiers suivis).

## Valeurs par défaut (asset vérifié par test et lecture du YAML)

| Section | Champ | Valeur |
| --- | --- | --- |
| Ivresse | minimum | 0 |
| | seuil Pompette | 30 |
| | seuil Ivre | 60 |
| | seuil KO (= maximum) | 100 |
| | gain par verre | 20 |
| | décroissance passive (points/s) | 0,2 |
| | réduction par eau hors réveil | 15 |
| Refus | durée normale (s) | 2 |
| | multiplicateur Sociable | 2 |
| | délai entre propositions d'un PNJ (s) | 20 |
| KO et entraide | maintien réveil à l'eau (s) | 3 |
| | ivresse après réveil à l'eau | 50 |
| | réveil automatique (s) | 45 |
| | ivresse après réveil automatique | 90 |
| | multiplicateur de vitesse de portage | 0,6 |

Ces valeurs sont provisoires ; leur présence dans l'asset ne valide pas le game design.

## Choix de validation

- `GetValidationErrors()` renvoie la liste (en français) des invariants violés ; `IsValid` en découle ; `ThrowIfInvalid()` lève `InvalidOperationException` et sera appelé par les futurs consommateurs : une configuration incohérente ne peut donc pas être utilisée silencieusement.
- `OnValidate()` (modification dans l'Inspector) affiche chaque violation avec `Debug.LogError` ; aucune valeur n'est corrigée automatiquement (pas de correction silencieuse).
- Invariants : min < Pompette < Ivre < KO ; durées et délais (refus, délai PNJ, maintien d'eau, réveil automatique) > 0 ; ivresse après réveil (eau, automatique) dans [min ; KO] bornes incluses ; multiplicateur de portage dans ]0 ; 1] (0 exclu pour ne pas figer le porteur, 1 accepté).
- Choix de ma part, hors liste de la fiche, à confirmer ou retirer : gain par verre > 0, réduction d'eau > 0, multiplicateur Sociable > 0, décroissance passive ≥ 0 (0 autorisé). Les valeurs NaN sont rejetées (comparaisons écrites en négatif).
- Pas de singleton, pas de chargement générique : un système qui en a besoin recevra la référence via un champ sérialisé.

## Compilation et tests

- `recompile` puis `recompile_status` : terminé, 0 erreur ; console Unity : 0 erreur, warnings sans rapport (Account API).
- Commande : `unity command run_tests --mode EditMode --filter LastCallCrew.Tests`
- Résultat : 24 tests, 24 réussis, 0 échec, 0 ignoré :
  - valeurs par défaut du GDD et validité d'une instance neuve ;
  - l'asset `TuningConfig.asset` existe, porte les mêmes valeurs et est valide ;
  - 21 cas de valeurs invalides (ordre des seuils, valeurs nulles/négatives, hors intervalle, multiplicateur 0/>1/négatif, NaN) rejetés avec `ThrowIfInvalid` ;
  - valeurs limites acceptées (portage = 1, réveils = 0 et 100, décroissance = 0).
- Aucun test P1-001/P1-002 n'existe (preuve réseau manuelle) ; aucun fichier P1 modifié, donc pas de régression attendue. Le comportement réseau n'a pas été rejoué.

## Limites

- Les tests accèdent aux champs privés par réflexion (noms de champs) : renommer un champ impose de mettre à jour les tests.
- La persistance après relance d'Unity repose sur la sérialisation standard (valeurs lues dans le YAML de l'asset) ; je n'ai pas redémarré l'éditeur.
- L'affichage de l'Inspector (sections, tooltips) n'a pas été regardé visuellement ; à vérifier par Dyllan.
- NGO recrée `Assets/DefaultNetworkPrefabs.asset` à la compilation ; supprimé avant la fin (non référencé).
- Rien n'est connecté au joueur, au réseau ni aux scènes.

## Changements non commités

Tout ce qui est listé ci-dessus (fichiers non suivis). Aucun commit, aucun push.

## Prochaine action recommandée

Dyllan ouvre l'asset dans l'Inspector pour valider lisibilité et unités, puis confirme les invariants ajoutés hors fiche. Ensuite, la prochaine fiche de Codex.
