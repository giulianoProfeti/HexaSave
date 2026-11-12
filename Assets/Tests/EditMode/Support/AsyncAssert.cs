using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HexaSave.Tests.EditMode.Support
{
    /// <summary>
    /// Verifica exceções em código assíncrono SEM travar o editor.
    /// O Assert.ThrowsAsync do NUnit bloqueia a thread principal e pode congelar a Unity;
    /// a documentação da Unity recomenda usar try/catch.
    /// </summary>
    internal static class AsyncAssert
    {
        /// <returns>A exceção capturada (do tipo T ou derivada), para verificações adicionais.</returns>
        public static async Task<T> ThrowsAsync<T>(Func<Task> action) where T : Exception
        {
            try
            {
                await action();
            }
            catch (T expected)
            {
                return expected;
            }
            catch (Exception other)
            {
                Assert.Fail($"Esperava {typeof(T).Name}, mas ocorreu {other.GetType().Name}: {other.Message}");
            }

            Assert.Fail($"Esperava {typeof(T).Name}, mas nenhuma exceção foi lançada.");
            return null; // inalcançável: Assert.Fail sempre lança
        }
    }
}
