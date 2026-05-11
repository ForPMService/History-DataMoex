using History_DataMoex.Contracts.Dto.Algopack;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParsingAlgUtf8
    {
        public static List<CandlesDTO> ParseAlgCandles(ReadOnlySpan<byte> jsonBytes)
        {
            List<CandlesDTO> candlesList = new List<CandlesDTO>();
            Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);
            while (reader.Read()) 
            {
                JsonTokenType tokenType = reader.TokenType;
                switch (tokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (reader.ValueTextEquals("metadata"u8))
                        {
                            reader.Skip();
                        }
                        else if(reader.ValueTextEquals("columns"u8))
                        {
                           reader.Read();
                           int number = 0;
                            while (reader.Read()&&reader.TokenType != JsonTokenType.EndArray)
                            {
                                if(!reader.ValueTextEquals(ColumnAndNumbersForParsing.AlgCandlesExpectedColumns[number].Name))
                                {
                                    throw new Exception($"Unexpected column name: {reader.GetString()}");
                                }

                                number++;
                            }
                        }
                        else if(reader.ValueTextEquals("data"u8))
                        {
                            reader.Read();
                            
                            while(reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                double? open = null, close = null, high = null, low = null;
                                double? value = null, volume = null; 
                                 DateTime? begin = null, end = null;
                                if (reader.TokenType == JsonTokenType.StartArray)
                                {
                                    
                                    for (int i = 0; i < ColumnAndNumbersForParsing.AlgCandlesExpectedColumns.Length; i++)
                                    {
                                        reader.Read();
                                        switch (i)
                                        {
                                            case 0:
                                                open = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 1:
                                                close = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 2:
                                                high = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 3:
                                                low = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 4:
                                                value = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 5:
                                                volume = reader.TokenType == JsonTokenType.Null ? null : reader.GetDouble();
                                                break;
                                            case 6:
                                                begin = reader.TokenType == JsonTokenType.Null ? null : ParseHelpersUtf8.GetDateTimeOrNull(reader.GetString());
                                                break;
                                            case 7:
                                                end = reader.TokenType == JsonTokenType.Null ? null : ParseHelpersUtf8.GetDateTimeOrNull(reader.GetString());
                                                break;


                                        }
                                        
                                    }
                                    reader.Read();                                    
                                        candlesList.Add(new CandlesDTO
                                        {
                                            Open = open,
                                            Close = close,
                                            High = high,
                                            Low = low,
                                            Value = value,
                                            Volume = volume,
                                            Begin = begin,
                                            End = end
                                        });
                                    
                                }
                            }
                        }
                        break;


                }

            }

            return candlesList;
        }
    }
}
