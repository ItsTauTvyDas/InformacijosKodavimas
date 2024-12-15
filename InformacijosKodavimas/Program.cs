using System.Diagnostics;

namespace InformacijosKodavimas
{
    public class Program
    {
        private static EncryptionType _selectedEncodingType = EncryptionType.E_AES;
        private static readonly Database _database = new();
        private static string? _newColumnValue = null;
        private static string? _cryptKey = null;
        private static Encryption? _encryption;
        private static Decryption? _decryption;

        public static void Main()
        {
            ConnectToDatabase();
        }

        private static void ChooseEncryption()
        {
            _cryptKey = null;
            Console.Clear();
            WriteHeader("Antras žingsnis: Pasirinkite koduotę");
            Console.WriteLine("Pasirinkite norimą koduotę su klavišų rodyklėmis (aukštyn/žemyn) ir ENTER pasirinkimui.");
            _selectedEncodingType = (EncryptionType)CreateMenuSelector(0, new List<string> {
                "AES", "3DES"
            });
            StartEncryption();
        }

        private static void WriteHeader(string text)
        {
            MiddleColorWrite("-------------------------- ", "Užkodavimo/Atkodavimo Programa", " --------------------------", ConsoleColor.Magenta);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" Ši programa leidžia užkoduoti ir atkoduoti informaciją naudojant pasirinktą metodą.");
            Console.WriteLine("                       Palaikomi kodavimo metodai: AES ir 3DES.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("------------------------------------------------------------------------------------");
            Write(text, ConsoleColor.Yellow, true);
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine();
        }

        private static void StartEncryption(int? selectedAction = null)
        {
            Console.Clear();
            WriteHeader("Trečias žingsnis: Įrašo reikšmės užkodavimas ir atkodavimas");
            var value = _database.GetRowColumnText();
            if (value == null)
            {
                Write("Kažkas nutko... Nepavyko gauti stulpelio reikšmės, bandykite pasirinkti kitą stulpelį! Paspauskite bet kokį klavišą norint tęsti.", ConsoleColor.Red, true);
                Console.ReadKey(true);
                ConnectToDatabase(true);
                return;
            }
            _newColumnValue ??= value.Length == 0 ? null : value;
            MiddleColorWrite("Pasirinkta koduotė:           ", _selectedEncodingType.ToString()[2..], null, ConsoleColor.Blue);
            MiddleColorWrite("Įrašyta DB stulpelio reikšmė: ", value.Length == 0 ? "<null?>" : value, null, value.Length == 0 ? ConsoleColor.Red : ConsoleColor.Blue);
            MiddleColorWrite("Nauja reikšmė įvedimui:       ", _newColumnValue ?? "<nėra>", null, _newColumnValue == null ? ConsoleColor.Red : ConsoleColor.Green);
            MiddleColorWrite("Užkodavimo/Atkodavimo raktas: ", _cryptKey ?? "<nėra>", null, _cryptKey == null ? ConsoleColor.Red : ConsoleColor.Green);
            var list = new List<string> {
                "Atpažinti koduotę", // 0
                "Užkoduoti", // 1
                "Atkoduoti", // 2
                "Keisti (at)kodavimo tipą", // 3
                "Keisti duomenų bazę", // 4
                "Keisti slaptą raktą", // 5
                "Keisti stulpelio naują reikšmę" // 6
            };
            if (_newColumnValue != null && _newColumnValue != value)
                list.Add("Įrašyti naują stulpelio reikšmę į duomenų bazę"); // 7
            var selected = selectedAction ?? CreateMenuSelector(0, list);
            if (selected <= 2 && (_cryptKey == null || _newColumnValue == null || _encryption == null || _decryption == null))
            {
                StartEncryption(_newColumnValue == null ? 6 : 5);
                return;
            }

            switch (selected)
            {
                case 0: // Atpažinti koduotę
                    try
                    {
                        _decryption.DecryptAES(_newColumnValue);
                        _selectedEncodingType = EncryptionType.E_AES;
                    }
                    catch
                    {
                        try
                        {
                            _decryption.Decrypt3DES(_newColumnValue);
                            _selectedEncodingType = EncryptionType.E_3DES;
                        }
                        catch
                        {
                            Console.WriteLine();
                            Write("Nepavyko atkoduoti (AES ar 3DES), gal neteisingas raktas?", ConsoleColor.Red, true);
                            Console.ReadKey(true);
                            break;
                        }
                    }
                    Console.WriteLine();
                    MiddleColorWrite("Koduotė sėkmingai atpažinta! Nustatoma į ", _selectedEncodingType.ToString(), ", paspauskite bet kokį klavišą norint tęsti.", ConsoleColor.Green);
                    Console.ReadKey(true);
                    break;
                case 1: // Užkoduoti
                    try
                    {
                        _newColumnValue = _encryption.Encrypt(_newColumnValue, _selectedEncodingType);
                    }
                    catch (Exception exception)
                    {
                        MiddleColorWrite("Įvyko klaida užkoduojant! ", exception.Message, null, ConsoleColor.Red);
                        Console.ReadKey(true);
                    }
                    break;
                case 2: // Atkoduoti
                    try
                    {
                        _newColumnValue = _decryption.Decrypt(_newColumnValue, _selectedEncodingType);
                    }
                    catch (Exception exception)
                    {
                        MiddleColorWrite("Atkoduoti nepavyko! ", exception.Message, null, ConsoleColor.Red);
                        Console.ReadKey(true);
                    }
                    break;
                case 3: // Keisti (at)kodavimo tipą
                    ChooseEncryption();
                    break;
                case 4: // Keisti duomenų bazę
                    ConnectToDatabase();
                    break;
                case 5: // Keisti slaptą raktą
                    Console.WriteLine();
                    ReadLine("Įveskite užkodavimo raktą", ref _cryptKey);
                    try
                    {
                        _encryption = new(_cryptKey ?? "");
                        _decryption = new(_cryptKey ?? "");
                    }
                    catch (Exception exception)
                    {
                        _cryptKey = null;
                        Console.SetCursorPosition(3, Console.CursorTop - 1);
                        Write(exception.Message, ConsoleColor.Red, true);
                        Console.ReadKey(true);
                    }
                    break;
                case 6: // Keisti stulpelio naują reikšmę
                    Console.WriteLine();
                    ReadLine($"Įveskite naują stulperio reikšmę (senoji - {_newColumnValue ?? "<jokia>"})", ref _newColumnValue);
                    break;
                case 7: // Įrašyti naują stulpelio reikšmę į duomenų bazę
                    if (_newColumnValue == null)
                    {
                        StartEncryption(6);
                        return;
                    }
                    _database.SetRowColumnText(_newColumnValue);
                    break;
            }
            StartEncryption();
        }

