using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Facial
{
    public class Face
    { 
        [JsonProperty("emotion")]
        public Dictionary<string, double> Emotions;

        public string CurrentEmotion()
        {
            var emotions = Emotions.OrderByDescending(x => x.Value);
            return emotions.FirstOrDefault().Key;

        }
    }
}