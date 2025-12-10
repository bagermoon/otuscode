using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.SmartEnum;

namespace RestoRate.SharedKernel.Enums;

public sealed class Status : SmartEnum<Status>
{
    public static readonly Status Draft = new(nameof(Draft), 1);
    public static readonly Status OnModeration = new(nameof(OnModeration), 2);
    public static readonly Status Published = new(nameof(Published), 3);
    public static readonly Status Rejected = new(nameof(Rejected), 4);
    public static readonly Status Archived = new(nameof(Archived), 5);

    private Status(string name, int value) : base(name, value) { }

    /// <summary> Описание статуса </summary>
    public string GetDescription() => Name switch
    {
        nameof(Draft) => "Черновик",
        nameof(OnModeration) => "На модерации",
        nameof(Published) => "Опубликован",
        nameof(Rejected) => "Отклонен",
        nameof(Archived) => "В архиве",
        _ => Name
    };

    /// <summary> Можно ли редактировать ресторан в этом статусе </summary>
    public bool CanBeEdited() => this == Draft || this == Rejected;

    /// <summary> Виден ли ресторан в публичном поиске </summary>
    public bool IsVisiblePublicly() => this == Published;
}
