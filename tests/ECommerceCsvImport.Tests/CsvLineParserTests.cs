using ECommerceCsvImport.Services;

namespace ECommerceCsvImport.Tests
{
    public class CsvLineParserTests
    {
        [Fact]
        public void Parse_SimpleLine_ReturnsAllFields()
        {
            var fields = CsvLineParser.Parse(
                "1001,Wi-Fi роутер Дом.ру Wave Plus,Роутер Wi-Fi 6,3990,RUB,Роутеры,25,/images/wave-plus.jpg,true,Дом.ру,12,Wave Plus AX3000");

            Assert.Equal(12, fields.Count);
            Assert.Equal("1001", fields[0]);
            Assert.Equal("Wave Plus AX3000", fields[11]);
        }

        [Fact]
        public void Parse_EmptyFields_AreKept()
        {
            var fields = CsvLineParser.Parse("1011,Кабель HDMI 2.0 1.5 м,,490,RUB,Аксессуары,50,,true,Cablexpert,12,CC-HDMI4-1.5M");

            Assert.Equal(12, fields.Count);
            Assert.Equal(string.Empty, fields[2]);
            Assert.Equal(string.Empty, fields[7]);
        }

        [Fact]
        public void Parse_CommaInsideQuotes_DoesNotSplitField()
        {
            var fields = CsvLineParser.Parse(
                "1012,\"Пульт Movix, голосовой\",Тестовая строка,1490,RUB,Аксессуары,1,/images/remote.jpg,true,Movix,12,MVX-RC01");

            Assert.Equal(12, fields.Count);
            Assert.Equal("Пульт Movix, голосовой", fields[1]);
        }

        [Fact]
        public void Parse_EscapedQuotes_AreUnescaped()
        {
            var fields = CsvLineParser.Parse(
                "1013,\"Название \"\"Special\"\"\",Описание,100,RUB,Категория,5,/img.jpg,true,Brand,12,Model");

            Assert.Equal(12, fields.Count);
            Assert.Equal("Название \"Special\"", fields[1]);
        }
    }
}
