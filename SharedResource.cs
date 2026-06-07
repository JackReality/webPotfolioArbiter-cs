namespace Portfolio;

/// <summary>
/// Classe "marqueur" VIDE qui sert UNIQUEMENT de point d'ancrage pour les
/// traductions. On injecte IStringLocalizer&lt;SharedResource&gt; dans les pages,
/// et .NET va chercher les textes dans les fichiers Resources/SharedResource(.xx).resx.
///
/// Pourquoi une seule classe partagée ? Pour qu'un débutant ait TOUTES ses
/// traductions au même endroit (un fichier par langue), au lieu d'un fichier
/// .resx par page.
/// </summary>
public class SharedResource
{
}
