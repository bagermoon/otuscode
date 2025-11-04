using Ardalis.SmartEnum;

namespace RestoRate.SharedKernel.Enums;

public sealed class RestaurantTag : SmartEnum<RestaurantTag>
{
    public static readonly RestaurantTag Banquet = new(nameof(Banquet), 1);
    public static readonly RestaurantTag Birthday = new(nameof(Birthday), 2);
    public static readonly RestaurantTag Wedding = new(nameof(Wedding), 3);
    public static readonly RestaurantTag CorporateParty = new(nameof(CorporateParty), 4);
    public static readonly RestaurantTag Georgian = new(nameof(Georgian), 5);
    public static readonly RestaurantTag Italian = new(nameof(Italian), 6);
    public static readonly RestaurantTag Chinese = new(nameof(Chinese), 7);
    public static readonly RestaurantTag Russian = new(nameof(Russian), 8);
    public static readonly RestaurantTag Japanese = new(nameof(Japanese), 9);
    public static readonly RestaurantTag Seafood = new(nameof(Seafood), 10);
    public static readonly RestaurantTag Steakhouse = new(nameof(Steakhouse), 11);
    public static readonly RestaurantTag Pizza = new(nameof(Pizza), 12);
    public static readonly RestaurantTag Khinkali = new(nameof(Khinkali), 13);
    public static readonly RestaurantTag Teahouse = new(nameof(Teahouse), 14);
    public static readonly RestaurantTag Breakfast = new(nameof(Breakfast), 15);
    public static readonly RestaurantTag Bar = new(nameof(Bar), 16);
    public static readonly RestaurantTag WineBar = new(nameof(WineBar), 17);
    public static readonly RestaurantTag BeerBar = new(nameof(BeerBar), 18);
    public static readonly RestaurantTag Pub = new(nameof(Pub), 19);
    public static readonly RestaurantTag SportsBar = new(nameof(SportsBar), 20);
    public static readonly RestaurantTag Cafe = new(nameof(Cafe), 21);
    public static readonly RestaurantTag NightClub = new(nameof(NightClub), 22);
    public static readonly RestaurantTag StripClub = new(nameof(StripClub), 23);
    public static readonly RestaurantTag LiveMusic = new(nameof(LiveMusic), 24);
    public static readonly RestaurantTag Karaoke = new(nameof(Karaoke), 25);
    public static readonly RestaurantTag Hookah = new(nameof(Hookah), 26);
    public static readonly RestaurantTag SummerTerrace = new(nameof(SummerTerrace), 27);
    public static readonly RestaurantTag NewYearParty = new(nameof(NewYearParty), 28);
    public static readonly RestaurantTag TwentyFourHours = new(nameof(TwentyFourHours), 29);
    public static readonly RestaurantTag KidsRoom = new(nameof(KidsRoom), 30);
    public static readonly RestaurantTag FuneralCafe = new(nameof(FuneralCafe), 31);
    public static readonly RestaurantTag Delivery = new(nameof(Delivery), 32);

    private RestaurantTag(string name, int value) : base(name, value) { }

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
