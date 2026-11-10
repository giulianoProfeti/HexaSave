namespace DemoGame.Composition
{
    /// <summary>
    /// Provedores que o composition root sabe montar.
    /// A Unity grava enums como números: os valores são explícitos e nunca devem ser reordenados.
    /// </summary>
    public enum SaveProvider
    {
        InMemory = 0,
        Local = 1,
    }
}