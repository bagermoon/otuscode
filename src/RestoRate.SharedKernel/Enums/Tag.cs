using Ardalis.SmartEnum;

namespace RestoRate.SharedKernel.Enums;

public sealed class Tag : SmartEnum<Tag>
{
    public static readonly Tag Banquet = new(nameof(Banquet), 1);
    public static readonly Tag Birthday = new(nameof(Birthday), 2);
    public static readonly Tag Wedding = new(nameof(Wedding), 3);
    public static readonly Tag CorporateParty = new(nameof(CorporateParty), 4);
    public static readonly Tag Georgian = new(nameof(Georgian), 5);
    public static readonly Tag Italian = new(nameof(Italian), 6);
    public static readonly Tag Chinese = new(nameof(Chinese), 7);
    public static readonly Tag Russian = new(nameof(Russian), 8);
    public static readonly Tag Japanese = new(nameof(Japanese), 9);
    public static readonly Tag Seafood = new(nameof(Seafood), 10);
    public static readonly Tag Steakhouse = new(nameof(Steakhouse), 11);
    public static readonly Tag Pizza = new(nameof(Pizza), 12);
    public static readonly Tag Khinkali = new(nameof(Khinkali), 13);
    public static readonly Tag Teahouse = new(nameof(Teahouse), 14);
    public static readonly Tag Breakfast = new(nameof(Breakfast), 15);
    public static readonly Tag Bar = new(nameof(Bar), 16);
    public static readonly Tag WineBar = new(nameof(WineBar), 17);
    public static readonly Tag BeerBar = new(nameof(BeerBar), 18);
    public static readonly Tag Pub = new(nameof(Pub), 19);
    public static readonly Tag SportsBar = new(nameof(SportsBar), 20);
    public static readonly Tag Cafe = new(nameof(Cafe), 21);
    public static readonly Tag NightClub = new(nameof(NightClub), 22);
    public static readonly Tag StripClub = new(nameof(StripClub), 23);
    public static readonly Tag LiveMusic = new(nameof(LiveMusic), 24);
    public static readonly Tag Karaoke = new(nameof(Karaoke), 25);
    public static readonly Tag Hookah = new(nameof(Hookah), 26);
    public static readonly Tag SummerTerrace = new(nameof(SummerTerrace), 27);
    public static readonly Tag NewYearParty = new(nameof(NewYearParty), 28);
    public static readonly Tag TwentyFourHours = new(nameof(TwentyFourHours), 29);
    public static readonly Tag KidsRoom = new(nameof(KidsRoom), 30);
    public static readonly Tag FuneralCafe = new(nameof(FuneralCafe), 31);
    public static readonly Tag Delivery = new(nameof(Delivery), 32);

    private Tag(string name, int value) : base(name, value) { }

    /// <summary> Описание тега на русском </summary>
    public string GetDescription() => Name switch
    {
        nameof(Banquet) => "Банкет в ресторане",
        nameof(Birthday) => "Где отметить день рождения",
        nameof(Wedding) => "Ресторан для свадьбы",
        nameof(CorporateParty) => "Новогодний корпоратив",
        nameof(Georgian) => "Грузинские рестораны",
        nameof(Italian) => "Итальянские рестораны",
        nameof(Chinese) => "Китайские рестораны",
        nameof(Russian) => "Рестораны русской кухни",
        nameof(Japanese) => "Японские рестораны",
        nameof(Seafood) => "Рыбный ресторан",
        nameof(Steakhouse) => "Стейк-хаус",
        nameof(Pizza) => "Пиццерии",
        nameof(Khinkali) => "Хинкальные",
        nameof(Teahouse) => "Чайхона",
        nameof(Breakfast) => "Завтраки в ресторанах",
        nameof(Bar) => "Бары",
        nameof(WineBar) => "Винные бары / рестораны (винотека)",
        nameof(BeerBar) => "Пивной ресторан",
        nameof(Pub) => "Пабы",
        nameof(SportsBar) => "Спорт-бары",
        nameof(Cafe) => "Кофейни",
        nameof(NightClub) => "Ночные клубы",
        nameof(StripClub) => "Стриптиз-клубы",
        nameof(LiveMusic) => "Живая музыка",
        nameof(Karaoke) => "Караоке",
        nameof(Hookah) => "Кальян",
        nameof(SummerTerrace) => "Летние веранды",
        nameof(NewYearParty) => "Новогодняя ночь",
        nameof(TwentyFourHours) => "Круглосуточные",
        nameof(KidsRoom) => "Детская комната",
        nameof(FuneralCafe) => "Кафе для поминок",
        nameof(Delivery) => "Доставка еды из ресторанов",
        _ => Name
    };
}
