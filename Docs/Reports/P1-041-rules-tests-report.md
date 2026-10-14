# P1-041 — Tests des règles

**Statut : PASS** par audit des tests existants et dernière exécution complète EditMode `199/199` consignée dans le rapport P1-040.

## Critères couverts

### Ivresse

`IntoxicationRulesTests.cs` vérifie notamment : gain configuré, plafond au seuil KO, cinq boissons jusqu'au seuil, décroissance et minimum, gel au seuil KO, valeurs hors limites et `NaN`, tous les paliers à leurs frontières et utilisation des valeurs du `TuningConfig` plutôt que de constantes cachées.

### KO

`KnockoutRulesTests.cs` vérifie : juste sous le seuil, au seuil et au-dessus, absence de double déclenchement, comportement avec `NaN`, enchaînement des boissons et seuil provenant du `TuningConfig`.

### Victoire et échec

`NightOutcomeRulesTests.cs` vérifie : deux joueurs rentrés, joueur manquant, troisième joueur, retour avant/à/après l'échéance, impossibilité de gagner seul, minuterie en attente, immuabilité du premier résultat et échec de déconnexion.

`ReturnRulesTests.cs` couvre en complément l'entrée, la sortie, le retour dans la zone et le cas qui avait causé une fausse victoire lorsqu'un joueur ressortait.

## Preuve d'exécution

- Dernière exécution : **199 tests EditMode réussis sur 199**.
- Compilation Unity : aucune erreur lors de la validation P1-040.
- Preuve source : `Docs/Reports/P1-040-critical-network-cases-report.md`.
- Commit audité : `1be60c0` (`feat: add Relay networking and critical disconnect handling`).

Les tests n'ont pas été relancés pour cette revue documentaire : l'arbre était propre au départ et aucun code de production ni test n'a été modifié. Ce rapport ne prétend pas remplacer les validations multijoueurs et builds déjà consignées dans les rapports correspondants.

## Limite

La suite est principalement composée de tests EditMode de logique pure. Les comportements Unity, réseau et interface restent couverts par les validations en éditeur et builds P1-003 à P1-040, pas par ces tests seuls.