        private static void ConnectToDatabase(bool skipLogin = false)
        {
            Console.Clear();
            WriteHeader("Pirmas žingsnis: Prisijungimas prie duomenų bazės (MS-SQL Serveris).");
            Console.WriteLine("Paspauskite ENTER norint automatiškai įvesti mėlinai parašytus duomenis!");
            Console.WriteLine();
            if (!skipLogin)
            {
                ReadLine("Įveskite duomenų šaltinį / serveri", ref _database.Server);
                ReadLine("Įveskite duomenų bazės pavadinimą", ref _database.DatabaseName);
                ReadLine("Įveskite vartotojo vardą", ref _database.User);
                ReadLine("Įveskite vartotojo slaptažodį", ref _database.Password, password: true);
                Console.WriteLine("Gaunamos duomenų bazės lentelės...");
            }
            var tables = _database.GetTables();
            if (tables == null)
            {
                Console.Write("Paspauskite bet kokį mygtuką norint išnaujo įvesti duomenis");
                Console.ReadKey(true);
                ConnectToDatabase();
                return;
            }
            if (tables.Count == 0)
            {
                Write("Ši duomenų bazė neturi jokių lentelių. Pasirinkite kitą duomenų bazę! (Paspauskite bet kokį mygtuką noritn tęsti)", ConsoleColor.Red, true);
                Console.ReadKey(true);
                ConnectToDatabase();
                return;
            }
            _database.SelectedTable = tables[0];
            if (tables.Count == 1)
            {
                Write($"Ši duomenų bazė turi tik vieną lentelę pavadinimu '{_database.SelectedTable}', ar tikrai norite tęsti?", ConsoleColor.Yellow, true);
                if (CreateMenuSelector(0, new List<string> {
                    "Taip", "Ne"
                }) == 1)
                {
                    ConnectToDatabase();
                    return;
                }
            }
            else
            {
                _database.SelectedTable = tables[CreateMenuSelector(0, tables)];
            }
            var columns = _database.GetTableColumns(out var identityColumn);
            if (identityColumn == null)
            {
                _database.IdentityColumn = "id";
#pragma warning disable CS8601 // Possible null reference assignment.
                ReadLine("Įrašykite lentelės identifikavimo stulpelį (dažniausiai tai būna 'id')", ref _database.IdentityColumn);
#pragma warning restore CS8601 // Possible null reference assignment.
            }
            else
            {
                _database.IdentityColumn = identityColumn;
                Write($"Identifikacijos stulpelis ('{_database.IdentityColumn}') rastas automatiškai.", ConsoleColor.Green, true);
            }
            ReadLine("Įrašykite įrašo identifikacijos reikšmę (dažniausiai tai būna skaičius)", ref _database.IdentityValue);
            if (columns != null)
            {
                Console.WriteLine("Pasirinkite norimą stulpelį iš kurio bus paimtas tekstas.");
                var data = _database.SelectData(columns);
                columns.Remove(_database.IdentityColumn ?? "id");
                data?.Remove(_database.IdentityColumn ?? "id");
                Debug.WriteLine(data?.Count);
                var selected = CreateMenuSelector(0, data?.ToList()?.Select(x => $"{x.Key} ({x.Value})")?.ToList() ?? columns);
                _database.EditableColumn = columns[selected];
            } else
                ReadLine("Įrašykite įrašo stulpelį, iš kurio bus nuskaitomas (ar įrašomas) užšifruotas tekstas", ref _database.EditableColumn);
            var value = _database.GetRowColumnText();
            if (value == null)
            {
                Console.WriteLine("Nepavyko rasti stulpelio reikšmės...");
                var selected = CreateMenuSelector(0, new List<string> {
                    "Iš naujo įvesti duomenų bazės prisijungimas",
                    "Iš naujo įvesti lentelės duomenis"
                });
                switch (selected)
                {
                    case 0:
                        ConnectToDatabase();
                        return;
                    case 1:
                        ConnectToDatabase(true);
                        return;
                }
                ConnectToDatabase();
                return;
            }
            MiddleColorWrite("Dabartinė stulpelio reikšmė: ", value, null, ConsoleColor.Blue);
            Console.WriteLine("Paspauskite bet koki klavišą norint tęsti.");
            Console.ReadKey(true);
            ChooseEncryption();
        }

