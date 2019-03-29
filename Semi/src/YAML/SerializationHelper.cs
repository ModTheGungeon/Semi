using System;
using YamlDotNet;
using YamlDotNet.Serialization;
using System.IO;

namespace Semi {
    public static class SerializationHelper {
        public static ISerializer Serializer = new SerializerBuilder().Build();
        public static IDeserializer Deserializer = new DeserializerBuilder().Build();

        public static T Deserialize<T>(TextReader f) {
            return Deserializer.Deserialize<T>(f);
        }

        public static T DeserializeFile<T>(string path) {
            using (var f = new StreamReader(File.OpenRead(path))) {
                return Deserializer.Deserialize<T>(f);
            }
        }
    }
}
