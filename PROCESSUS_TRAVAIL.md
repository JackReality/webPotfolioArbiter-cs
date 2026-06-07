# Processus de Travail (Workflow)

Ce document définit les règles strictes de suivi des modifications pour garantir la traçabilité et la validation par l'utilisateur.

## Règles de gestion des lots de modifications

1. **Planification** : Avant de lancer *tout* lot de modifications, une entrée décrivant le lot doit être ajoutée dans la section "🔄 Lots en cours" du fichier `STATUS.md`.
2. **Avancement** : Une fois le code d'une tâche spécifique du lot écrit et prêt à être testé, son statut doit être mis à jour dans `STATUS.md` (ex: "Code écrit, en attente de test").
3. **Validation** : Une fois le lot **entièrement terminé et explicitement validé par l'utilisateur**, l'entrée correspondante dans `STATUS.md` doit être déplacée vers la section "✅ Fait" ou cochée comme validée.
4. **Atomicité** : Ne pas enchaîner plusieurs lots distincts sans validation intermédiaire, sauf instruction contraire explicite de l'utilisateur.

> **Note** : Ce processus s'applique à toutes les tâches de développement, de refactoring ou de correction de bugs.
