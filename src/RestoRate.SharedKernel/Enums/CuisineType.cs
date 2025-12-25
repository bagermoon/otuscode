using Ardalis.SmartEnum;

namespace RestoRate.SharedKernel.Enums
{
    public class CuisineType : SmartEnum<CuisineType>
    {
        public string RussianName { get; }

        // Европейская
        public static readonly CuisineType Italian = new(nameof(Italian), 1, "Итальянская");
        public static readonly CuisineType French = new(nameof(French), 2, "Французская");
        public static readonly CuisineType Spanish = new(nameof(Spanish), 3, "Испанская");
        public static readonly CuisineType Greek = new(nameof(Greek), 4, "Греческая");
        public static readonly CuisineType German = new(nameof(German), 5, "Немецкая");
        public static readonly CuisineType English = new(nameof(English), 6, "Английская");
        public static readonly CuisineType Irish = new(nameof(Irish), 7, "Ирландская");
        public static readonly CuisineType Portuguese = new(nameof(Portuguese), 8, "Португальская");
        public static readonly CuisineType Austrian = new(nameof(Austrian), 9, "Австрийская");
        public static readonly CuisineType Swiss = new(nameof(Swiss), 10, "Швейцарская");
        public static readonly CuisineType Belgian = new(nameof(Belgian), 11, "Бельгийская");
        public static readonly CuisineType Dutch = new(nameof(Dutch), 12, "Голландская");
        public static readonly CuisineType Polish = new(nameof(Polish), 13, "Польская");
        public static readonly CuisineType Czech = new(nameof(Czech), 14, "Чешская");
        public static readonly CuisineType Hungarian = new(nameof(Hungarian), 15, "Венгерская");
        public static readonly CuisineType Romanian = new(nameof(Romanian), 16, "Румынская");
        public static readonly CuisineType Bulgarian = new(nameof(Bulgarian), 17, "Болгарская");
        public static readonly CuisineType Croatian = new(nameof(Croatian), 18, "Хорватская");
        public static readonly CuisineType Serbian = new(nameof(Serbian), 19, "Сербская");
        public static readonly CuisineType Scandinavian = new(nameof(Scandinavian), 20, "Скандинавская");

        // Азиатская
        public static readonly CuisineType Chinese = new(nameof(Chinese), 21, "Китайская");
        public static readonly CuisineType Japanese = new(nameof(Japanese), 22, "Японская");
        public static readonly CuisineType Korean = new(nameof(Korean), 23, "Корейская");
        public static readonly CuisineType Thai = new(nameof(Thai), 24, "Тайская");
        public static readonly CuisineType Vietnamese = new(nameof(Vietnamese), 25, "Вьетнамская");
        public static readonly CuisineType Indian = new(nameof(Indian), 26, "Индийская");
        public static readonly CuisineType Pakistani = new(nameof(Pakistani), 27, "Пакистанская");
        public static readonly CuisineType Indonesian = new(nameof(Indonesian), 28, "Индонезийская");
        public static readonly CuisineType Malaysian = new(nameof(Malaysian), 29, "Малайзийская");
        public static readonly CuisineType Filipino = new(nameof(Filipino), 30, "Филиппинская");
        public static readonly CuisineType Singaporean = new(nameof(Singaporean), 31, "Сингапурская");
        public static readonly CuisineType Burmese = new(nameof(Burmese), 32, "Бирманская");
        public static readonly CuisineType Cambodian = new(nameof(Cambodian), 33, "Камбоджийская");
        public static readonly CuisineType Laotian = new(nameof(Laotian), 34, "Лаосская");
        public static readonly CuisineType Nepalese = new(nameof(Nepalese), 35, "Непальская");
        public static readonly CuisineType Tibetan = new(nameof(Tibetan), 36, "Тибетская");
        public static readonly CuisineType Mongolian = new(nameof(Mongolian), 37, "Монгольская");

