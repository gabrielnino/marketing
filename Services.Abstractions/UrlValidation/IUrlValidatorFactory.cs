namespace Services.Abstractions.UrlValidation
{
    public interface IUrlValidatorFactory
    {
        IUrValidator GetValidator(string url);
    }
}
