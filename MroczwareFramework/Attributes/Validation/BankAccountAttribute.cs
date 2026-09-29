using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MroczwareFramework.Attributes.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class BankAccountAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success!;

            if (value is not string bankAccount)
                return new ValidationResult($"Nie udana próba parsowania");

            if (bankAccount.Where(char.IsDigit).ToArray().Length != 26)
                return new ValidationResult($"Numer konta bankowego powinien składać się z 26 cyfr");

            bankAccount = Regex.Replace(bankAccount, @"\s", "").ToUpper(); // remove empty space & convert all uppercase

            if (Regex.IsMatch(bankAccount, @"\W")) // contains chars other than (a-zA-Z0-9)
                return new ValidationResult("Numer IBAN zawiera niepoprawne znaki");

            if (!Regex.IsMatch(bankAccount, @"^\D\D\d\d.+")) // first chars are letter letter digit digit
                return new ValidationResult("Nie poprawny format numeru IBAN");

            if (Regex.IsMatch(bankAccount, @"^\D\D00.+|^\D\D01.+|^\D\D99.+")) // check digit are 00 or 01 or 99
                return new ValidationResult("Cyfry kontrolne IBAN są nieprawidłowe");

            string countryCode = bankAccount.Substring(0, 2);

            IBANData? currentIbanData = (from id in IBANList()
                                         where id.CountryCode == countryCode
                                         select id).FirstOrDefault();

            if (currentIbanData is null) // test if country respected
                return new ValidationResult("Number IBAN dla danego kraju nie jest obecnie dostępny");

            if (bankAccount.Length != currentIbanData.Lenght) // fits length to country
                return new ValidationResult(string.Format("IBAN dla {0} powinien mieć {1} znaków długości", countryCode, currentIbanData.Lenght));

            if (!Regex.IsMatch(bankAccount.Remove(0, 4), currentIbanData.RegexStructure))
                return new ValidationResult("Specyficzna dla kraju struktura IBAN jest nieprawidłowa.");

            // ******* from wikipedia.org
            //The checksum is a basic ISO 7064 mod 97-10 calculation where the remainder must equal 1.
            //To validate the checksum:
            //1- Check that the total IBAN length is correct as per the country. If not, the IBAN is invalid. 
            //2- Move the four initial characters to the end of the string. 
            //3- Replace each letter in the string with two digits, thereby expanding the string, where A=10, B=11, ..., Z=35. 
            //4- Interpret the string as a decimal integer and compute the remainder of that number on division by 97. 
            //The IBAN number can only be valid if the remainder is 1.
            string modifiedIban = bankAccount.ToUpper().Substring(4) + bankAccount.Substring(0, 4);
            modifiedIban = Regex.Replace(modifiedIban, @"\D", m => (m.Value[0] - 55).ToString());

            int remainer = 0;
            while (modifiedIban.Length >= 7)
            {
                remainer = int.Parse(remainer + modifiedIban.Substring(0, 7)) % 97;
                modifiedIban = modifiedIban.Substring(7);
            }
            remainer = int.Parse(remainer + modifiedIban) % 97;

            if (remainer != 1)
                return new ValidationResult("IBAN jest nieprawidłowy");

            System.Reflection.PropertyInfo? property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
            if (property is not null && property.CanWrite)
                property.SetValue(validationContext.ObjectInstance, bankAccount);

            return ValidationResult.Success!;
        }

        /// <summary>
        /// If you develop another code/algorithm you can use this list
        /// check updates @ http://www.tbg5-finance.org/checkiban.js
        /// </summary>
        private static IEnumerable<IBANData> IBANList()
        {
            List<IBANData> newList =
                [
                        new IBANData("AD", 24, @"\d{8}[a-zA-Z0-9]{12}", false, "AD1200012030200359100100"),
                        new IBANData("AL", 28, @"\d{8}[a-zA-Z0-9]{16}", false, "AL47212110090000000235698741"),
                        new IBANData("AT", 20, @"\d{16}", true, "AT611904300234573201"),
                        new IBANData("BA", 20, @"\d{16}", false, "BA391290079401028494"),
                        new IBANData("BE", 16, @"\d{12}", true, "BE68539007547034"),
                        new IBANData("BG", 22, @"[A-Z]{4}\d{6}[a-zA-Z0-9]{8}", true, "BG80BNBG96611020345678"),
                        new IBANData("CH", 21, @"\d{5}[a-zA-Z0-9]{12}", false, "CH9300762011623852957"),
                        new IBANData("CY", 28, @"\d{8}[a-zA-Z0-9]{16}", true, "CY17002001280000001200527600"),
                        new IBANData("CZ", 24, @"\d{20}", true, "CZ6508000000192000145399"),
                        new IBANData("DE", 22, @"\d{18}", true, "DE89370400440532013000"),
                        new IBANData("DK", 18, @"\d{14}", true, "DK5000400440116243"),
                        new IBANData("EE", 20, @"\d{16}", true, "EE382200221020145685"),
                        new IBANData("ES", 24, @"\d{20}", true, "ES9121000418450200051332"),
                        new IBANData("FI", 18, @"\d{14}", true, "FI2112345600000785"),
                        new IBANData("FO", 18, @"\d{14}", false, "FO6264600001631634"),
                        new IBANData("FR", 27, @"\d{10}[a-zA-Z0-9]{11}\d\d", true, "FR1420041010050500013M02606"),
                        new IBANData("GB", 22, @"[A-Z]{4}\d{14}", true, "GB29NWBK60161331926819"),
                        new IBANData("GI", 23, @"[A-Z]{4}[a-zA-Z0-9]{15}", true, "GI75NWBK000000007099453"),
                        new IBANData("GL", 18, @"\d{14}", false, "GL8964710001000206"),
                        new IBANData("GR", 27, @"\d{7}[a-zA-Z0-9]{16}", true, "GR1601101250000000012300695"),
                        new IBANData("HR", 21, @"\d{17}", false, "HR1210010051863000160"),
                        new IBANData("HU", 28, @"\d{24}", true, "HU42117730161111101800000000"),
                        new IBANData("IE", 22, @"[A-Z]{4}\d{14}", true, "IE29AIBK93115212345678"),
                        new IBANData("IL", 23, @"\d{19}", false, "IL620108000000099999999"),
                        new IBANData("IS", 26, @"\d{22}", true, "IS140159260076545510730339"),
                        new IBANData("IT", 27, @"[A-Z]\d{10}[a-zA-Z0-9]{12}", true, "IT60X0542811101000000123456"),
                        new IBANData("LB", 28, @"\d{4}[a-zA-Z0-9]{20}", false, "LB62099900000001001901229114"),
                        new IBANData("LI", 21, @"\d{5}[a-zA-Z0-9]{12}", true, "LI21088100002324013AA"),
                        new IBANData("LT", 20, @"\d{16}", true, "LT121000011101001000"),
                        new IBANData("LU", 20, @"\d{3}[a-zA-Z0-9]{13}", true, "LU280019400644750000"),
                        new IBANData("LV", 21, @"[A-Z]{4}[a-zA-Z0-9]{13}", true, "LV80BANK0000435195001"),
                        new IBANData("MC", 27, @"\d{10}[a-zA-Z0-9]{11}\d\d", true, "MC1112739000700011111000h79"),
                        new IBANData("ME", 22, @"\d{18}", false, "ME25505000012345678951"),
                        new IBANData("MK", 19, @"\d{3}[a-zA-Z0-9]{10}\d\d", false, "MK07300000000042425"),
                        new IBANData("MT", 31, @"[A-Z]{4}\d{5}[a-zA-Z0-9]{18}", true, "MT84MALT011000012345MTLCAST001S"),
                        new IBANData("MU", 30, @"[A-Z]{4}\d{19}[A-Z]{3}", false, "MU17BOMM0101101030300200000MUR"),
                        new IBANData("NL", 18, @"[A-Z]{4}\d{10}", true, "NL91ABNA0417164300"),
                        new IBANData("NO", 15, @"\d{11}", true, "NO9386011117947"),
                        new IBANData("PL", 28, @"\d{8}[a-zA-Z0-9]{16}", true, "PL27114020040000300201355387"),
                        new IBANData("PT", 25, @"\d{21}", true, "PT50000201231234567890154"),
                        new IBANData("RO", 24, @"[A-Z]{4}[a-zA-Z0-9]{16}", true, "RO49AAAA1B31007593840000"),
                        new IBANData("RS", 22, @"\d{18}", false, "RS35260005601001611379"),
                        new IBANData("SA", 24, @"\d{2}[a-zA-Z0-9]{18}", false, "SA0380000000608010167519"),
                        new IBANData("SE", 24, @"\d{20}", true, "SE4550000000058398257466"),
                        new IBANData("SI", 19, @"\d{15}", true, "SI56191000000123438"),
                        new IBANData("SK", 24, @"\d{20}", true, "SK3112000000198742637541"),
                        new IBANData("SM", 27, @"[A-Z]\d{10}[a-zA-Z0-9]{12}", false, "SM86U0322509800000000270100"),
                        new IBANData("TN", 24, @"\d{20}", false, "TN5914207207100707129648"),
                        new IBANData("TR", 26, @"\d{5}[a-zA-Z0-9]{17}", false, "TR330006100519786457841326")
                    ];

            return newList;
        }
    }

    /// <summary>
    /// IBAN data structure for storing necessary data
    /// </summary>
    public class IBANData
    {
        public string CountryCode;
        public int Lenght;
        public string RegexStructure;
        public bool IsEU924;
        public string Sample;

        public IBANData(string countryCode, int lenght, string regexStructure, bool isEU924, string sample)
        {
            CountryCode = countryCode;
            Lenght = lenght;
            RegexStructure = regexStructure;
            IsEU924 = isEU924;
            Sample = sample;
        }
    }
}