        private static void MiddleColorWrite(string? prefix, string text, string? suffix, ConsoleColor color)
        {
            if (prefix != null)
                Console.Write(prefix);
            Write(text, color, suffix == null);
            if (suffix != null)
                Console.WriteLine(suffix);
        }

        private static void Write(string text, ConsoleColor color, bool newLine = false)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            if (newLine)
                Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void ReadLine(string text, ref string? prevInput, string prefix = ">> ", bool password = false)
        {
            Console.Write(text);
            var display = prevInput ?? "";
            if (prevInput != null)
            {
                if (password)
                    display = string.Empty.PadRight(prevInput.Length, '*');
                Console.Write(" (");
                Write(display, ConsoleColor.Cyan);
                Console.Write(")");
            }
            Console.WriteLine();
            Console.Write(prefix);
            Console.ForegroundColor = ConsoleColor.Cyan;
            var input = "";
            if (password)
            {
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length == 0)
                            continue;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        Console.Write(" ");
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        input = input[..^1];
                        continue;
                    }
                    input += key.KeyChar;
                    Console.Write("*");
                }
            }
            else
            {
                input = Console.ReadLine()?.Trim() ?? "";
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (input.Length != 0)
                prevInput = input;
            else if (prevInput != null)
            {
                Console.SetCursorPosition(prefix.Length, Console.CursorTop - 1);
                Write(display, ConsoleColor.Cyan);
                Console.WriteLine(" (automatiškai)");
            }
            else if (prevInput == null)
            {
                Console.SetCursorPosition(0, Math.Max(0, Console.CursorTop - 2));
                ReadLine(text, ref prevInput, prefix);
            }
        }

        private static void SetTopPosition(int? currentPosition = null, int offset = 0)
        {
            Console.SetCursorPosition(0, (currentPosition ?? Console.CursorTop) - offset);
        }

        private static int CreateMenuSelector(int selected, List<string>? selections, int? currentPosition = null, int maxIndex = 0)
        {
            Console.CursorVisible = false;
            if (selections != null)
            {
                for (int i = 0; i < selections.Count; i++)
                {
                    Write(i == selected ? ">" : " ", ConsoleColor.Cyan);
                    var text = $" {i + 1}. {selections[i]}";
                    if (i + 1 == selections.Count)
                        Console.Write(text);
                    else
                        Console.WriteLine(text);
                }
                SetTopPosition(null, selections.Count - (selected + 1));
                maxIndex = selections.Count - 1;
            }
            currentPosition ??= Console.CursorTop;
            var key = Console.ReadKey(true);
            var pos = (int)(Console.CursorTop - currentPosition);
            if (key.Key == ConsoleKey.UpArrow)
                pos--;
            if (pos >= 0 && pos < maxIndex)
            {
                if (key.Key == ConsoleKey.DownArrow)
                {
                    SetTopPosition(currentPosition, -pos);
                    Console.Write(" ");
                    SetTopPosition(currentPosition, -pos - 1);
                    Write(">", ConsoleColor.Cyan);
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    SetTopPosition(currentPosition, -pos - 1);
                    Console.Write(" ");
                    SetTopPosition(currentPosition, -pos);
                    Write(">", ConsoleColor.Cyan);
                }
            }
            if (key.Key == ConsoleKey.Enter)
            {
                selected = (int) (Console.CursorTop - currentPosition);
                Console.SetCursorPosition(0, Console.CursorTop + (maxIndex - selected));
                Console.WriteLine();
                Console.CursorVisible = true;
                return selected;
            }
            return CreateMenuSelector(selected, null, currentPosition, maxIndex);
        }
    }
}