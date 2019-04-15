using System;
using YamlDotNet;
using YamlDotNet.Serialization;
using System.IO;

namespace Semi {
    internal static class SerializationHelper {
        internal static ISerializer Serializer = new SerializerBuilder().Build();
        internal static IDeserializer Deserializer = new DeserializerBuilder().Build();

        internal static T Deserialize<T>(TextReader f) {
            return Deserializer.Deserialize<T>(f);
        }

        internal static T DeserializeFile<T>(string path) {
            using (var f = new StreamReader(File.OpenRead(path))) {
                return Deserializer.Deserialize<T>(f);
            }
        }
    }
}
