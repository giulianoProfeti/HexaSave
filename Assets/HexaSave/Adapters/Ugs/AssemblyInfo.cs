using System.Runtime.CompilerServices;

// Permite que os testes EditMode enxerguem as classes "internal" deste assembly (mappers),
// sem torná-las públicas para o resto do projeto.
[assembly: InternalsVisibleTo("HexaSave.Tests.EditMode")]