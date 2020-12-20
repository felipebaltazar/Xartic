using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Xartic.Api.Domain.Models;
using Xartic.Api.Infrastructure.Abstractions;
using Xartic.Api.Infrastructure.Extensions;
using Xartic.Core;
using Xartic.Core.Extensions;

namespace Xartic.Api.Domain
{
    public sealed class GameController : IGameController
    {
        #region Fields

        private static readonly JsonSerializer _serializer = new JsonSerializer();

        private readonly IList<DrawCommand> _draws;
        private readonly Random _random;

        private bool isStarted;

        private string currentGameWord;
        private string roomName;
        private string host;

        #endregion

        #region Constructors

        public GameController()
        {
            _draws = new List<DrawCommand>();
            _random = new Random();
        }

        #endregion

        #region IGameController

        /// <inheritdoc/>
        public IEnumerable<DrawCommand> GetCurrentDraw() =>
            new ReadOnlyCollection<DrawCommand>(_draws);

        /// <inheritdoc/>
        public void OnDrawReceived(DrawCommand drawCommand)
        {
            if (!isStarted)
                return;

            _draws.Add(drawCommand);
        }

        /// <inheritdoc/>
        public void OnClearReceived()
        {
            if (!isStarted)
                return;

            _draws.Clear();
        }

        /// <inheritdoc/>
        public ResponseResult OnResponse(string response)
        {
            if (string.IsNullOrEmpty(response) || !isStarted)
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
            if (isClosest)
                return ResponseResult.IsClosest(response);

            return ResponseResult.Invalid(response);
        }

        /// <inheritdoc/>
        public string StartGame(string roomName, string host)
        {
            if (this.roomName == roomName && isStarted)
                return currentGameWord;

            this.host = host;
            this.roomName = roomName;
            isStarted = true;

            RandomizeGameWord();

            return currentGameWord;
        }

        /// <inheritdoc/>
        public bool OnPlayerDisconnected(string connectionId)
        {
            if (!isStarted)
                return false;

            if (connectionId == host)
            {
                isStarted = false;
                currentGameWord = string.Empty;
                roomName = string.Empty;
                host = string.Empty;

                _draws.Clear();
            }

            return isStarted;
        }

        #endregion

        #region PrivateMethods

        private void RandomizeGameWord()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var categoryJson = resources.GetResource($"{roomName}.json");
            var category = LoadCategory(categoryJson, assembly);

            var index = _random.Next(0, category.Words.Count - 1);

            currentGameWord = category.Words[index];
        }

        private static GameCategory LoadCategory(string categoryJson, Assembly assembly)
        {
            if (string.IsNullOrEmpty(categoryJson))
                throw new InvalidOperationException($"{categoryJson} file not found");

            using (var file = assembly.GetManifestResourceStream(categoryJson))
            {
                using (var streamReader = new StreamReader(file))
                {
                    using (var textReader = new JsonTextReader(streamReader))
                    {
                        return _serializer.Deserialize<GameCategory>(textReader);
                    }
                }
            }
        }

        #endregion
    }
}
