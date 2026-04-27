using ClosedXML.Excel;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class MoexTableParsing
    {
        public MoexTableParsing() 
        {

        }

        public void CreateTable(string jsonstring)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonstring);
            var root = doc.RootElement;
            Process(root);


        }

        void Process(JsonElement element)
        {
            

                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        // это объект — у него есть свойства, можно вызвать EnumerateObject()
                        foreach (JsonProperty prop in element.EnumerateObject())
                        {
                            Console.WriteLine($"Ключ: {prop.Name}");
                            Process(prop.Value);  // ← рекурсия: обрабатываем значение свойства
                        }
                        break;

                    case JsonValueKind.Array:
                        // это массив — у него есть элементы, можно вызвать EnumerateArray()
                        foreach (JsonElement item in element.EnumerateArray())
                        {
                            Process(item);  // ← рекурсия: обрабатываем каждый элемент
                        }
                        break;

                    default:
                        // это лист — простое значение, дальше спускаться некуда
                        Console.WriteLine($"Значение: {element}");
                        break;
                }
                
        }

    }
}
