using Ardalis.SmartEnum;

namespace RestoRate.SharedKernel.Enums
{
    public class CuisineType : SmartEnum<CuisineType>
    {
        // Европейская
        public static readonly CuisineType Italian = new(nameof(Italian), 1);
        public static readonly CuisineType French = new(nameof(French), 2);
        public static readonly CuisineType Spanish = new(nameof(Spanish), 3);
        public static readonly CuisineType Greek = new(nameof(Greek), 4);
        public static readonly CuisineType German = new(nameof(German), 5);
        public static readonly CuisineType English = new(nameof(English), 6);
        public static readonly CuisineType Irish = new(nameof(Irish), 7);
        public static readonly CuisineType Portuguese = new(nameof(Portuguese), 8);
        public static readonly CuisineType Austrian = new(nameof(Austrian), 9);
        public static readonly CuisineType Swiss = new(nameof(Swiss), 10);
        public static readonly CuisineType Belgian = new(nameof(Belgian), 11);
        public static readonly CuisineType Dutch = new(nameof(Dutch), 12);
        public static readonly CuisineType Polish = new(nameof(Polish), 13);
        public static readonly CuisineType Czech = new(nameof(Czech), 14);
        public static readonly CuisineType Hungarian = new(nameof(Hungarian), 15);
        public static readonly CuisineType Romanian = new(nameof(Romanian), 16);
        public static readonly CuisineType Bulgarian = new(nameof(Bulgarian), 17);
        public static readonly CuisineType Croatian = new(nameof(Croatian), 18);
        public static readonly CuisineType Serbian = new(nameof(Serbian), 19);
        public static readonly CuisineType Scandinavian = new(nameof(Scandinavian), 20);

        // Азиатская
        public static readonly CuisineType Chinese = new(nameof(Chinese), 21);
        public static readonly CuisineType Japanese = new(nameof(Japanese), 22);
        public static readonly CuisineType Korean = new(nameof(Korean), 23);
        public static readonly CuisineType Thai = new(nameof(Thai), 24);
        public static readonly CuisineType Vietnamese = new(nameof(Vietnamese), 25);
        public static readonly CuisineType Indian = new(nameof(Indian), 26);
        public static readonly CuisineType Pakistani = new(nameof(Pakistani), 27);
        public static readonly CuisineType Indonesian = new(nameof(Indonesian), 28);
        public static readonly CuisineType Malaysian = new(nameof(Malaysian), 29);
        public static readonly CuisineType Filipino = new(nameof(Filipino), 30);
        public static readonly CuisineType Singaporean = new(nameof(Singaporean), 31);
        public static readonly CuisineType Burmese = new(nameof(Burmese), 32);
        public static readonly CuisineType Cambodian = new(nameof(Cambodian), 33);
        public static readonly CuisineType Laotian = new(nameof(Laotian), 34);
        public static readonly CuisineType Nepalese = new(nameof(Nepalese), 35);
        public static readonly CuisineType Tibetan = new(nameof(Tibetan), 36);
        public static readonly CuisineType Mongolian = new(nameof(Mongolian), 37);

        // Ближний Восток
        public static readonly CuisineType Lebanese = new(nameof(Lebanese), 38);
        public static readonly CuisineType Turkish = new(nameof(Turkish), 39);
        public static readonly CuisineType Israeli = new(nameof(Israeli), 40);
        public static readonly CuisineType Persian = new(nameof(Persian), 41);
        public static readonly CuisineType Syrian = new(nameof(Syrian), 42);
        public static readonly CuisineType Jordanian = new(nameof(Jordanian), 43);
        public static readonly CuisineType Yemeni = new(nameof(Yemeni), 44);
        public static readonly CuisineType Iraqi = new(nameof(Iraqi), 45);
        public static readonly CuisineType Saudi = new(nameof(Saudi), 46);
        public static readonly CuisineType Emirati = new(nameof(Emirati), 47);

        // Кавказская / СНГ
        public static readonly CuisineType Georgian = new(nameof(Georgian), 48);
        public static readonly CuisineType Armenian = new(nameof(Armenian), 49);
        public static readonly CuisineType Azerbaijani = new(nameof(Azerbaijani), 50);
        public static readonly CuisineType Russian = new(nameof(Russian), 51);
        public static readonly CuisineType Ukrainian = new(nameof(Ukrainian), 52);
        public static readonly CuisineType Belarusian = new(nameof(Belarusian), 53);
        public static readonly CuisineType Kazakh = new(nameof(Kazakh), 54);
        public static readonly CuisineType Uzbek = new(nameof(Uzbek), 55);
        public static readonly CuisineType Tajik = new(nameof(Tajik), 56);
        public static readonly CuisineType Kyrgyz = new(nameof(Kyrgyz), 57);
        public static readonly CuisineType Turkmen = new(nameof(Turkmen), 58);
        public static readonly CuisineType Tatar = new(nameof(Tatar), 59);
        public static readonly CuisineType Chechen = new(nameof(Chechen), 60);
        public static readonly CuisineType Ossetian = new(nameof(Ossetian), 61);

        // Африканская
        public static readonly CuisineType Moroccan = new(nameof(Moroccan), 62);
        public static readonly CuisineType Egyptian = new(nameof(Egyptian), 63);
        public static readonly CuisineType Ethiopian = new(nameof(Ethiopian), 64);
        public static readonly CuisineType Nigerian = new(nameof(Nigerian), 65);
        public static readonly CuisineType SouthAfrican = new(nameof(SouthAfrican), 66);
        public static readonly CuisineType Tunisian = new(nameof(Tunisian), 67);
        public static readonly CuisineType Algerian = new(nameof(Algerian), 68);
        public static readonly CuisineType Kenyan = new(nameof(Kenyan), 69);

        // Американская
        public static readonly CuisineType American = new(nameof(American), 70);
        public static readonly CuisineType Mexican = new(nameof(Mexican), 71);
        public static readonly CuisineType TexMex = new(nameof(TexMex), 72);
        public static readonly CuisineType Brazilian = new(nameof(Brazilian), 73);
        public static readonly CuisineType Argentinian = new(nameof(Argentinian), 74);
        public static readonly CuisineType Peruvian = new(nameof(Peruvian), 75);
        public static readonly CuisineType Colombian = new(nameof(Colombian), 76);
        public static readonly CuisineType Chilean = new(nameof(Chilean), 77);
        public static readonly CuisineType Cuban = new(nameof(Cuban), 78);
        public static readonly CuisineType Caribbean = new(nameof(Caribbean), 79);
        public static readonly CuisineType Canadian = new(nameof(Canadian), 80);

        // Океания
        public static readonly CuisineType Australian = new(nameof(Australian), 81);
        public static readonly CuisineType NewZealand = new(nameof(NewZealand), 82);
        public static readonly CuisineType Hawaiian = new(nameof(Hawaiian), 83);

        public CuisineType(string name, int value) : base(name, value) { }
    }
}
