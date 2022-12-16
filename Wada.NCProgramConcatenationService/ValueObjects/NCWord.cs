using System.Text.RegularExpressions;

namespace Wada.NCProgramConcatenationService.ValueObjects
{
    public interface INCWord { }

    public record class NCComment(string? Comment) : INCWord
    {
        public override string ToString() => $"({Comment})";
    }

    public record class NCWord(Address Address, ValueData ValueData) : INCWord;

    public record class Address
    {
        public Address(char value)
        {
            if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z]$"))
                throw new NCProgramConcatenationServiceException(nameof(value));

            Value = value;
        }
        public override string ToString() => Value.ToString();

        public char Value { get; init; }
    }

    public record class ValueData(decimal Value)
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
