# P1-021 Portage d'un partenaire KO

## Intention

Permettre au joueur encore debout de transporter son partenaire KO puis de le lâcher, avec un état autoritaire identique sur les deux instances. Cette tâche prouve l'entraide physique minimale ; elle n'ajoute aucun réveil.

## Comportement observable

- Un joueur debout vise son partenaire KO à courte portée et appuie sur `E` pour le porter.
- Le corps KO se place sur son dos ou son épaule et suit ses déplacements.
- La vitesse du porteur est multipliée par `TuningConfig.CarrySpeedMultiplier`.
- Appuyer de nouveau sur `E` lâche le partenaire près du porteur, sur le sol.
- Le joueur porté reste KO et conserve sa caméra KO.
- Les deux instances voient le même porteur, le même joueur porté et la même position.

## Règles P1

- Un joueur ne peut porter qu'un seul partenaire.
- Un joueur ne peut être porté que par un seul partenaire.
- Impossible de se porter soi-même, de porter un joueur non KO ou de commencer hors portée.
- Un joueur KO ne peut porter personne.
- Pendant le portage, `E` est réservé au lâcher et les interactions boire/refuser du porteur sont bloquées.
- Si le porteur devient KO, se déconnecte ou disparaît, le joueur porté est lâché proprement.
- Le portage ne réveille pas et ne change aucune valeur d'ivresse.

## Autorité réseau

- L'hôte valide la demande de prise et de lâcher : identité, états KO, portée, disponibilité des deux joueurs et absence de portage concurrent.
- L'état durable doit identifier le porteur et le joueur porté ; les nouveaux clients doivent le recevoir sans RPC de rattrapage.
- Utiliser le parenting réseau NGO ou une solution équivalente validée pour que la racine du joueur porté suive réellement le porteur. Ne pas déplacer seulement son mesh.
- Le joueur KO ne pilote plus son transform ; éviter tout conflit avec l'autorité propriétaire existante.
- Au lâcher, restaurer une racine indépendante et placer le joueur au sol sans modifier son état KO.

## Présentation minimale

- Ajouter un point de portage sérialisé sur le prefab joueur, placé derrière ou sur l'épaule.
- Le visuel KO reste couché et attaché au point de portage ; aucun système d'animation ou d'IK.
- Désactiver localement le collider du joueur porté pendant le transport si nécessaire pour éviter qu'il bloque le porteur ; le restaurer au lâcher.
- Étendre le HUD existant : `E — PORTER` en visant un partenaire KO, puis `E — LÂCHER` pendant le portage. Indiquer clairement `VOUS PORTEZ JOUEUR N` et `PORTÉ PAR JOUEUR N`.

## Travail demandé

1. Lire `AGENTS.md`, le rapport P1-020 et cette fiche.
2. Partir du commit `1d883b3` avec un arbre propre.
3. Ajouter une responsabilité réseau de portage courte, sans framework d'interaction généraliste.
4. Réutiliser la visée et la portée de `PlayerInteractor`.
5. Implémenter les validations serveur et les cas d'annulation.
6. Appliquer le multiplicateur de vitesse configuré sans modifier la vitesse de base.
7. Ajouter le point de portage et les références du prefab avec l'éditeur Unity.
8. Étendre l'interface de debug existante sans panneau concurrent.
9. Ajouter des tests EditMode pour les règles pures utiles.
10. Valider le portage dans les deux directions avec un hôte et un client.

## Critères d'acceptation

- Hôte et client peuvent chacun porter puis lâcher l'autre lorsqu'il est KO.
- Une demande invalide est rejetée sans changement d'état.
- Le joueur porté suit le porteur sans divergence visible sur les deux instances.
- La vitesse du porteur applique exactement le multiplicateur configuré et revient à la normale après le lâcher.
- Le joueur porté reste KO, ne contrôle rien et ne change pas d'ivresse.
- Le collider ne bloque pas le déplacement pendant le transport et est restauré au lâcher.
- Le lâcher place le joueur sur le sol à proximité, sans téléportation incohérente ni passage sous le niveau.
- Le KO du porteur provoque un lâcher automatique cohérent.
- Un rejoin tardif reçoit l'état de portage actuel.
- Mouvement, caméra KO et boire/refuser hors portage ne régressent pas.
- Compilation, tests et consoles ne présentent aucune nouvelle erreur.

## Validation manuelle demandée

1. Mettre le client KO, le porter avec l'hôte, marcher et le lâcher.
2. Refaire avec l'hôte KO et le client porteur.
3. Comparer approximativement la vitesse normale et la vitesse chargée.
4. Tenter de porter un joueur debout, hors portée et déjà porté.
5. Faire atteindre le KO au porteur pendant le transport et vérifier le lâcher automatique.
6. Vérifier les deux caméras et les messages sur les deux écrans.

## Hors périmètre

Animation de prise ou de marche, IK, ragdoll, collision caméra, choix de pose selon le personnage, lancer, inventaire, eau, réveil, réveil automatique, sortie de niveau, Relay et build autonome.

## Compte rendu attendu

Créer `Docs/Reports/P1-021-carry-report.md` avec : commit de départ, fichiers modifiés, modèle d'autorité, méthode de suivi réseau, règles de validation, gestion des colliders, vitesse, prises/lâchers dans les deux directions, KO du porteur, rejoin tardif, tests, erreurs, limites et statut PASS, FAIL ou BLOCKED. Ne pas commit ni push.
