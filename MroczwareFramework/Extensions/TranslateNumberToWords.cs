namespace MroczwareFramework.Extensions
{
    public static class TranslateNumberToWords
    {
        public static string TranslatePriceToWords(this decimal price)
        {
            var valueInArray = price.ToString().Replace(',', '.').Split('.');
            var result = valueInArray[0].TranslateValueToWords() + " złotych ";
            if (valueInArray.Length > 1 && !valueInArray[1].Equals("00") && !valueInArray[1].Equals("0")
                && !valueInArray[1].Equals(""))
            {
                result += " i " + valueInArray[1].TranslateValueToWords();
                if (Convert.ToInt32(valueInArray[1]) > 4)
                    result += " groszy";
                else
                    result += " grosze";
            }

            return result;
        }

        private static string TranslateValueToWords(this string valueInString)
        {
            var result = "";
            var moneyString = valueInString;
            var do6 = true;
            var do4 = true;
            var do2 = true;

            if (moneyString.Length > 6)
            {
                switch (moneyString[moneyString.Length - 7])
                {
                    case '1':
                        result += "jeden milion ";
                        break;
                    case '2':
                        result += "dwa miliony ";
                        break;
                    case '3':
                        result += "trzy miliony ";
                        break;
                    case '4':
                        result += "cztery miliony ";
                        break;
                    case '5':
                        result += "pięć milionów ";
                        break;
                    case '6':
                        result += "sześć milionów ";
                        break;
                    case '7':
                        result += "siedem milionów ";
                        break;
                    case '8':
                        result += "osiem milionów ";
                        break;
                    case '9':
                        result += "dziewięć milionów ";
                        break;
                }
            }
            if (moneyString.Length > 5)
            {
                switch (moneyString[moneyString.Length - 6])
                {
                    case '1':
                        result += "sto ";
                        break;
                    case '2':
                        result += "dwieście ";
                        break;
                    case '3':
                        result += "trzysta ";
                        break;
                    case '4':
                        result += "czterysta ";
                        break;
                    case '5':
                        result += "pięćset ";
                        break;
                    case '6':
                        result += "sześćset ";
                        break;
                    case '7':
                        result += "siedemset ";
                        break;
                    case '8':
                        result += "osiemset ";
                        break;
                    case '9':
                        result += "dziewięćset ";
                        break;
                    case '0':
                        do6 = false;
                        break;
                }
            }
            if (moneyString.Length > 4)
            {
                switch (moneyString[moneyString.Length - 5])
                {
                    case '1':
                        do4 = false;
                        switch (moneyString[moneyString.Length - 4])
                        {
                            case '0':
                                result += "dziesięć ";
                                break;
                            case '1':
                                result += "jedenaście ";
                                break;
                            case '2':
                                result += "dwanaście ";
                                break;
                            case '3':
                                result += "trzynaście ";
                                break;
                            case '4':
                                result += "czternaście ";
                                break;
                            case '5':
                                result += "piętnaście ";
                                break;
                            case '6':
                                result += "szesnaście ";
                                break;
                            case '7':
                                result += "siedemnaście ";
                                break;
                            case '8':
                                result += "osiemnaście ";
                                break;
                            case '9':
                                result += "dziewiętnaście ";
                                break;
                        }
                        result += "tysięcy ";
                        break;
                    case '2':
                        result += "dwadzieścia ";
                        break;
                    case '3':
                        result += "trzydzieści ";
                        break;
                    case '4':
                        result += "czterdzieści ";
                        break;
                    case '5':
                        result += "pięćdziesiąt ";
                        break;
                    case '6':
                        result += "sześćdziesiąt ";
                        break;
                    case '7':
                        result += "siedemdziesiąt ";
                        break;
                    case '8':
                        result += "osiemdziesiąt ";
                        break;
                    case '9':
                        result += "dziewięćdziesiąt ";
                        break;
                }
            }
            if (moneyString.Length > 3 && do4)
            {
                switch (moneyString[moneyString.Length - 4])
                {
                    case '1':
                        result += "jeden ";
                        if (moneyString.Length > 4) result += " " + "tysięcy ";
                        else result += " " + "tysiąc ";
                        break;
                    case '2':
                        result += "dwa tysiące ";
                        break;
                    case '3':
                        result += "trzy tysiące ";
                        break;
                    case '4':
                        result += "cztery tysiące ";
                        break;
                    case '5':
                        result += "pięć tysięcy ";
                        break;
                    case '6':
                        result += "sześć tysięcy ";
                        break;
                    case '7':
                        result += "siedem tysięcy ";
                        break;
                    case '8':
                        result += "osiem tysięcy ";
                        break;
                    case '9':
                        result += "dziewięć tysięcy ";
                        break;
                    case '0':
                        if (do6) result += "tysięcy ";
                        break;
                }
            }

            if (moneyString.Length > 2)
            {
                switch (moneyString[moneyString.Length - 3])
                {
                    case '1':
                        result += "sto ";
                        break;
                    case '2':
                        result += "dwieście ";
                        break;
                    case '3':
                        result += "trzysta ";
                        break;
                    case '4':
                        result += "czterysta ";
                        break;
                    case '5':
                        result += "pięćset ";
                        break;
                    case '6':
                        result += "sześćset ";
                        break;
                    case '7':
                        result += "siedemset ";
                        break;
                    case '8':
                        result += "osiemset ";
                        break;
                    case '9':
                        result += "dziewięćset ";
                        break;
                }
            }

            if (moneyString.Length > 1)
            {
                switch (moneyString[moneyString.Length - 2])
                {
                    case '1':
                        do2 = false;
                        switch (moneyString[moneyString.Length - 1])
                        {
                            case '0':
                                result += "dziesięć ";
                                break;
                            case '1':
                                result += "jedenaście ";
                                break;
                            case '2':
                                result += "dwanaście ";
                                break;
                            case '3':
                                result += "trzynaście ";
                                break;
                            case '4':
                                result += "czternaście ";
                                break;
                            case '5':
                                result += "piętnaście ";
                                break;
                            case '6':
                                result += "szesnaście ";
                                break;
                            case '7':
                                result += "siedemnaście ";
                                break;
                            case '8':
                                result += "osiemnaście ";
                                break;
                            case '9':
                                result += "dziewiętnaście ";
                                break;
                        }
                        break;
                    case '2':
                        result += "dwadzieścia ";
                        break;
                    case '3':
                        result += "trzydzieści ";
                        break;
                    case '4':
                        result += "czterdzieści ";
                        break;
                    case '5':
                        result += "pięćdziesiąt ";
                        break;
                    case '6':
                        result += "sześćdziesiąt ";
                        break;
                    case '7':
                        result += "siedemdziesiąt ";
                        break;
                    case '8':
                        result += "osiemdziesiąt ";
                        break;
                    case '9':
                        result += "dziewięćdziesiąt ";
                        break;
                }
            }

            if (do2)
            {
                switch (moneyString[moneyString.Length - 1])
                {
                    case '0':
                        result += "zero ";
                        break;
                    case '1':
                        result += "jeden ";
                        break;
                    case '2':
                        result += "dwa ";
                        break;
                    case '3':
                        result += "trzy ";
                        break;
                    case '4':
                        result += "cztery ";
                        break;
                    case '5':
                        result += "pięć ";
                        break;
                    case '6':
                        result += "sześć ";
                        break;
                    case '7':
                        result += "siedem ";
                        break;
                    case '8':
                        result += "osiem ";
                        break;
                    case '9':
                        result += "dziewięć ";
                        break;
                }
            }
            return result;

        }
    }
}
