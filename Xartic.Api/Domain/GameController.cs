using System;
using Xartic.Api.Domain.Models;
using Xartic.Api.Infrastructure.Extensions;

namespace Xartic.Api.Domain
{
    public sealed class GameController
    {
        //TODO: Tornar a seleção mais dinamica (talvez fazer um random de posição dentro de um json de palavras)
        private string currentGameWord = "Gato";

        /// <summary>
        /// Verifica o resultado de um "chute"
        /// </summary>
        /// <param name="response">String que representa um "chute" de um player</param>
        /// <returns>Resultado do "chute"</returns>
        public ResponseResult OnResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
                return ResponseResult.Invalid(response);

            var letters = currentGameWord.AsSpan();
            var msgLetters = response.AsSpan();

            var allMatch = msgLetters.Length == letters.Length;
            var matchCount = 0;

            for (int i = 0; i < msgLetters.Length; i++)
            {
                if (i >= letters.Length)
                {
                    allMatch = false;
                    break;
                }
                else if (letters[i].EqualsIgnoreCase(msgLetters[i]))
                {
                    matchCount++;
                }
            }

            allMatch &= letters.Length == matchCount;

            if (allMatch)
                return ResponseResult.AllMatch(response);

            var isClosest = matchCount >= (letters.Length / 2);
            if(isClosest)
                return ResponseResult.IsClosest(response);

            return ResponseResult.Invalid(response);
        }
    }
}