        // Ближний Восток
        public static readonly CuisineType Lebanese = new(nameof(Lebanese), 38, "Ливанская");
        public static readonly CuisineType Turkish = new(nameof(Turkish), 39, "Турецкая");
        public static readonly CuisineType Israeli = new(nameof(Israeli), 40, "Израильская");
        public static readonly CuisineType Persian = new(nameof(Persian), 41, "Персидская");
        public static readonly CuisineType Syrian = new(nameof(Syrian), 42, "Сирийская");
        public static readonly CuisineType Jordanian = new(nameof(Jordanian), 43, "Иорданская");
        public static readonly CuisineType Yemeni = new(nameof(Yemeni), 44, "Йеменская");
        public static readonly CuisineType Iraqi = new(nameof(Iraqi), 45, "Иракская");
        public static readonly CuisineType Saudi = new(nameof(Saudi), 46, "Саудовская");
        public static readonly CuisineType Emirati = new(nameof(Emirati), 47, "Эмиратская");

        // Кавказская / СНГ
        public static readonly CuisineType Georgian = new(nameof(Georgian), 48, "Грузинская");
        public static readonly CuisineType Armenian = new(nameof(Armenian), 49, "Армянская");
        public static readonly CuisineType Azerbaijani = new(nameof(Azerbaijani), 50, "Азербайджанская");
        public static readonly CuisineType Russian = new(nameof(Russian), 51, "Русская");
        public static readonly CuisineType Ukrainian = new(nameof(Ukrainian), 52, "Украинская");
        public static readonly CuisineType Belarusian = new(nameof(Belarusian), 53, "Белорусская");
        public static readonly CuisineType Kazakh = new(nameof(Kazakh), 54, "Казахская");
        public static readonly CuisineType Uzbek = new(nameof(Uzbek), 55, "Узбекская");
        public static readonly CuisineType Tajik = new(nameof(Tajik), 56, "Таджикская");
        public static readonly CuisineType Kyrgyz = new(nameof(Kyrgyz), 57, "Киргизская");
        public static readonly CuisineType Turkmen = new(nameof(Turkmen), 58, "Туркменская");
        public static readonly CuisineType Tatar = new(nameof(Tatar), 59, "Татарская");
        public static readonly CuisineType Chechen = new(nameof(Chechen), 60, "Чеченская");
        public static readonly CuisineType Ossetian = new(nameof(Ossetian), 61, "Осетинская");

        // Африканская
        public static readonly CuisineType Moroccan = new(nameof(Moroccan), 62, "Марокканская");
        public static readonly CuisineType Egyptian = new(nameof(Egyptian), 63, "Египетская");
        public static readonly CuisineType Ethiopian = new(nameof(Ethiopian), 64, "Эфиопская");
        public static readonly CuisineType Nigerian = new(nameof(Nigerian), 65, "Нигерийская");
        public static readonly CuisineType SouthAfrican = new(nameof(SouthAfrican), 66, "Южноафриканская");
        public static readonly CuisineType Tunisian = new(nameof(Tunisian), 67, "Тунисская");
        public static readonly CuisineType Algerian = new(nameof(Algerian), 68, "Алжирская");
        public static readonly CuisineType Kenyan = new(nameof(Kenyan), 69, "Кенийская");

        // Американская
        public static readonly CuisineType American = new(nameof(American), 70, "Американская");
        public static readonly CuisineType Mexican = new(nameof(Mexican), 71, "Мексиканская");
        public static readonly CuisineType TexMex = new(nameof(TexMex), 72, "Текс-Мекс");
        public static readonly CuisineType Brazilian = new(nameof(Brazilian), 73, "Бразильская");
        public static readonly CuisineType Argentinian = new(nameof(Argentinian), 74, "Аргентинская");
        public static readonly CuisineType Peruvian = new(nameof(Peruvian), 75, "Перуанская");
        public static readonly CuisineType Colombian = new(nameof(Colombian), 76, "Колумбийская");
        public static readonly CuisineType Chilean = new(nameof(Chilean), 77, "Чилийская");
        public static readonly CuisineType Cuban = new(nameof(Cuban), 78, "Кубинская");
        public static readonly CuisineType Caribbean = new(nameof(Caribbean), 79, "Карибская");
        public static readonly CuisineType Canadian = new(nameof(Canadian), 80, "Канадская");

        // Океания
        public static readonly CuisineType Australian = new(nameof(Australian), 81, "Австралийская");
        public static readonly CuisineType NewZealand = new(nameof(NewZealand), 82, "Новозеландская");
        public static readonly CuisineType Hawaiian = new(nameof(Hawaiian), 83, "Гавайская");

        public CuisineType(string name, int value, string russianName) : base(name, value)
        {
            RussianName = russianName;
        }
    }
}
