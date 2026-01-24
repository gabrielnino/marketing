namespace Services.Abstractions.UrlValidation
{
    public interface IUrlFactory
    {
        IUrValidator GetValidator(string url);
    }
}
