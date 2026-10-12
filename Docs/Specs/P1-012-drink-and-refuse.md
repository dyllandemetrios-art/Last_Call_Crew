# P1-012 Boire et refuser

## Intention

Transformer l'interaction réseau de P1-002 en premier choix de gameplay : accepter immédiatement un verre ou maintenir une commande pour le refuser. L'hôte reste l'autorité de la décision et les deux instances reçoivent le même résultat.

Cette tâche valide le choix et son feedback. Elle n'applique pas encore de valeur d'ivresse au joueur ; cette responsabilité appartient à P1-013.

## Comportement observable

Dans `NetworkTest.unity`, un objet temporaire représente une proposition de verre. Lorsqu'un joueur propriétaire le vise à portée :

- appuyer sur `E` accepte immédiatement le verre ;
- maintenir le bouton droit de la souris pendant la durée configurée refuse le verre ;
- relâcher trop tôt annule le refus et ne produit aucune décision.

Après validation par l'hôte, un feedback temporaire indique sur les deux instances quel joueur a accepté ou refusé. La proposition devient indisponible pendant le délai configuré avant de pouvoir être faite de nouveau.

## Règles de gameplay

- La durée de maintien vient de `TuningConfig.RefusalDurationSeconds` ou de la propriété équivalente déjà créée en P1-011.
- Le délai entre deux propositions vient de la valeur correspondante de `TuningConfig` ; aucune copie littérale de ces nombres ne doit être ajoutée au code de gameplay.
- Le multiplicateur du profil Sociable n'est pas appliqué : les profils de personnage ne font pas partie de P1-012.
- Une demande ne peut produire qu'une seule décision : accepter ou refuser, jamais les deux.
- Une proposition déjà décidée ou en délai de récupération refuse toute nouvelle demande.

## Autorité réseau

Le client propriétaire lit ses entrées et affiche la progression locale du maintien. Il envoie seulement une demande de décision.

L'hôte vérifie au minimum :

- l'identité de l'émetteur ;
- l'existence de la cible ;
- la portée au moment de la décision ;
- la disponibilité de la proposition ;
- la durée de maintien requise pour un refus.

Le client ne doit pas pouvoir certifier lui-même qu'il a maintenu assez longtemps. L'hôte mesure ou vérifie la durée entre le début et la fin de la demande de refus. L'état décidé et le délai restant sont autoritaires et répliqués.

## Travail demandé

1. Lire `AGENTS.md`, `Docs/Backlog.md`, les rapports P1-002 et P1-011, puis cette fiche.
2. Vérifier que le commit de départ est `4757202` et que l'arbre est propre.
3. Réutiliser le raycast, la validation de portée et le patron d'autorité de P1-002 ; ne pas créer de framework d'interaction généraliste.
4. Ajouter une action Input System `Refuse`, liée provisoirement au bouton droit de la souris. La durée reste dans `TuningConfig`, pas dans le binding.
5. Remplacer ou spécialiser le cube de test par une proposition de verre minimale dans `NetworkTest.unity`.
6. Référencer explicitement l'asset `TuningConfig` depuis le composant qui en a besoin, sans singleton ni chargement global.
7. Implémenter l'acceptation immédiate et le refus maintenu avec les validations serveur décrites ci-dessus.
8. Ajouter un feedback de test lisible pour l'invite, la progression du refus, la dernière décision et l'indisponibilité temporaire. L'interface finale n'est pas demandée.
9. Ajouter des tests EditMode ciblés sur la règle de décision et les cas limites lorsque ces règles peuvent être isolées de NGO.
10. Tester manuellement avec un hôte et un client, dans les deux sens.

## Critères d'acceptation

- L'hôte et le client peuvent chacun accepter une proposition valide.
- L'hôte et le client peuvent chacun refuser après le maintien configuré.
- Un relâchement avant la durée configurée annule le refus.
- Une pression ou un maintien valide produit exactement une décision.
- Une demande hors portée, pendant l'indisponibilité ou provenant d'un émetteur invalide est rejetée par l'hôte.
- La dernière décision et la disponibilité sont identiques sur les deux instances.
- Les durées proviennent de l'asset `TuningConfig` et non de constantes dupliquées.
- Unity compile sans erreur et les tests pertinents passent.
- Les comportements validés de P1-001 et P1-002 ne régressent pas.

## Validation manuelle demandée

Avec deux instances, effectuer au minimum :

1. une acceptation par l'hôte ;
2. une acceptation par le client ;
3. un refus complet par l'hôte ;
4. un refus complet par le client ;
5. un refus relâché trop tôt ;
6. une tentative hors portée ;
7. une tentative pendant le délai d'indisponibilité.

Consigner le résultat observé sur les deux écrans et toute erreur de console. Le feeling du maintien et la lisibilité du feedback restent à valider par Dyllan.

## Hors périmètre

Jauge et gain d'ivresse, décroissance, seuils, profils de personnage, PNJ final, dialogue comique, animation, audio, eau, KO, portage, réveil, interface définitive et Relay.

## Compte rendu attendu

Créer `Docs/Reports/P1-012-drink-and-refuse-report.md` avec : commit de départ, fichiers créés ou modifiés, protocole d'autorité, usage de `TuningConfig`, tests automatiques et manuels exécutés, résultats observés sur les deux instances, erreurs de console, limites, statut PASS, FAIL ou BLOCKED et prochaine action. Ne pas commit ni push sans autorisation explicite.
