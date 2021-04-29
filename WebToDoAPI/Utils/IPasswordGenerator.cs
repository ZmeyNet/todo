namespace WebToDoAPI.Utils
{
    public interface IPasswordGenerator
    {
        public string Generate(int length, int numberOfNonAlphanumericCharacters);
    }
}
