using HexaSave.Application.Errors;

namespace HexaSave.Application.Ports
{
    /// <summary>
    /// Usuário e senha digitados pelo jogador. Não é um Value Object do domínio: é um dado
    /// de entrada transitório, que nunca é salvo nem comparado. Por isso não implementa igualdade.
    /// As regras de formato (tamanho, símbolos) são de cada provedor; aqui só se exige que existam.
    /// </summary>
    public readonly struct AccountCredentials
    {
        public string Username { get; }
        public string Password { get; }

        public AccountCredentials(string username, string password)
        {
            var trimmedUsername = username?.Trim();

            if (string.IsNullOrEmpty(trimmedUsername) || string.IsNullOrEmpty(password))
                throw new AccountException(AccountError.InvalidCredentials, "Informe o nome de usuário e a senha válidos.");

            Username = trimmedUsername;
            Password = password; // a senha não é aparada: espaços podem fazer parte dela
        }

        /// <summary>Nunca expõe a senha: ToString é o que aparece em logs e no depurador.</summary>
        public override string ToString() => $"AccountCredentials({Username}, senha oculta)";
    }
}