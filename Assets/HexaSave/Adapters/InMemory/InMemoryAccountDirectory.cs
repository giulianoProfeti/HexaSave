using System;
using System.Collections.Generic;
using HexaSave.Domain.Shared;

namespace HexaSave.Adapters.InMemory
{
    /// <summary>
    /// O "servidor de contas" em memória. Dois InMemoryAccountService que compartilham o mesmo
    /// diretório simulam dois aparelhos falando com o mesmo serviço (ex.: PC e celular nos testes).
    /// </summary>
    public sealed class InMemoryAccountDirectory
    {
        // OrdinalIgnoreCase: como no UGS, "Player" e "player" são o MESMO nome de usuário.
        internal Dictionary<string, (string Password, PlayerId Player)> Accounts { get; } =
            new Dictionary<string, (string Password, PlayerId Player)>(StringComparer.OrdinalIgnoreCase);

        internal Dictionary<PlayerId, string> Usernames { get; } = new Dictionary<PlayerId, string>();
    }
}