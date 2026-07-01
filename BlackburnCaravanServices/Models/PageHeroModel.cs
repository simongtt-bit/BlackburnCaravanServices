namespace BlackburnCaravanServices.Models;

public sealed record PageHeroModel(
    string Eyebrow,
    string Title,
    string Description,
    IReadOnlyList<PageHeroFeatureModel> Features
);

public sealed record PageHeroFeatureModel(
    string Title,
    string Text
);