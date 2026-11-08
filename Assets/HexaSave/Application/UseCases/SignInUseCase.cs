using System;
using System.Threading.Tasks;
using HexaSave.Application.Ports;
using HexaSave.Domain.Shared;
 
namespace HexaSave.Application.UseCases
{
    /// <summary>
    /// Caso de uso: entrar no jogo (criar ou retomar a sessão do jogador).
    /// Mesmo sendo simples, existe para que o jogo NUNCA fale diretamente
    /// com um port de saída (decisão D14).
    /// </summary>
    public sealed class SignInUseCase
    {
        private readonly IAuthService _auth;
 
        public SignInUseCase(IAuthService auth)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }
 
        public async Task<PlayerId> ExecuteAsync()
        {
            return await _auth.SignInAsync();
        }
    }
}