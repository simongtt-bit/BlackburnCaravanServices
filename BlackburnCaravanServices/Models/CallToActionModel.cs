namespace BlackburnCaravanServices.Models;

public sealed record CallToActionModel(
    string Eyebrow,
    string Title,
    string Description,
    string ButtonText,
    string Url
);