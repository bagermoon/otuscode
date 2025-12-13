namespace RestoRate.Contracts.Common.Dtos;

/// <summary>
/// Представляет канонический тег, используемый для категоризации ресторанов.
/// <para>`Id` — уникальный идентификатор тега (полезен, когда теги сохраняются как агрегат в контексте Restaurant).</para>
/// <para>`Name` — человекочитаемое имя тега, отображаемое пользователям (например, "vegan", "family-friendly").</para>
/// </summary>
public record TagDto(Guid Id, string Name);
