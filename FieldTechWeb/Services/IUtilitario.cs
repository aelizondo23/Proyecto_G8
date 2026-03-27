namespace FieldTechWeb.Services
{
    public interface IUtilitario
    {
        string Encrypt(string texto);
        string Decrypt(string texto);
    }
}
